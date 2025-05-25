#region Header

// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers

#endregion



// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"



using KC.Crawler;

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
public class Scrapers : PuppetPageBase, IAsyncDisposable
{

    private readonly HeadlessBrowserOptions _browserOptions;

    private readonly DownloaderOptions _downloaderOptions;
    private readonly MiniFrontierSettings _frontierOptions;
    private readonly ILogger _scraperLogger;
    private readonly Scraping _scraperOptions;

    private bool _disposed;

    private string _startUrl;







    /// <summary>
    ///     Constructs a new <see cref="Scrapers" /> instance.
    ///     Use <see cref="CreateAsync" /> for instantiation.
    /// </summary>
    private Scrapers(
        HeadlessBrowserOptions browserOptions,
        Scraping scraperOptions,
        DownloaderOptions downloaderOptions,
        ILogger logger)
        : base(logger)
    {
        _browserOptions = browserOptions ??
                          throw new ArgumentNullException(nameof(browserOptions), "Browser options cannot be null.");
        _scraperOptions = scraperOptions ??
                          throw new ArgumentNullException(nameof(scraperOptions), "Scraper options cannot be null.");
        _downloaderOptions = downloaderOptions ??
                             throw new ArgumentNullException(nameof(downloaderOptions),
                                 "Downloader options cannot be null.");
        _scraperLogger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

        _startUrl = scraperOptions.StartingWebPage ??
                    throw new InvalidOperationException("StartingWebPage cannot be null.");
        _scraperLogger.LogInformation("Scraper instance constructed. Awaiting initialization...");
    }







    internal MiniFrontier Frontier { get; private set; }







    /// <summary>
    ///     Disposes resources used by the Scrapers instance.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (Frontier != null)
        {
            Frontier.StatusUpdate -= Frontier_StatusUpdate;
        }

