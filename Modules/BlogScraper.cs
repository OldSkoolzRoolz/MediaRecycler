// Project Name: MediaRecycler
// File Name: BlogScraper.cs
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using System.Linq;

using MediaRecycler.Logging;
using MediaRecycler.Model;
using MediaRecycler.Modules.Interfaces;
using MediaRecycler.Modules.Options;

using Microsoft.EntityFrameworkCore;

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
public class BlogScraper : IBlogScraper
{

    private readonly IWebAutomationService _automationService;






    public BlogScraper(IWebAutomationService automationService)
    {
        _automationService = automationService ?? throw new ArgumentNullException(nameof(automationService), "AutomationService cannot be null.");

        //  _downloaderModule = downloaderModule ?? throw new ArgumentNullException(nameof(downloaderModule), "DownloaderModule cannot be null.");

        Log.LogInformation("The Scraper .ctor has finished.");
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
    public async Task BeginScrapingTargetBlogAsync(CancellationToken cancellation)
    {
        await _automationService.InitAsync();
        if (ScrapingOptions.Default == null) throw new InvalidOperationException("ScrapingOptions.Default is null. Ensure it is properly configured.");

        if (string.IsNullOrWhiteSpace(ScrapingOptions.Default.GroupingSelector))
        {
            Log.LogInformation("GroupingSelector is null or empty. Check Scraping Options and try again.");
            throw new InvalidOperationException("GroupingSelector is null. Check Scraping Options and try again.");
        }


        try
        {


            cancellation.ThrowIfCancellationRequested();
            Log.LogInformation($"Navigating to initial page: {ScrapingOptions.Default.StartingWebPage}...");

            //await NavigateAndLoginIfNeededAsync(ScrapingOptions.Default.StartingWebPage, cancellation);


            if(!await _automationService.NavigateWithRetryAsync(ScrapingOptions.Default.StartingWebPage)) 
            {   Log.LogCritical("Unable to navigate to the starting URL. Please check the URL and try again.");
                await _automationService.DisposeAsync();
                return;
            }

            int totalLinksFound = 0;
            int pageNumber = 1;
            bool hasNextPage = true;
            Log.LogInformation("Starting scraping process...");

            while (hasNextPage)
            {
                cancellation.ThrowIfCancellationRequested();

                Log.LogInformation("Processing page number: {PageNumber}", pageNumber);
                int linksOnCurrentPage = await ScrapeCurrentPageLinksAsync(pageNumber);
                totalLinksFound += linksOnCurrentPage;

                Log.LogInformation($"Found {linksOnCurrentPage} links on page {pageNumber}. Total links found: {totalLinksFound}.");

                hasNextPage = await HandlePaginationAsync();

                pageNumber++;
                Log.UpdatePageCount(pageNumber);
            }
        }
        catch (NavigationException navEx)
        {
            Log.LogError(navEx, "Navigation error during scraping: {Message}", navEx.Message);
        }
        catch (WaitTaskTimeoutException timeoutEx)
        {
            Log.LogError(timeoutEx, "Timeout waiting for element during scraping: {Message}", timeoutEx.Message);
        }
        catch (OperationCanceledException)
        {
            Log.LogInformation("Cancellation has been initiated...");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
        {

            Log.LogError(ex, "An unexpected error occurred during scraping: {Message}", ex.Message);
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
            if (_automationService is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync();
        }
    }






    /// <summary>
    /// </summary>
    /// <returns></returns>
    public Task DownloadCollectedLinksAsync()
    {
        throw new NotImplementedException();
    }






    /// <summary>
    ///     Extracts target links from a list of unprocessed post pages.
    /// </summary>
    /// <param name="ctsToken"></param>
    /// <remarks>
    ///     This function iterates through a list of post pages that have not been processed,
    ///     extracts the video link from each page, and adds it to the internal collection.
    ///     It also marks each processed page as processed in the database.
    /// </remarks>
    public async Task ExtractTargetLinksAsync(CancellationToken ctsToken)
    {

        Log.LogInformation("Starting extraction of target links.");
         using var db = new MRContext();
        
        int pagesToProcess = await db.PostPages.CountAsync(p => !p.IsProcessed);
        Log.UpdateQueue(pagesToProcess);

        // Start the puppeteer headless browser and page.
        await _automationService.InitAsync();

        await foreach (var page in db.PostPages.AsQueryable().Where(p => !p.IsProcessed).AsAsyncEnumerable())
        {
            try
            {
                pagesToProcess--;

                if (ctsToken.IsCancellationRequested)
                {
                    Log.LogInformation("Cancellation requested. Stopping extraction of target links.");
                    await _automationService.DisposeAsync();

                }

                Log.LogDebug("Navigating to post page: {0}", page.Link);

                if (!await _automationService.NavigateWithRetryAsync(page.Link))
                {
                    Log.LogCritical("Unabled to navigate to Url. Aborting url");
                }

                // Wait for the video source element to be present
                await _automationService.WaitForSelectorAsync(@"video > source");

                var sourceHandle = await _automationService.QuerySelectorAsync(@"video > source").ConfigureAwait(false);

                if (sourceHandle == null)
                {
                    Log.LogWarning("Video source element not found on page: {Link}", page.Link);
                    continue;
                }

                string? videoLink = await sourceHandle.GetPropertyAsync("src").Result.JsonValueAsync<string>();

                if (!string.IsNullOrWhiteSpace(videoLink))
                {
                    Log.LogInformation("Extracted video link: {0} from page: {1}", videoLink, page.Link);
                    page.IsProcessed = true;
                    db.TargetLinks.Add(new TargetLink
                    {
                        PostId = page.PostId,
                        Link = videoLink
                    });
                }
                else
                {
                    Log.LogWarning("Extracted video link was null or empty from page: {0}", page.Link);
                }

                Log.UpdateQueue(pagesToProcess);

            }
            catch (WaitTaskTimeoutException timeoutEx)
            {
                Log.LogError(timeoutEx, "Timeout waiting for video source on page {0}. Marking as processed to avoid re-attempting.", page.Link);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Error extracting target link for page {0}. ", page.Link);
            }


        }
        await db.SaveChangesAsync();
        await db.DisposeAsync();
        Log.LogInformation("Processing of post pages is finished.");
        await _automationService.DisposeAsync();
    }






    public ValueTask CancelAsync()
    {
        // If there are ongoing Puppeteer operations that can be cancelled,
        // this is where you'd pass a CancellationToken to them.
        // For now, it's a placeholder.
        Log.LogInformation("Scraping cancellation requested.");
        return ValueTask.CompletedTask;
    }






    /// <summary>
    ///     Disposes the <see cref="IWebAutomationService" />.
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






    internal static string ExtractPostIDFromUrl(string link)
    {
        string? postid = link.Split("/").LastOrDefault();

        return !string.IsNullOrWhiteSpace(postid) ? postid : "ExtractError";
    }






    /// <summary>
    ///     Handles pagination by clicking the next page link.
    /// </summary>
    /// <returns>True if a next page link was found and clicked, false otherwise.</returns>
    private async Task<bool> HandlePaginationAsync()
    {
        try
        {
            var paginationElement = await _automationService.QuerySelectorAsync(ScrapingOptions.Default.PaginationSelector);

            if (paginationElement != null)
            {
                Log.LogInformation("Found pagination element. Clicking to navigate to next page...");
                await paginationElement.ClickAsync();

                // Wait for navigation to complete after clicking.
                // PuppeteerAutomationService.ClickElementAsync already handles navigation wait.
                // If ClickElementAsync doesn't wait for navigation, add it here.
                // For now, assuming ClickAsync is enough or next WaitForSelectorAsync will handle it.
                // A more robust approach would be to wait for a specific URL change or network idle.
                // await _automationService.Page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });
                return true;
            }

            Log.LogInformation("No pagination element found with selector '{Selector}'.", ScrapingOptions.Default.PaginationSelector);
            return false;
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Error handling pagination with selector '{Selector}'.", ScrapingOptions.Default.PaginationSelector);
            return false; // Stop pagination on error
        }
    }






    /// <summary>
    ///     Navigates to the specified URL and performs login if required.
    /// </summary>
    /// <param name="url">The URL to navigate to.</param>
    /// <param name="cancellation"></param>
    private async Task NavigateAndLoginIfNeededAsync(string url, CancellationToken cancellation)
    {
        await _automationService.GoToAsync(url);

        // Check for login requirement by looking for a specific element or content
        // This is more robust than checking page content string which might change.
        // Assuming 'input#email' is a good indicator for the login page.
        bool requiresLogin = await _automationService.IsElementVisibleAsync("input#email");

        if (requiresLogin && !cancellation.IsCancellationRequested)
        {
            Log.LogInformation("Login required. Attempting to log in...");
            await _automationService.DoSiteLoginAsync().ConfigureAwait(false);

            // After login, navigate to the starting page again to ensure we are on the correct page
            Log.LogInformation("Login successful. Navigating back to {Url}...", url);
            await _automationService.GoToAsync(url);
        }
        else
        {
            Log.LogInformation("No login required for {Url}.", url);
        }
    }






    /// <summary>
    ///     Scrapes links from the current page and inserts them into the database.
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
            var mediaContainer = await _automationService.GetNodeCollectionFromPageAsync(ScrapingOptions.Default.GroupingSelector);
            
            if (mediaContainer == null || mediaContainer.Length == 0)
            {
                Log.LogWarning("No elements found with selector '{Selector}' on page {PageNumber}.", ScrapingOptions.Default.GroupingSelector, pageNumber);
                return 0;
            }

            // Iterate the elements we have found.
            foreach (var anchorHandle in mediaContainer)
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

                        await DataLayer.InsertPostPageUrlToDbAsync(ExtractPostIDFromUrl(url), url);
                        linksFoundOnPage++;
                        Log.LogDebug("Extracted and queued link: {Url}", url);
                    }
                }
                catch (Exception ex)
                {
                    Log.LogWarning(ex, "Error extracting href from an element on page {PageNumber}. Skipping element.", pageNumber);
                }
                finally
                {
                    // Dispose the element handle to free up Puppeteer resources
                    await anchorHandle.DisposeAsync();
                }

            Log.LogInformation("Finished scraping {LinksFound} links on page {PageNumber}.", linksFoundOnPage, pageNumber);
        }
        catch (WaitTaskTimeoutException timeoutEx)
        {
            Log.LogError(timeoutEx, "Timeout waiting for selector '{Selector}' on page {PageNumber}.", ScrapingOptions.Default.GroupingSelector, pageNumber);
            throw; // Re-throw to be caught by the main scraping loop
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Error scraping links on page {PageNumber}.", pageNumber);
            throw; // Re-throw to be caught by the main scraping loop
        }

        return linksFoundOnPage;
    }

}