// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using MediaRecycler.Modules.Interfaces;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;

using PuppeteerSharp.Helpers;



namespace MediaRecycler.Modules
{


}


namespace MediaRecycler.Modules
{


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

        private readonly HashSet<string> _collectedUrls = [];


        private readonly string startingUrl;
        private IWebAutomationService _webAutomationService;







        public BlogScraper(IEventAggregator aggregator)
        {

            _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator), "EventAggregator cannot be null.");
            startingUrl = ScrapingOptions.Default.StartingWebPage ?? throw new ArgumentNullException(nameof(ScrapingOptions.Default.StartingWebPage),
                        "StartingWebPage cannot be null. Please set it in the Scraping options.");
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


                await AutomationService.WaitForSelectorAsync(ScrapingOptions.Default.GroupingSelector!).WithTimeout(60000).ConfigureAwait(false);

                while (hasNextPage)
                {

                    // This only collects the urls that contains videos not the actual video content.
                    var pageLinks = await AutomationService.GetNodeCollectionFromPageAsync(ScrapingOptions.Default.GroupingSelector!).ConfigureAwait(false);

                    foreach (var anchorHandle in pageLinks)
                    {
                        var hrefProperty = await anchorHandle.GetPropertyAsync("href").ConfigureAwait(false);
                        string? url = await hrefProperty.JsonValueAsync<string>().ConfigureAwait(false);
                        _ = _collectedUrls.Add(url);
                    }
                    // TODO: Selector needs refinement if it's selecting non-next links.
                    // Maybe look for specific text or 'rel=next' attribute if available.
                    // Example: "ul.pagination li:not(.disabled) a[rel='next']" or similar
                    hasNextPage = await AutomationService.IsElementVisibleAsync(ScrapingOptions.Default.PaginationSelector).ConfigureAwait(false);

                    if (hasNextPage)
                    {
                        Program.Logger.LogInformation("Clicking next page....");

                        await AutomationService.ClickElementAsync(ScrapingOptions.Default.PaginationSelector).ConfigureAwait(false);
                        pageNumber++;

                        //  _aggregator.Publish(new PageNumberMessage(pageNumber));
                    }
                }




                ReportStatus($"Found {_collectedUrls.Count} video containing urls on {pageNumber} pages during scraping operations.");
                SaveCollectedUrls();

                ReportStatus("Starting to extract and download video urls.");
                _aggregator.Publish(new StatusBarMessage("Downloading collected links is starting..."));
                
                
                // Now we have all the links, we can start downloading them.
                await DownloadCollectedLinksAsync(_collectedUrls);

            }
            catch (Exception ex)
            {

                ReportStatus($"{ex.Message}");
                ReportStatus("BlogScraper::BeginScrapingTargetSiteAsync - An error occured during scraping.");
                SaveCollectedUrls(); //save what we have so far

            }
            finally
            {
                await _webAutomationService.DisposeAsync();
            }
        }








        private async Task ExtractPageLinksAsync()
        {
            // This only collects the urls that contains videos not the actual video content.
            var pageLinks = await _webAutomationService.GetNodeCollectionFromPageAsync(ScrapingOptions.Default.GroupingSelector!).ConfigureAwait(false);

            foreach (var anchorHandle in pageLinks)
            {
                var hrefProperty = await anchorHandle.GetPropertyAsync("href").ConfigureAwait(false);
                string? url = await hrefProperty.JsonValueAsync<string>().ConfigureAwait(false);
                _ = _collectedUrls.Add(url);
            }


        }





        public ValueTask CancelAsync()
        {
            Thread.Sleep(10000);
            return ValueTask.CompletedTask;
        }







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
        /// <param name="collectedUrls"></param>
        /// <returns></returns>
        /// <remarks>In the event of an abnormal application termination the download queue is saved to file at it's present state.</remarks>
        public async Task DownloadCollectedLinksAsync(HashSet<string>? collectedUrls = null)
        {
            TaskCompletionSource tcs = new();
            _aggregator.Publish(new StatusBarMessage("Downloader module is starting..."));
            _aggregator.Publish(new StatusMessage("Loading collected links.."));
            string[]? links = null;
            

            if(collectedUrls == null || collectedUrls.Count == 0)
            {
                //We weren't passed links, so we will try and load them from the files.
                links = GetCollectedLinksFromFiles();

                if (links.Length == 0)
                {

                }
            }
            
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
                tcs.SetException(ex);
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
                if (links.Length == 0)
                {
                    _aggregator.Publish(new StatusMessage("No links found to download. Please scrape first."));
                    Program.Logger.LogInformation("No links were loaded to process..");
                    return;
                }

                _aggregator.Publish(new StatusMessage("Extracting video links adding to queue..."));
                for (int a = 0; a <= links.Length - 1; a++)
                {
                    await _webAutomationService.GoToAsync(links[a]);
                    var sourceHandle = await _webAutomationService.QuerySelectorAsync(@"\video > source").ConfigureAwait(false);

                    if (sourceHandle != null)
                    {
                        string? videoLink = await sourceHandle?.GetPropertyAsync("src").Result.JsonValueAsync<string>();
                        downloader.QueueUrl(videoLink);
                        _aggregator.Publish(new QueueCountMessage(downloader.QueueCount));
                    }
                }

                // --- Start the downloader ---
                _aggregator.Publish(new StatusMessage("Starting Downloader......."));
                await downloader.StartDownloadsAsync();


                Program.Logger.LogInformation("Downloader module has finished. Cleaning up.");


                _aggregator.Publish(new StatusMessage("Downloader finished.."));
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







        /// <summary>
        ///     Retrieves all collected links from text files located in the application's base directory.
        /// </summary>
        /// <remarks>
        ///     This method searches for all text files (*.txt) in the application's base directory, reads their contents,
        ///     and aggregates all lines into a single array of strings. If an error occurs while reading a file,
        ///     a status message is published using the event aggregator.
        /// </remarks>
        /// <returns>
        ///     An array of strings containing all the lines collected from the text files.
        /// </returns>
        /// <exception cref="IOException">
        ///     Thrown if an I/O error occurs while accessing the files.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     Thrown if the application does not have permission to access the files.
        /// </exception>
        private string[] GetCollectedLinksFromFiles()
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            string[] txtFiles = Directory.GetFiles(directory, "*.txt", SearchOption.TopDirectoryOnly);
            var allLines = new List<string>();

            foreach (string file in txtFiles)
            {
                try
                {
                    string[] lines = File.ReadAllLines(file);
                    allLines.AddRange(lines);
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    _aggregator?.Publish(new StatusMessage($"Error reading file '{file}': {ex.Message}"));
                }
            }

            return allLines.ToArray();
        }







        private void ReportStatus(string txt)
        {
            // _aggregator.Publish(new StatusMessage(txt));
            Program.Logger.LogInformation(txt);
        }







        /// <summary>
        ///     Saves the collected URLs to a file in the application's base directory.
        /// </summary>
        /// <remarks>
        ///     This method writes all URLs stored in the internal collection to a file named "CollectedUrls.txt".
        ///     If the operation is successful, a status message is logged indicating the number of URLs saved and the file path.
        ///     In case of an error during the save operation, an error message is logged with the exception details.
        /// </remarks>
        /// <exception cref="Exception">
        ///     Thrown if an error occurs while attempting to write the URLs to the file.
        /// </exception>
        private void SaveCollectedUrls()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CollectedUrls.txt");



                File.AppendAllLines(filePath, _collectedUrls);

                ReportStatus($"Successfully saved {_collectedUrls.Count} URLs to {filePath}.");
            }
            catch (Exception ex)
            {
                ReportStatus($"Failed to save collected URLs: {ex.Message}");
            }
        }

    }


}