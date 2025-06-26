// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using MediaRecycler.Model;
using MediaRecycler.Modules.Interfaces;
using MediaRecycler.Modules.Loggers;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;



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







 private readonly IWebAutomationService _automationService;
    private readonly IDownloaderModule _downloaderModule;
    private readonly ILogger<BlogScraper> _logger;

    public BlogScraper(
        IEventAggregator aggregator,
        IWebAutomationService automationService,
        IDownloaderModule downloaderModule,
        ILogger<BlogScraper> logger)
    {
        _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator), "EventAggregator cannot be null.");
        _automationService = automationService ?? throw new ArgumentNullException(nameof(automationService), "AutomationService cannot be null.");
        _downloaderModule = downloaderModule ?? throw new ArgumentNullException(nameof(downloaderModule), "DownloaderModule cannot be null.");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
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


        _logger.LogInformation("GroupingSelector is null or empty. Check Scraping Options and try again.");
          //  throw new InvalidOperationException("GroupingSelector is null. Check Scraping Options and try again.");
        

        try
        {
            ReportStatus($"Navigating to initial page: {ScrapingOptions.Default.StartingWebPage}...");
            await NavigateAndLoginIfNeededAsync(ScrapingOptions.Default.StartingWebPage);

            int totalLinksFound = 0;
            int pageNumber = 1;

            while (true)
            {
                _logger.LogInformation("Processing page number: {PageNumber}", pageNumber);
                int linksOnCurrentPage = await ScrapeCurrentPageLinksAsync(pageNumber);
                totalLinksFound += linksOnCurrentPage;

                ReportStatus($"Found {linksOnCurrentPage} links on page {pageNumber}. Total links found: {totalLinksFound}.");

                bool hasNextPage = await HandlePaginationAsync();
                if (!hasNextPage)
                {
                    _logger.LogInformation("No more pagination links found. Scraping finished.");
                    break;
                }

                pageNumber++;
            }
        }
        catch (NavigationException navEx)
        {
            _logger.LogError(navEx, "Navigation error during scraping: {Message}", navEx.Message);
            ReportStatus($"Navigation error: {navEx.Message}");
        }
        catch (WaitTaskTimeoutException timeoutEx)
        {
            _logger.LogError(timeoutEx, "Timeout waiting for element during scraping: {Message}", timeoutEx.Message);
            ReportStatus($"Timeout error: {timeoutEx.Message}");
        }
        catch (Exception ex)
        {
           
            _logger.LogError(ex, "An unexpected error occurred during scraping: {Message}", ex.Message);
            ReportStatus($"An unexpected error occurred: {ex.Message}");
        }
        finally
        {
          
            // The automation service's disposal should be handled by the DI container
            // or at a higher level if it's a singleton/scoped instance.
            // If it's transient and created per operation, then dispose here.
            // Given the current setup, it's injected, so its lifecycle is external.
            // However, if the IWebAutomationService implementation (PuppeteerAutomationService)
            // manages an IPage, that page might need to be closed.
            // For now, assuming the injected service handles its internal resources.
            // If PuppeteerAutomationService is transient, then:
            // await _automationService.DisposeAsync();
        }
    }





 /// <summary>
    /// Navigates to the specified URL and performs login if required.
    /// </summary>
    /// <param name="url">The URL to navigate to.</param>
    private async Task NavigateAndLoginIfNeededAsync(string url)
    {
        await _automationService.GoToAsync(url);

        // Check for login requirement by looking for a specific element or content
        // This is more robust than checking page content string which might change.
        // Assuming 'input#email' is a good indicator for the login page.
        bool requiresLogin = await _automationService.IsElementVisibleAsync("input#email");

        if (requiresLogin)
        {
            _logger.LogInformation("Login required. Attempting to log in...");
            ReportStatus("Login required. Attempting to log in...");
            await _automationService.DoSiteLoginAsync().ConfigureAwait(false);

            // After login, navigate to the starting page again to ensure we are on the correct page
            _logger.LogInformation("Login successful. Navigating back to {Url}...", url);
            await _automationService.GoToAsync(url);
        }
        else
        {
            _logger.LogInformation("No login required for {Url}.", url);
        }
    }