        await base.DisposeAsync();
        _disposed = true;
    }







    /// <summary>
    ///     Asynchronously creates and initializes a <see cref="Scrapers" /> instance.
    /// </summary>
    public static async Task<Scrapers> CreateAsync(HeadlessBrowserOptions launcher,
        Scraping scraperSettings,
        DownloaderOptions downloaderSettings,
        ILogger logger)
    {
        /*var launchOptions = Program.serviceProvider.GetRequiredService<IOptionsMonitor<LauncherSettings>>();
        var scraperOptions = Program.serviceProvider.GetRequiredService<IOptionsMonitor<ScraperSettings>>();
        var frontierOptions = Program.serviceProvider.GetRequiredService<IOptionsMonitor<MiniFrontierSettings>>();
        var downloaderOptions = Program.serviceProvider.GetRequiredService<IOptionsMonitor<DownloaderSettings>>();
        */



        Scrapers instance = new(launcher, scraperSettings, downloaderSettings, logger);

        try
        {
            instance._scraperLogger.LogInformation("Verifying distribution status and initializing URL frontier...");
            await instance.CheckFrontierStatusAsync();

            instance._scraperLogger.LogInformation("Starting asynchronous initialization...");
            ArgumentNullException.ThrowIfNull(launcher, nameof(launcher));

            await instance.InitializeAsync(launcher);

            if (instance.Browser == null)
            {
                throw new InvalidOperationException("Browser failed to initialize.");
            }

            instance._scraperLogger.LogDebug("Browser initialized.");

            await instance.CreateContextAsync();

            if (instance.Context == null)
            {
                throw new InvalidOperationException("Browser Context failed to initialize.");
            }

            instance._scraperLogger.LogDebug("Browser Context created.");

            await instance.CreatePageAsync();

            if (instance.Page == null)
            {
                throw new InvalidOperationException("Page failed to initialize.");
            }

            instance._scraperLogger.LogDebug("Page created.");

            instance._scraperLogger.LogInformation("Asynchronous initialization complete. Scraper is ready.");
            return instance;
        }
        catch (Exception ex)
        {
            instance._scraperLogger.LogError(ex, "Failed during asynchronous initialization.");
            await instance.DisposeAsync();
            throw;
        }
    }







    /// <summary>
    ///     Initializes the URL frontier and subscribes to status updates.
    /// </summary>
    private async Task CheckFrontierStatusAsync()
    {
        // Frontier = new MiniFrontier(FrontierSettings);
        // Frontier.StatusUpdate += Frontier_StatusUpdate;
        await Task.CompletedTask;
    }







    /// <summary>
    ///     Handles status updates from the URL frontier.
    /// </summary>
    private static void Frontier_StatusUpdate(object? sender, FrontierStatusEventArgs e)
    {
        Console.WriteLine($"[{e.TimestampUtc:HH:mm:ss.fff}][{e.EventType}] {e.Message}");

        if (e.EventType == FrontierStatusEventType.EvictionEnd && e.Details != null)
        {
            if (e.Details.TryGetValue("ActuallyRemoved", out var removedCount))
            {
                Console.WriteLine($"  Eviction removed {removedCount} items.");
            }
        }
        else if (e.EventType == FrontierStatusEventType.Warning && e.Details != null)
        {
            if (e.Details.TryGetValue("Host", out var host))
            {
                Console.WriteLine($"  Warning related to host: {host}");
            }
        }
    }







    /// <summary>
    ///     Runs the main scraping and downloading workflow.
    /// </summary>
    public async Task RunAsync()
    {
        _scraperLogger.LogDebug("Creating Downloader module for enqueuing video URLs");
        var downloader = await DownloaderModule.CreateAsync(_downloaderOptions, _scraperLogger);

        _scraperLogger.LogDebug("Scraper starting to gather followed blogs for the URL Frontier...");

        ArgumentNullException.ThrowIfNull(Page, nameof(Page));

        try
        {
            // Optionally, you could make StartDownloader a config option
            var settings = _scraperOptions;
            settings.StartDownloader = true;

            await VideoLinkExtractor.ProcessSingleBlogAsync(new Uri(_startUrl), _scraperLogger, Page, settings,
                downloader);

            _scraperLogger.LogTrace("Enable Downloader Check: {StartDownloader}", settings.StartDownloader);

            if (settings.StartDownloader)
            {
                _scraperLogger.LogInformation("Downloader module enabled. Starting download process...");
                downloader.Start();
                downloader.SignalNoMoreUrls();
                _scraperLogger.LogInformation("Waiting for downloader to complete processing...");
                await downloader.WaitForDownloadsAsync();
                _scraperLogger.LogInformation("Downloader has completed processing its queue.");
            }
        }
        catch (Exception ex)
        {
            _scraperLogger.LogError(ex, "An error occurred during scraping run.");
            throw;
        }
        finally
        {
            if (Frontier != null)
            {
                Frontier.StatusUpdate -= Frontier_StatusUpdate;
            }

            if (Frontier != null)
            {
                await Frontier.ShutDownAsync();
            }

            _scraperLogger.LogInformation("RunAsync completing.");
            await DisposeAsync();
        }
    }







    /// <summary>
    ///     Gathers followed blog URLs from paginated pages and enqueues them in the frontier.
    /// </summary>
    public async Task GatherFollowedBlogsAsync()
    {
        var nextPage = true;
        _scraperLogger.LogInformation("Navigating to initial URL: {StartUrl}", _startUrl);

        if (Page == null)
        {
            throw new InvalidOperationException("Page is not initialized before gathering blogs.");
        }

        Page.DefaultNavigationTimeout = 60000;
        Page.DefaultTimeout = 60000;

        _ = await Page.GoToAsync(_startUrl);
        _ = await Page.WaitForNavigationAsync(new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
        });

        await ScrapeArchivePageAsync(Page);

        _ = await Page.WaitForSelectorAsync("div.namefollowerholder a")
            .WithTimeout(TimeSpan.FromSeconds(_scraperOptions.DefaultPuppeteerTimeout));
        await Task.Delay(500);

        var pageNum = 1;

        while (nextPage)
        {
            _scraperLogger.LogDebug("--- Processing Page {PageNum} ---", pageNum);
            await Task.Delay(500);

            IElementHandle[]? nodes = await Page.QuerySelectorAllAsync("div.namefollowerholder");
            _scraperLogger.LogDebug("Found {NodeCount} potential blog entries on page {PageNum}.", nodes?.Length ?? 0,
                pageNum);

            if (nodes != null)
            {
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
                            var blogUrl = await hrefHandle.JsonValueAsync<string>();

                            if (Uri.TryCreate(blogUrl, UriKind.Absolute, out var uriResult))
                            {
                                if (Frontier.Enqueue(uriResult))
                                {
                                    _scraperLogger.LogTrace("Queued: {BlogUrl}", blogUrl);
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (hrefHandle != null)
                        {
                            await hrefHandle.DisposeAsync();
                        }

                        if (linkHandle != null)
                        {
                            await linkHandle.DisposeAsync();
                        }

                        await nodeHandle.DisposeAsync();
                    }
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
                    var nextPageUrl = await nextHrefHandle.JsonValueAsync<string>();

                    if (!string.IsNullOrWhiteSpace(nextPageUrl) && nextPageUrl != _startUrl)
                    {
                        _startUrl = nextPageUrl;
                        _scraperLogger.LogInformation("Navigating to next page: {NextPageUrl}", _startUrl);
                        _ = await Page.GoToAsync(_startUrl,
                            new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
                        pageNum++;
                    }
                    else
                    {
                        _scraperLogger.LogInformation(
                            "Next page link found but URL is invalid or same as current ({NextPageUrl}). Stopping pagination.",
                            nextPageUrl);
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
                if (nextHrefHandle != null)
                {
                    await nextHrefHandle.DisposeAsync();
                }

                if (nextAnchorHandle != null)
                {
                    await nextAnchorHandle.DisposeAsync();
                }
            }
        }

        _scraperLogger.LogDebug("Exited pagination loop.");
    }







    /// <summary>
    ///     Scrapes an archive page, optionally at a specific URL.
    /// </summary>
    private async Task ScrapeArchivePageAsync(IPage page, string? archivePgUrl = null)
    {
        _scraperLogger.LogDebug("ScrapeArchivePage method being called.");
        ArgumentNullException.ThrowIfNull(page, nameof(page));

        if (archivePgUrl is null)
        {
            var arcElement = await page.QuerySelectorAsync(_scraperOptions.GroupingSelector);

            if (arcElement != null)
            {
                // TODO: Implement archive page scraping logic
            }
        }
        else
        {
            _ = await page.GoToAsync(archivePgUrl,
                new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
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
    ///     Performs login on the target site using credentials from environment variables.
    /// </summary>
    private static async Task DoLoginAsync(IPage page)
    {
        _ = await page.GoToAsync("https://www.bdsmlr.com/login");
        await Task.Delay(5000);

        var element1Handle = await page.QuerySelectorAsync("input#email");

        if (element1Handle == null)
        {
            return;
        }

        var email = Environment.GetEnvironmentVariable("SCRAPER_EMAIL");
        await element1Handle.TypeAsync(email);

        var element2Handle = await page.QuerySelectorAsync("input#password");

        if (element2Handle == null)
        {
            return;
        }

        var password = Environment.GetEnvironmentVariable("SCRAPER_PASSWORD");
        await element2Handle.TypeAsync(password);

        await page.ClickAsync("button[type=submit]");
        await Task.Delay(5000);
    }

}
