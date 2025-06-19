// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using KC.Crawler;

using MediaRecycler.Modules.Interfaces;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;
using PuppeteerSharp.Helpers;



namespace MediaRecycler.Modules;


/// <summary>
///     Orchestrates browser-based web scraping and download operations.
///     Manages PuppeteerSharp browser/page lifecycle, URL queueing (frontier), and download coordination.
///     Designed for dependency injection and asynchronous workflows.
/// </summary>
public class Scrapers : PuppetPageBase
{


    private readonly ILogger _scraperLogger;
    private bool _disposed;
    private string _startUrl;







    /// <summary>
    ///     Constructs a new <see cref="Scrapers" /> instance.
    ///     Use <see cref="CreateAsync" /> for instantiation.
    /// </summary>
    private Scrapers(ILogger logger) : base(logger)
    {

        _scraperLogger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

        _startUrl = ScrapingOptions.Default.StartingWebPage ?? throw new InvalidOperationException("StartingWebPage cannot be null.");
        _scraperLogger.LogInformation("Scraper instance constructed. Awaiting initialization...");
    }







    internal MiniFrontier Frontier { get; private set; }
    private static readonly IWebAutomationService _automationService;







    /// <summary>
    ///     Performs login on the target site using credentials from environment variables.
    /// </summary>
    private static async Task DoLoginAsync(IPage page)
    {
        _ = await page.GoToAsync("https://www.bdsmlr.com/login");
        await Task.Delay(5000);

        var element1Handle = await page.QuerySelectorAsync("input#email");

        if (element1Handle == null) return;

        string? email = Environment.GetEnvironmentVariable("SCRAPER_EMAIL");
        await element1Handle.TypeAsync(email);

        var element2Handle = await page.QuerySelectorAsync("input#password");

        if (element2Handle == null) return;

        string? password = Environment.GetEnvironmentVariable("SCRAPER_PASSWORD");
        await element2Handle.TypeAsync(password);

        await page.ClickAsync("button[type=submit]");
        await Task.Delay(5000);
    }







    /// <summary>
    ///     Handles status updates from the URL frontier.
    /// </summary>
    private static void Frontier_StatusUpdate(object? sender, FrontierStatusEventArgs e)
    {
        Console.WriteLine($"[{e.TimestampUtc:HH:mm:ss.fff}][{e.EventType}] {e.Message}");

        if (e.EventType == FrontierStatusEventType.EvictionEnd && e.Details != null)
        {
            if (e.Details.TryGetValue("ActuallyRemoved", out object? removedCount)) Console.WriteLine($"  Eviction removed {removedCount} items.");
        }
        else if (e.EventType == FrontierStatusEventType.Warning && e.Details != null)
        {
            if (e.Details.TryGetValue("Host", out object? host)) Console.WriteLine($"  Warning related to host: {host}");
        }
    }







    internal async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        try
        {
            // Dispose managed resources here if needed
            if (Frontier != null)
            {
                await Frontier.ShutDownAsync();
                Frontier = null!;
            }
        }
        catch (Exception ex)
        {
            _scraperLogger.LogError(ex, "Error during Scrapers.DisposeAsync");
        }
        finally
        {
            await base.DisposeAsync();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }







    /// <summary>
    ///     Wraps the login logic in a task, handling exceptions and logging errors.
    /// </summary>
    private Task DoLoginWrappedAsync(IPage page)
    {
        try
        {
            return DoLoginAsync(page).WaitAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _scraperLogger.LogError(ex, "An error occurred during login.");
            return Task.FromException(ex);
        }
    }