/// <summary>
    /// Scrapes links from the current page and inserts them into the database.
    /// </summary>
    /// <param name="pageNumber">The current page number for logging.</param>
    /// <returns>The number of links found on the current page.</returns>
    private async Task<int> ScrapeCurrentPageLinksAsync(int pageNumber)
    {
        int linksFoundOnPage = 0;
        try
        {
            // Ensure the page is loaded and our grouping selector is ready
            await _automationService.WaitForSelectorAsync(ScrapingOptions.Default.GroupingSelector);

            // Grab the group selector elements for iteration
            var pageLinks = await _automationService.GetNodeCollectionFromPageAsync(ScrapingOptions.Default.GroupingSelector);

            if (pageLinks == null || pageLinks.Length == 0)
            {
                _logger.LogWarning("No elements found with selector '{Selector}' on page {PageNumber}.", ScrapingOptions.Default.GroupingSelector, pageNumber);
                return 0;
            }

            // Iterate the elements we have found.
            foreach (var anchorHandle in pageLinks)
            {
                try
                {
                    // Get the property on the element we are looking for
                    var hrefProperty = await anchorHandle.GetPropertyAsync("href");

                    // Get the value of the property we searched for.
                    string? url = await hrefProperty.JsonValueAsync<string>();

                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        // Do something with the value. --> Storage..
                        // Consider batching these inserts for performance if DataLayer supports it.
                        DataLayer.InsertPostPageUrlToDb(url);
                        linksFoundOnPage++;
                        _logger.LogTrace("Extracted and queued link: {Url}", url);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error extracting href from an element on page {PageNumber}. Skipping element.", pageNumber);
                }
                finally
                {
                    // Dispose the element handle to free up Puppeteer resources
                    await anchorHandle.DisposeAsync();
                }
            }
            _logger.LogInformation("Finished scraping {LinksFound} links on page {PageNumber}.", linksFoundOnPage, pageNumber);
        }
        catch (WaitTaskTimeoutException timeoutEx)
        {
            _logger.LogError(timeoutEx, "Timeout waiting for selector '{Selector}' on page {PageNumber}.", ScrapingOptions.Default.GroupingSelector, pageNumber);
            throw; // Re-throw to be caught by the main scraping loop
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping links on page {PageNumber}.", pageNumber);
            throw; // Re-throw to be caught by the main scraping loop
        }
        return linksFoundOnPage;
    }










