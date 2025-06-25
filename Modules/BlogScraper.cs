// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using System.Collections.Concurrent;

using MediaRecycler.Model;
using MediaRecycler.Modules.Interfaces;
using MediaRecycler.Modules.Loggers;
using MediaRecycler.Modules.Options;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using PuppeteerSharp.Helpers;



namespace MediaRecycler.Modules;



    /// <summary>
    ///     Represents a scraper for extracting data from a blog site.
    ///     This class utilizes web automation services to navigate and scrape content from the target site.
    /// </summary>
    /// <remarks>
    ///     The <see cref="BlogScraper" /> class implements the <see cref="IBlogScraper" /> interface and provides
    ///     functionality for automating the process of navigating through a blog's archive pages, extracting relevant
    ///     data, and handling pagination. It relies on injected dependencies for web automation and event aggregation.
    /// </remarks>
    internal class BlogScraper : IBlogScraper
    {

        private readonly IEventAggregator _aggregator;
        private IWebAutomationService _webAutomationService;







        public BlogScraper(IEventAggregator aggregator)
        {

            _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator), "EventAggregator cannot be null.");
        }







        /// <summary>
        ///     Initiates the process of scraping the target blog site for content.
        /// </summary>
        /// <remarks>
        ///     This method navigates to the starting page of the blog's archive, iterates through the pages,
        ///     and collects URLs of relevant content based on the specified selectors. It handles pagination
        ///     and ensures that all available pages are processed until no further pages are detected.
        /// </remarks>
        /// <exception cref="Exception">
        ///     Thrown when an error occurs during the scraping process. The exception message is logged
        ///     for debugging purposes.
        /// </exception>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        public async Task BeginScrapingTargetBlogAsync()
        {
            if (string.IsNullOrWhiteSpace(ScrapingOptions.Default.GroupingSelector))
            {
                throw new InvalidOperationException("GroupingSelector is null. Check Scraping Options and try again.");
            }

            try
            {

                var AutomationService = new PuppeteerAutomationService(_aggregator);
                await AutomationService.InitializeAsync();
                _webAutomationService = AutomationService;

                //  await AutomationService.DoSiteLoginAsync().ConfigureAwait(false);

                //TODO: change this to use the starting URL from the options
                //Go to the first page of the archive for the single blog and filter to videos only
                // Note: Ensure that ScrapingOptions.Default.ArchivePageUrlSuffix is set correctly in the options.
                // This is not a recommended approach, as it assumes the archive page is always at this URL and hardcoded values.
                ReportStatus($"Loading page {ScrapingOptions.Default.StartingWebPage}...");

                await AutomationService.GoToAsync(ScrapingOptions.Default.StartingWebPage).WithTimeout(60000).ConfigureAwait(false);

                // Check if the page requires login
                string content = await AutomationService.GetPageContentsAsync();

                if (content.Contains("Please log in to view archive"))
                {
                    // If login is required, perform the login operation
                    await AutomationService.DoSiteLoginAsync().ConfigureAwait(false);
                    // After login, navigate to the starting page again
                    await AutomationService.GoToAsync(ScrapingOptions.Default.StartingWebPage).WithTimeout(60000).ConfigureAwait(false);
                }


                int pageNumber = 1;
                bool hasNextPage = true;
            int linkCount = 0;

                await AutomationService.WaitForSelectorAsync(ScrapingOptions.Default.GroupingSelector!).WithTimeout(60000).ConfigureAwait(false);

                while (hasNextPage)
                {

                    // This only collects the urls that contains videos not the actual video content.
                    var pageLinks = await AutomationService.GetNodeCollectionFromPageAsync(ScrapingOptions.Default.GroupingSelector!).ConfigureAwait(false);

                    foreach (var anchorHandle in pageLinks)
                    {
                        linkCount++;
                        var hrefProperty = await anchorHandle.GetPropertyAsync("href").ConfigureAwait(false);
                        string? url = await hrefProperty.JsonValueAsync<string>().ConfigureAwait(false);
                        DataLayer.InsertPostPageUrlToDb(url);
                    }
                    // TODO: Selector needs refinement if it's selecting non-next links.
                    // Maybe look for specific text or 'rel=next' attribute if available.
                    // Example: "ul.pagination li:not(.disabled) a[rel='next']" or similar
                    hasNextPage = await AutomationService.IsElementVisibleAsync(ScrapingOptions.Default.PaginationSelector).ConfigureAwait(false);

                    if (hasNextPage)
                    {
                        Program.Logger.LogInformation("Clicking next page....");

                        await AutomationService.ClickElementAsync(ScrapingOptions.Default.PaginationSelector).WithTimeout(60000).ConfigureAwait(false);
                        pageNumber++;
                        linkCount = 0;
                        //  _aggregator.Publish(new PageNumberMessage(pageNumber));
                    }
                }




                ReportStatus($"Found {linkCount} video containing urls on {pageNumber} pages during scraping operations.");



            }
            catch (Exception ex)
            {

                ReportStatus($"{ex.Message}");
                ReportStatus("BlogScraper::BeginScrapingTargetSiteAsync - An error occured during scraping.");

            }
            finally
            {
                await _webAutomationService.DisposeAsync();
            }
        }







        /// <summary>
        /// Extracts URLs from the page that contain videos, but does not retrieve the actual video content.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task ExtractPageLinksAsync()
        {
            // This only collects the urls that contains videos not the actual video content.
            var pageLinks = await _webAutomationService.GetNodeCollectionFromPageAsync(ScrapingOptions.Default.GroupingSelector!).ConfigureAwait(false);

            foreach (var anchorHandle in pageLinks)
            {
                var hrefProperty = await anchorHandle.GetPropertyAsync("href").ConfigureAwait(false);
                string? url = await hrefProperty.JsonValueAsync<string>().ConfigureAwait(false);
               DataLayer.InsertPostPageUrlToDb(url);
            }


        }




        public ValueTask CancelAsync()
        {
            Thread.Sleep(10000);
            return ValueTask.CompletedTask;
        }






    /// <summary>
    /// Disposes the <see cref="IWebAutomationService" />.
    /// </summary>
    /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            if (_webAutomationService is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
            else if (_webAutomationService is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }






        /// <summary>
        /// Attempts to extract video links from the collected URLs passed in as a param or from file that was saved during a previous application run.
        /// It adds the video links to a download queue and starts the download process.
        /// </summary>
        /// <returns></returns>
        /// <remarks>In the event of an abnormal application termination the download queue is saved to file at it's present state.</remarks>
        public async Task DownloadCollectedLinksAsync()
        {
            _aggregator.Publish(new StatusBarMessage("Downloader module is starting..."));
            _aggregator.Publish(new StatusMessage("Loading collected links.."));
            string[]? links = null;



            var db = new MRContext();
            var collectedUrls = await db.PostPages.ToListAsync();

            UrlDownloader downloader = new(_aggregator);

            try
            {
                var AutomationService = new PuppeteerAutomationService(_aggregator);
                await AutomationService.InitializeAsync();
                _webAutomationService = AutomationService;
            }
            catch (Exception ex)
            {
                ReportStatus("Fatal error creating control modules, restart application and try again.");
                _aggregator.Publish(new StatusBarMessage("Failed to create Automation Manager."));
                if (_webAutomationService != null)
                {
                    await _webAutomationService.DisposeAsync();
                }
                if (downloader != null)
                {
                    await downloader.DisposeAsync();
                }
                _aggregator.Publish(new StatusBarMessage($"Error initializing web automation service: {ex.Message}"));
                return;
            }


            // --- Subscribe to Events ---
            downloader.DownloadCompleted += (sender, e) =>
            {
                Program.Logger.LogInformation($"[SUCCESS] Downloaded: {e.Url}");
                Program.Logger.LogInformation($"          -> Saved to: {e.FilePath}");
                Program.Logger.LogInformation($"          -> Size: {e.FileSizeBytes / 1024.0:F2} KB");
            };

            downloader.DownloadFailed += (sender, e) =>
            {
                Program.Logger.LogInformation($"[FAILURE] Failed: {e.Url}");
                Program.Logger.LogInformation($"          -> Reason: {e.Exception.Message}");
            };

            downloader.QueueFinished += (sender, e) => { Program.Logger.LogInformation("\n--- All downloads have been processed. ---"); };

            try
            {
                if (collectedUrls.Count == 0)
                {
                    _aggregator.Publish(new StatusMessage("No links found to download. Please scrape first."));
                    Program.Logger.LogInformation("No links were loaded to process..");
                    return;
                }

                _aggregator.Publish(new StatusMessage("Extracting video links adding to queue..."));
                foreach (var link in collectedUrls)
                {
                    await _webAutomationService.GoToAsync(link.Link);

                    var sourceHandle = await _webAutomationService.QuerySelectorAsync(@"\video > source").ConfigureAwait(false);

                    if (sourceHandle != null)
                    {
                        string? videoLink = await sourceHandle?.GetPropertyAsync("src").Result.JsonValueAsync<string>();
                        downloader.QueueUrl(videoLink);
                        _aggregator.Publish(new QueueCountMessage(downloader.QueueCount));
                    
                        // Add the link to the internal collection
                        DataLayer.InsertTargetLinkToDb(link.PostId,link.Link);
                        
                        // Mark record as processed
                        await DataLayer.UpdatePostPageProcessedFlagAsync(link.PostId,true);
                    }
                }



            }
            catch (Exception e)
            {
                Program.Logger.LogInformation(e, "An unexpected error occured during downloading");

            }
            finally
            {
                await downloader.DisposeAsync();
            }
        }














        private void ReportStatus(string txt)
        {
            // _aggregator.Publish(new StatusMessage(txt));
            Program.Logger.LogInformation(txt);
        }








    }