    /// <summary>
    ///     Gathers followed blog URLs from paginated pages and enqueues them in the frontier.
    /// </summary>
    public async Task GatherFollowedBlogsAsync()
    {
        bool nextPage = true;
        _scraperLogger.LogInformation("Navigating to initial URL: {StartUrl}", _startUrl);

        if (Page == null) throw new InvalidOperationException("Page is not initialized before gathering blogs.");

        Page.DefaultNavigationTimeout = 60000;
        Page.DefaultTimeout = 60000;

        _ = await Page.GoToAsync(_startUrl);
        _ = await Page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });

        //await ScrapeArchivePageAsync(Page);

        _ = await Page.WaitForSelectorAsync("div.namefollowerholder a").WithTimeout(TimeSpan.FromSeconds(ScrapingOptions.Default.DefaultPuppeteerTimeout));
        await Task.Delay(500);

        int pageNum = 1;

        while (nextPage)
        {
            _scraperLogger.LogDebug("--- Processing Page {PageNum} ---", pageNum);
            await Task.Delay(500);

            var nodes = await Page.QuerySelectorAllAsync("div.namefollowerholder");
            _scraperLogger.LogDebug("Found {NodeCount} potential blog entries on page {PageNum}.", nodes?.Length ?? 0, pageNum);

            if (nodes != null)
                foreach (var nodeHandle in nodes)
                {
                    IElementHandle? linkHandle = null;
                    IJSHandle? hrefHandle = null;

                    try
                    {
                        linkHandle = await nodeHandle.QuerySelectorAsync("a");

                        if (linkHandle != null)
                        {
                            hrefHandle = await linkHandle.GetPropertyAsync("href");
                            string? blogUrl = await hrefHandle.JsonValueAsync<string>();

                            if (Uri.TryCreate(blogUrl, UriKind.Absolute, out var uriResult))
                                if (Frontier.Enqueue(uriResult))
                                    _scraperLogger.LogTrace("Queued: {BlogUrl}", blogUrl);
                        }
                    }
                    finally
                    {
                        if (hrefHandle != null) await hrefHandle.DisposeAsync();

                        if (linkHandle != null) await linkHandle.DisposeAsync();

                        await nodeHandle.DisposeAsync();
                    }
                }

            IElementHandle? nextAnchorHandle = null;
            IJSHandle? nextHrefHandle = null;

            try
            {
                nextAnchorHandle = await Page.QuerySelectorAsync("ul.pagination a[rel='next']");

                if (nextAnchorHandle != null)
                {
                    nextHrefHandle = await nextAnchorHandle.GetPropertyAsync("href");
                    string? nextPageUrl = await nextHrefHandle.JsonValueAsync<string>();

                    if (!string.IsNullOrWhiteSpace(nextPageUrl) && nextPageUrl != _startUrl)
                    {
                        _startUrl = nextPageUrl;
                        _scraperLogger.LogInformation("Navigating to next page: {NextPageUrl}", _startUrl);
                        _ = await Page.GoToAsync(_startUrl, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
                        pageNum++;
                    }
                    else
                    {
                        _scraperLogger.LogInformation("Next page link found but URL is invalid or same as current ({NextPageUrl}). Stopping pagination.", nextPageUrl);
                        nextPage = false;
                    }
                }
                else
                {
                    _scraperLogger.LogInformation("No next page link found. Pagination finished.");
                    nextPage = false;
                }
            }
            finally
            {
                if (nextHrefHandle != null) await nextHrefHandle.DisposeAsync();

                if (nextAnchorHandle != null) await nextAnchorHandle.DisposeAsync();
            }
        }

        _scraperLogger.LogDebug("Exited pagination loop.");
    }







    public async Task GetPageSourceAsync()
    {
        if (Page is null)
        {
            _scraperLogger.LogCritical("The Page object is null, cannot continue");
            await DisposeAsync();

        }
        else
        {
            _ = await Page.GoToAsync(_startUrl, WaitUntilNavigation.Networkidle2);

            if (Page.Url.Contains("login")) await DoLoginWrappedAsync(Page);

            string? source = await Page.GetContentAsync();

            await File.WriteAllTextAsync(Directory.GetCurrentDirectory(), source);

        }




    }







    public event EventHandler<string>? MainLogUpdated;







    private void OnMainFormTextUpdated(string message)
    {
        MainLogUpdated?.Invoke(this, message);
    }







    private void OnStatusBarUpdated(string message)
    {
        StatusBarUpdated?.Invoke(this, message);
    }







    /// <summary>
    ///     Scrapes an archive page, optionally at a specific URL.
    /// </summary>
    internal async Task ScrapeArchivePageAsync(string singleBlogHomeUrl)
    {
        _scraperLogger.LogDebug("ScrapeArchivePage method being called.");
        OnMainFormTextUpdated("Starting::ScrapeArchivePageAsync()");

        ArgumentNullException.ThrowIfNull(Page, nameof(Page));
        ArgumentNullException.ThrowIfNull(singleBlogHomeUrl, nameof(singleBlogHomeUrl));
        DownloaderModule downloaderModule = new(_scraperLogger);

        // Subscribe to ExtractionUpdates before extraction starts

        try
        {
            await DoLoginWrappedAsync(Page);

            //Go to the first page of the archive for the single blog
            // This URL should be the blog's archive page, e.g., "https://example.bdsmlr.com/archive?page=1"
            // Ensure the URL ends with the archive page suffix and hard codes the page number to 1 to fix server-side loading issues
            var response = await Page.GoToAsync(singleBlogHomeUrl + ScrapingOptions.Default.ArchivePageUrlSuffix + "?page=1",
                        new NavigationOptions { WaitUntil = [WaitUntilNavigation.Networkidle2] });

            OnMainFormTextUpdated(response.StatusText);

            // Wait for the archive header to ensure the page has loaded
            _ = await Page.WaitForSelectorAsync("div.archiveheader", new WaitForSelectorOptions { Timeout = ScrapingOptions.Default.DefaultPuppeteerTimeout });

            //Start processing the blog archive
            await VideoLinkExtractor.ProcessSingleBlogAsync(singleBlogHomeUrl, _scraperLogger, Page, downloaderModule);
        }
        catch (Exception e)
        {
            OnMainFormTextUpdated(e.Message);

        }
        finally
        {
            await downloaderModule.DisposeAsync();
        }

    }







    public event EventHandler<string>? StatusBarUpdated;

}