// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;



namespace MediaRecycler.Modules;


internal interface IBlogScraper
{

    ValueTask DisposeAsync();

    Task BeginScrapingTargetBlogAsync();

}


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






    public BlogScraper(IWebAutomationService webAutomationService, IEventAggregator aggregator, ILogger<MainForm> logger)
    {
        _webAutomationService = webAutomationService ?? throw new ArgumentNullException(nameof(webAutomationService));

        //_scraperOptions = Scraping.
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator), "EventAggregator cannot be null.");
        startingUrl = ScrapingOptions.Default.StartingWebPage ?? throw new ArgumentNullException(nameof(ScrapingOptions.Default.StartingWebPage), "StartingWebPage cannot be null. Please set it in the Scraping options.");
        _logger = logger;
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
            //  await _webAutomationService.DoSiteLoginAsync().ConfigureAwait(false);

            //TODO: change this to use the starting URL from the options
            //Go to the first page of the archive for the single blog and filter to videos only
            // Note: Ensure that ScrapingOptions.Default.ArchivePageUrlSuffix is set correctly in the options.
            // This is not a recommended approach, as it assumes the archive page is always at this URL and hardcoded values.
            await _webAutomationService.GoToAsync(ScrapingOptions.Default.StartingWebPage + ScrapingOptions.Default.ArchivePageUrlSuffix + "?page=85").ConfigureAwait(false);
            ReportStatus($"Loading page {ScrapingOptions.Default.StartingWebPage}...");

            int pageNumber = 1;
            bool hasNextPage = true;

            await _webAutomationService.WaitForSelectorAsync(ScrapingOptions.Default.GroupingSelector!).ConfigureAwait(false);

            while (hasNextPage)
            {

                // This only collects the urls that contains videos not the actual video content.
                var pageLinks = await _webAutomationService.GetNodeCollectionFromPageAsync(ScrapingOptions.Default.GroupingSelector!).ConfigureAwait(false);

                foreach (var anchorHandle in pageLinks)
                {
                    var hrefProperty = await anchorHandle.GetPropertyAsync("href").ConfigureAwait(false);
                    var url = await hrefProperty.JsonValueAsync<string>().ConfigureAwait(false);
                    _ = _collectedUrls.Add(url);
                }

                ReportStatus($"Collected {pageLinks.Length} on page {pageNumber}");


                // TODO: Selector needs refinement if it's selecting non-next links.
                // Maybe look for specific text or 'rel=next' attribute if available.
                // Example: "ul.pagination li:not(.disabled) a[rel='next']" or similar
                hasNextPage = await _webAutomationService.IsElementVisibleAsync(ScrapingOptions.Default.PaginationSelector).ConfigureAwait(false);

                if (hasNextPage)
                {
                    ReportStatus("Clicking next page....");
                    await _webAutomationService.ClickElementAsync(ScrapingOptions.Default.PaginationSelector).ConfigureAwait(false);
                    pageNumber++;

                    //  _aggregator.Publish(new PageNumberMessage(pageNumber));
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

            if (File.Exists(filePath))
            {
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetRandomFileName() + ".txt");
            }

            File.WriteAllLines(filePath, _collectedUrls);
            ReportStatus($"Successfully saved {_collectedUrls.Count} URLs to {filePath}.");
        }
        catch (Exception ex)
        {
            ReportStatus($"Failed to save collected URLs: {ex.Message}");
        }
    }






    private void ReportStatus(string txt)
    {
        // _aggregator.Publish(new StatusMessage(txt));
        Program.Logger.LogInformation(txt);
    }






    public async Task DownloadCollectLinksAsync()
    {
        string[] links = GetCollectedLinksFromFiles();
        UrlDownloader downloader = new(DownloaderOptions.Default.MaxConcurrency, DownloaderOptions.Default.DownloadPath);




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

        downloader.QueueFinished += (sender, e) =>
        {
            _logger.LogInformation("\n--- All downloads have been processed. ---");
        };

        try
        {
            if (links.Length == 0)
            {
                return;
            }

            foreach (string link in links)
            {
                await _webAutomationService.GoToAsync(link);


                IElementHandle? sourceHandle = await _webAutomationService.QuerySelectorAsync(@"\video > source").ConfigureAwait(false);

                if (sourceHandle != null)
                {
                    var videoLink = await sourceHandle?.GetPropertyAsync("src").Result.JsonValueAsync<string>();
                    downloader.QueueUrl(videoLink);
                }



            }

            await downloader.StartDownloadsAsync();


            _logger.LogInformation("Downloader module has finished. Cleaning up.");



        }
        catch (Exception e)
        {
            _logger.LogInformation(e.Message);
            throw;
        }
        finally
        {
            downloader.Dispose();
        }
    }






    private string[] GetCollectedLinksFromFiles()
    {
        var directory = AppDomain.CurrentDomain.BaseDirectory;
        var txtFiles = Directory.GetFiles(directory, "*.txt", SearchOption.TopDirectoryOnly);
        var allLines = new List<string>();

        foreach (var file in txtFiles)
        {
            try
            {
                var lines = File.ReadAllLines(file);
                allLines.AddRange(lines);
            }
            catch (Exception ex)
            {
                _aggregator?.Publish($"Error reading file '{file}': {ex.Message}");
            }
        }

        return allLines.ToArray();
    }

}