/// <summary>
    /// Handles pagination by clicking the next page link.
    /// </summary>
    /// <returns>True if a next page link was found and clicked, false otherwise.</returns>
    private async Task<bool> HandlePaginationAsync()
    {
        try
        {
            var paginationElement = await _automationService.QuerySelectorAsync(ScrapingOptions.Default.PaginationSelector);

            if (paginationElement != null)
            {
                _logger.LogInformation("Found pagination element. Clicking to navigate to next page...");
                await paginationElement.ClickAsync();
                // Wait for navigation to complete after clicking.
                // PuppeteerAutomationService.ClickElementAsync already handles navigation wait.
                // If ClickElementAsync doesn't wait for navigation, add it here.
                // For now, assuming ClickAsync is enough or next WaitForSelectorAsync will handle it.
                // A more robust approach would be to wait for a specific URL change or network idle.
                // await _automationService.Page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });
                return true;
            }
            else
            {
                _logger.LogInformation("No pagination element found with selector '{Selector}'.", ScrapingOptions.Default.PaginationSelector);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling pagination with selector '{Selector}'.", ScrapingOptions.Default.PaginationSelector);
            return false; // Stop pagination on error
        }
    }




























































    public ValueTask CancelAsync()
    {
        // If there are ongoing Puppeteer operations that can be cancelled,
        // this is where you'd pass a CancellationToken to them.
        // For now, it's a placeholder.
        _logger.LogInformation("Scraping cancellation requested.");
        return ValueTask.CompletedTask;
    }






    /// <summary>
    /// Disposes the <see cref="IWebAutomationService" />.
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
       




 // The injected services should be disposed by the DI container
        // or at the application shutdown level.
        // This class itself doesn't own the lifecycle of _automationService or _downloaderModule.
        // If this class *did* own them (e.g., created them in its constructor), then they would be disposed here.
        // For now, just call CancelAsync if there's any ongoing cancellable work.
        await CancelAsync();



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





        _logger.LogInformation("Starting download of collected links.");

        await using var db = new MRContext();

       
        // Use the injected downloader module
        _downloaderModule.DownloadQueCountUpdated += (sender, message) => _aggregator.Publish(new QueueCountMessage(int.Parse(message)));
        _downloaderModule.StatusUpdated += (sender, message) => _aggregator.Publish(new StatusMessage(message));

        var linksToDownload = db.TargetLinks.Where(p => !p.IsDownloaded).Select(p => new Uri(p.Link)).ToList();
        foreach (var link in linksToDownload)
        {
            _downloaderModule.EnqueueUrl(link);
        }

        _logger.LogInformation("Queued {Count} links for download.", linksToDownload.Count);
        _aggregator.Publish(new StatusBarMessage($"Queued {linksToDownload.Count} links for download."));

        // --- Subscribe to Events ---

        // These events should ideally be handled by a dedicated logging/event service,
        // not directly within the scraper, but for now, we'll keep them here.
        // The UrlDownloader events are different from DownloaderModule events.
        // The original code used UrlDownloader, but now we're using DownloaderModule.
        // Let's assume DownloaderModule has similar events or we adapt.
        // Looking at DownloaderModule.cs, it has StatusUpdated and DownloadQueCountUpdated.
        // It does *not* have DownloadCompleted or DownloadFailed events like UrlDownloader.
        // This means the logging for individual download success/failure needs to be handled
        // within DownloaderModule itself or by subscribing to its internal logging.
        // For now, I'll remove the specific DownloadCompleted/Failed subscriptions here
        // and rely on DownloaderModule's internal logging.

        try
        {
            _downloaderModule.Start();
            await _downloaderModule.WaitForDownloadsAsync();
            _logger.LogInformation("All downloads have finished.");
            _aggregator.Publish(new StatusBarMessage("Downloads have finished."));
        }
        catch (Exception e)
        {
            Program.Logger.LogInformation(e, "An unexpected error occured during downloading");

            _logger.LogError(e, "An unexpected error occurred during downloading: {Message}", e.Message);
            _aggregator.Publish(new StatusMessage($"Download error: {e.Message}"));
        }
        finally
        {
           
            // The injected downloader module's disposal should be handled by the DI container.
            // If it's transient, it might be disposed here, but typically it's scoped or singleton.
            // For now, assuming external lifecycle management.
            // If it needs to be explicitly stopped/disposed by this class:
            // await _downloaderModule.StopAsync();
            // await _downloaderModule.DisposeAsync();
        }
    }





    /// <summary>
    ///     Extracts target links from a list of unprocessed post pages.
    /// </summary>
    /// <remarks>
    ///     This function iterates through a list of post pages that have not been processed,
    ///     extracts the video link from each page, and adds it to the internal collection.
    ///     It also marks each processed page as processed in the database.
    /// </remarks>
    public async Task ExtractTargetLinksAsync()
    {

         _logger.LogInformation("Starting extraction of target links.");
        await using var db = new MRContext();
        var startingList = db.PostPages.Where(p => p.IsProcessed == false).ToList();
        await using var _webAuto = new PuppeteerAutomationService(_aggregator);

        //Starts browser and sets up vars
        await _webAuto.InitializeAsync();


        _aggregator.Publish(new StatusBarMessage("Started extracting target links."));
        foreach (var page in startingList)
        {
            try
            {


                _logger.LogDebug("Navigating to post page: {Link}", page.Link);
                await _automationService.GoToAsync(page.Link);

                // Wait for the video source element to be present
                await _automationService.WaitForSelectorAsync(@"video > source");

                var sourceHandle = await _automationService.QuerySelectorAsync(@"video > source").ConfigureAwait(false);
                if (sourceHandle == null)
                {
                    _logger.LogWarning("Video source element not found on page: {Link}", page.Link);
                    continue;
                }

                string? videoLink = await sourceHandle.GetPropertyAsync("src").Result.JsonValueAsync<string>();

                if (!string.IsNullOrWhiteSpace(videoLink))
                {
                    _logger.LogInformation("Extracted video link: {VideoLink} from page: {PostPageLink}", videoLink, page.Link);
                    await DataLayer.InsertTargetLinkAndMarkPageAsProcessedAsync(page.PostId, videoLink);
                }
                else
                {
                    _logger.LogWarning("Extracted video link was null or empty from page: {Link}", page.Link);
                }

            }
            catch (WaitTaskTimeoutException timeoutEx)
            {

                _logger.LogError(timeoutEx, "Timeout waiting for video source on page {Link}. Marking as processed to avoid re-attempting.", page.Link);
                // Mark as processed to avoid getting stuck on this page
                await DataLayer.MarkPostPageAsProcessedAsync(page.PostId);

            }
            catch (Exception ex)
            {

                // Log the error and continue with the next page
                //_logger.LogError(ex, "Error extracting target link for page {LinkId}", page.Link);
                continue;
                _logger.LogError(ex, "Error extracting target link for page {Link}. Marking as processed to avoid re-attempting.", page.Link);
                // Mark as processed to avoid getting stuck on this page
                await DataLayer.MarkPostPageAsProcessedAsync(page.PostId);
            }
        }
        _aggregator.Publish(new StatusBarMessage("Finished extracting target links."));


    }















    private void ReportStatus(string txt)
    {
         _aggregator.Publish(new StatusMessage(txt));
        _logger.LogInformation(txt);
    }








}


