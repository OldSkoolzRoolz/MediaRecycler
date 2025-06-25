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

            ReportStatus($"Loading page {ScrapingOptions.Default.StartingWebPage}...");

            await AutomationService.GoToAsync(ScrapingOptions.Default.StartingWebPage);

            // Check if the page requires login
            string content = await AutomationService.GetPageContentsAsync();

            if (content.Contains("Please log in to view archive"))
            {
                // If login is required, perform the login operation
                await AutomationService.DoSiteLoginAsync().ConfigureAwait(false);
                // After login, navigate to the starting page again
            }
            await AutomationService.GoToAsync(ScrapingOptions.Default.StartingWebPage);


            int pageNumber = 1;
            bool hasNextPage = true;
            int linkCount = 0;

            //ReportStatus("Waiting for selector to load....");

            while (hasNextPage)
            {

                // This only collects the urls that contains videos not the actual video content.
                //var pageLinks = await AutomationService.GetNodeCollectionFromPageAsync(ScrapingOptions.Default.GroupingSelector!).ConfigureAwait(false);
                await AutomationService.Page.WaitForSelectorAsync(ScrapingOptions.Default.GroupingSelector);
                var pageLinks = await AutomationService.Page.QuerySelectorAllAsync(ScrapingOptions.Default.GroupingSelector).ConfigureAwait(false);



                foreach (var anchorHandle in pageLinks)
                {
                    linkCount++;
                    var hrefProperty = await anchorHandle.GetPropertyAsync("href").ConfigureAwait(false);
                    string? url = await hrefProperty.JsonValueAsync<string>().ConfigureAwait(false);
                    DataLayer.InsertPostPageUrlToDb(url);
                }
                
                hasNextPage = await AutomationService.IsElementVisibleAsync(ScrapingOptions.Default.PaginationSelector).ConfigureAwait(false);

                if (hasNextPage)
                {
                    Program.Logger.LogInformation("Clicking next page....");

                    await AutomationService.ClickElementAsync(ScrapingOptions.Default.PaginationSelector);
                    pageNumber++;
                    ReportStatus($"Found {linkCount} video containing urls on page# {pageNumber} during scraping operations.");
                    linkCount = 0;
                    //  _aggregator.Publish(new PageNumberMessage(pageNumber));
                }
            }



            await using var db = new MRContext();

            var pages = db.PostPages.Where(p => !p.IsProcessed).ToList();


            foreach (var page in pages)
            {
                await _webAutomationService.GoToAsync(page.Link);
                await _webAutomationService.WaitForSelectorAsync(@"video > source");
                var sourceHandle = await _webAutomationService.QuerySelectorAsync(@"\video > source").ConfigureAwait(false);
                if (sourceHandle == null) continue;
                string? videoLink = await sourceHandle?.GetPropertyAsync("src").Result.JsonValueAsync<string>()!;

                // Add the link to the internal collection
                DataLayer.InsertTargetLinkToDb(page.PostId, videoLink);

                // Mark record as processed
                await DataLayer.UpdatePostPageProcessedFlagAsync(page.PostId, true);
            }







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









    public ValueTask CancelAsync()
    {
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




        await using var db = new MRContext();

        UrlDownloader downloader = new(_aggregator);
        downloader.QueueUrls(db.TargetLinks.Where(p => p.IsDownloaded == false).Select(p => p.Link));
        _aggregator.Publish(new QueueCountMessage(downloader.QueueCount));

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

            await downloader.StartDownloadsAsync().ConfigureAwait(false);

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
    ///     Extracts target links from a list of unprocessed post pages.
    /// </summary>
    /// <remarks>
    ///     This function iterates through a list of post pages that have not been processed,
    ///     extracts the video link from each page, and adds it to the internal collection.
    ///     It also marks each processed page as processed in the database.
    /// </remarks>
    public async Task ExtractTargetLinksAsync()
    {
        await using var db = new MRContext();
        var startingList = db.PostPages.Where(p => p.IsProcessed == false).ToList();
        var _webAuto = new PuppeteerAutomationService(_aggregator);
        await _webAuto.InitializeAsync();
        _webAutomationService = _webAuto;

        try
        {
            _aggregator.Publish(new StatusBarMessage("Started extracting target links."));
            foreach (var page in startingList)
            {
                try
                {
                    await _webAuto.GoToAsync(page.Link);
                    await _webAutomationService.WaitForSelectorAsync(@"video > source");
                    var sourceHandle = await _webAutomationService.QuerySelectorAsync(@"video > source").ConfigureAwait(false);
                    if (sourceHandle == null) continue;
                    string? videoLink = await sourceHandle.GetPropertyAsync("src").Result.JsonValueAsync<string>()!;

                    // Add the link to the internal collection
                    DataLayer.InsertTargetLinkToDb(page.PostId, videoLink);

                    // Mark record as processed
                    await DataLayer.UpdatePostPageProcessedFlagAsync(page.PostId, true);
                }
                catch (Exception ex)
                {
                    // Log the error and continue with the next page
                    //_logger.LogError(ex, "Error extracting target link for page {LinkId}", page.Link);
                }
            }
            _aggregator.Publish(new StatusBarMessage("Finished extracting target links."));
        }
        catch (BrowserAbortedException e)
        {
            //Browser crashed to clean-up any resources and abort
            await _webAutomationService.DisposeAsync();
        }
        finally
        {
            await _webAuto.DisposeAsync();
        }
    
    }















    private void ReportStatus(string txt)
    {
        // _aggregator.Publish(new StatusMessage(txt));
        Program.Logger.LogInformation(txt);
    }








}


