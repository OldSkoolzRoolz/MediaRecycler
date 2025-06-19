// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using MediaRecycler.Modules.Interfaces;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;



namespace MediaRecycler.Modules.Interfaces
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

        private readonly ILogger _logger;

        private readonly IWebAutomationService _webAutomationService;
        private readonly string startingUrl;







        public BlogScraper(IEventAggregator aggregator, ILogger<MainForm> logger)
        {
            _webAutomationService = new PuppeteerAutomationService(_aggregator);

            //_scraperOptions = Scraping.
            _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator), "EventAggregator cannot be null.");
            startingUrl = ScrapingOptions.Default.StartingWebPage ?? throw new ArgumentNullException(nameof(ScrapingOptions.Default.StartingWebPage),
                        "StartingWebPage cannot be null. Please set it in the Scraping options.");
            _logger = logger;
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
                throw new InvalidOperationException("GroupingSelector is null. Check Scraping Options and try again.");

            try
            {
                //  await _webAutomationService.DoSiteLoginAsync().ConfigureAwait(false);

                //TODO: change this to use the starting URL from the options
                //Go to the first page of the archive for the single blog and filter to videos only
                // Note: Ensure that ScrapingOptions.Default.ArchivePageUrlSuffix is set correctly in the options.
                // This is not a recommended approach, as it assumes the archive page is always at this URL and hardcoded values.
                ReportStatus($"Loading page {ScrapingOptions.Default.StartingWebPage}...");

                await _webAutomationService.GoToAsync(ScrapingOptions.Default.StartingWebPage).ConfigureAwait(false);

                int pageNumber = 1;
                bool hasNextPage = true;


                await _webAutomationService.WaitForSelectorAsync(ScrapingOptions.Default.GroupingSelector!).ConfigureAwait(false);








                if (ScrapingOptions.Default.SinglePageScan)
                {
                    await ExtractPageLinksAsync().ConfigureAwait(false);
                }
                else
                {
                    while (hasNextPage)
                    {
                        await ExtractPageLinksAsync().ConfigureAwait(false);


                        // TODO: Selector needs refinement if it's selecting non-next links.
                        // Maybe look for specific text or 'rel=next' attribute if available.
                        // Example: "ul.pagination li:not(.disabled) a[rel='next']" or similar
                        hasNextPage = await _webAutomationService.IsElementVisibleAsync(ScrapingOptions.Default.PaginationSelector).ConfigureAwait(false);

                        if (hasNextPage)
                        {
                            _logger.LogInformation("Clicking next page....");

                            await _webAutomationService.ClickElementAsync(ScrapingOptions.Default.PaginationSelector).ConfigureAwait(false);
                            pageNumber++;

                            //  _aggregator.Publish(new PageNumberMessage(pageNumber));
                        }
                    } 
                }

                   


                ReportStatus($"Found {_collectedUrls.Count} video containing urls on {pageNumber} pages during scraping operations.");
                SaveCollectedUrls();
            }
            catch (Exception ex)
            {

                ReportStatus($"{ex.Message}\n{ex.StackTrace}");
                ReportStatus("BlogScraper::BeginScrapingTargetSiteAsync - An error occured during scraping.");
                SaveCollectedUrls(); //save what we have so far


            }
            finally
            {
                // Ensure the web automation service is disposed of properly.
                await _webAutomationService.DisposeAsync().ConfigureAwait(false);
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





        public Task CancelAsync()
        {
            throw new NotImplementedException();
        }







        public async ValueTask DisposeAsync()
        {
            if (_webAutomationService is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            else if (_webAutomationService is IDisposable disposable) disposable.Dispose();
        }







        public async Task DownloadCollectedLinksAsync()
        {
            string[] links = GetCollectedLinksFromFiles();
            UrlDownloader downloader = new(_aggregator);




            // --- Subscribe to Events ---
            downloader.DownloadCompleted += (sender, e) =>
            {
                _logger.LogInformation($"[SUCCESS] Downloaded: {e.Url}");
                _logger.LogInformation($"          -> Saved to: {e.FilePath}");
                _logger.LogInformation($"          -> Size: {e.FileSizeBytes / 1024.0:F2} KB");
            };

            downloader.DownloadFailed += (sender, e) =>
            {
                _logger.LogInformation($"[FAILURE] Failed: {e.Url}");
                _logger.LogInformation($"          -> Reason: {e.Exception.Message}");
            };

            downloader.QueueFinished += (sender, e) => { _logger.LogInformation("\n--- All downloads have been processed. ---"); };

            try
            {
                if (links.Length == 0) return;

                for (int a = 0; a <= links.Length; a++)
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

                await downloader.StartDownloadsAsync();


                _logger.LogInformation("Downloader module has finished. Cleaning up.");



            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "An unexpected error occured during downloading");

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

                if (File.Exists(filePath)) filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetRandomFileName() + ".txt");

                File.WriteAllLines(filePath, _collectedUrls);
                ReportStatus($"Successfully saved {_collectedUrls.Count} URLs to {filePath}.");
            }
            catch (Exception ex)
            {
                ReportStatus($"Failed to save collected URLs: {ex.Message}");
            }
        }

    }


}