// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"

using KC.Crawler;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PuppeteerSharp;
using PuppeteerSharp.Dom;
using PuppeteerSharp.Helpers;

namespace MediaRecycler.Modules;

/// <summary>
/// Orchestrates browser-based web scraping and download operations.
/// Manages PuppeteerSharp browser/page lifecycle, URL queueing (frontier), and download coordination.
/// Designed for dependency injection and asynchronous workflows.
/// </summary>
public class Scrapers : PuppetPageBase, IAsyncDisposable
{
    private readonly IOptionsMonitor<LauncherSettings> _launchOptionsMonitor;
    private readonly IOptionsMonitor<ScraperSettings> _scraperOptionsMonitor;
    private readonly IOptionsMonitor<MiniFrontierSettings> _frontierOptionsMonitor;
    private readonly IOptionsMonitor<DownloaderSettings> _downloaderOptionsMonitor;
    private readonly ILogger<Scrapers> _scraperLogger;

    private ScraperSettings ScraperSettings => _scraperOptionsMonitor.CurrentValue;
    private MiniFrontierSettings FrontierSettings => _frontierOptionsMonitor.CurrentValue;
    private DownloaderSettings DownloaderOptions => _downloaderOptionsMonitor.CurrentValue;
    private LauncherSettings LaunchOptions => _launchOptionsMonitor.CurrentValue;

    private string _startUrl;
    internal MiniFrontier Frontier { get; private set; }

    private bool _disposed;

    /// <summary>
    /// Constructs a new <see cref="Scrapers"/> instance.
    /// Use <see cref="CreateAsync"/> for instantiation.
    /// </summary>
    private Scrapers(
        IOptionsMonitor<LauncherSettings> launchOptions,
        IOptionsMonitor<ScraperSettings> scraperOptions,
        IOptionsMonitor<MiniFrontierSettings> frontierOptions,
        IOptionsMonitor<DownloaderSettings> downloaderOptions,
        ILoggerFactory factory)
        : base(factory)
    {
        _launchOptionsMonitor = launchOptions;
        _scraperOptionsMonitor = scraperOptions;
        _frontierOptionsMonitor = frontierOptions;
        _downloaderOptionsMonitor = downloaderOptions;
        _scraperLogger = factory.CreateLogger<Scrapers>();

        _startUrl = ScraperSettings.StartingWebPage ?? throw new InvalidOperationException("StartingWebPage cannot be null.");
        _scraperLogger.LogInformation("Scraper instance constructed. Awaiting initialization...");
    }

    /// <summary>
    /// Asynchronously creates and initializes a <see cref="Scrapers"/> instance.
    /// </summary>
    public static async Task<Scrapers> CreateAsync()
    {
        var launchOptions = Program.serviceProvider.GetRequiredService<IOptionsMonitor<LauncherSettings>>();
        var scraperOptions = Program.serviceProvider.GetRequiredService<IOptionsMonitor<ScraperSettings>>();
        var frontierOptions = Program.serviceProvider.GetRequiredService<IOptionsMonitor<MiniFrontierSettings>>();
        var downloaderOptions = Program.serviceProvider.GetRequiredService<IOptionsMonitor<DownloaderSettings>>();
        var factory = Program.serviceProvider.GetRequiredService<ILoggerFactory>();

        var instance = new Scrapers(launchOptions, scraperOptions, frontierOptions, downloaderOptions, factory);

        try
        {
            instance._scraperLogger.LogInformation("Verifying distribution status and initializing URL frontier...");
            await instance.CheckFrontierStatusAsync();

            instance._scraperLogger.LogInformation("Starting asynchronous initialization...");
            ArgumentNullException.ThrowIfNull(launchOptions, nameof(launchOptions));
            
            await instance.InitializeAsync(launchOptions.CurrentValue);

            if (instance.Browser == null)
                throw new InvalidOperationException("Browser failed to initialize.");

            instance._scraperLogger.LogDebug("Browser initialized.");

            await instance.CreateContextAsync();
            if (instance.Context == null)
                throw new InvalidOperationException("Browser Context failed to initialize.");

            instance._scraperLogger.LogDebug("Browser Context created.");

            await instance.CreatePageAsync();
            if (instance.Page == null)
                throw new InvalidOperationException("Page failed to initialize.");

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
    /// Initializes the URL frontier and subscribes to status updates.
    /// </summary>
    private async Task CheckFrontierStatusAsync()
    {
        Frontier = new MiniFrontier(FrontierSettings);
        Frontier.StatusUpdate += Frontier_StatusUpdate;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles status updates from the URL frontier.
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
    /// Runs the main scraping and downloading workflow.
    /// </summary>
    public async Task RunAsync()
    {
        _scraperLogger.LogDebug("Creating Downloader module for enqueuing video URLs");
        var downloader = await DownloaderModule.CreateAsync(DownloaderOptions, _scraperLogger);

        _scraperLogger.LogDebug("Scraper starting to gather followed blogs for the URL Frontier...");

        ArgumentNullException.ThrowIfNull(Page, nameof(Page));

        try
        {
            // Optionally, you could make StartDownloader a config option
            var settings = ScraperSettings;
            settings.StartDownloader = true;

            await VideoLinkExtractor.ProcessSingleBlogAsync(new Uri(_startUrl), _scraperLogger, Page, settings, downloader);

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
                Frontier.StatusUpdate -= Frontier_StatusUpdate;
            if (Frontier != null)
                await Frontier.ShutDownAsync();
            _scraperLogger.LogInformation("RunAsync completing.");
            await DisposeAsync();
        }
    }

    /// <summary>
    /// Gathers followed blog URLs from paginated pages and enqueues them in the frontier.
    /// </summary>
    public async Task GatherFollowedBlogsAsync()
    {
        var nextPage = true;
        _scraperLogger.LogInformation("Navigating to initial URL: {StartUrl}", _startUrl);

        if (Page == null)
            throw new InvalidOperationException("Page is not initialized before gathering blogs.");

        Page.DefaultNavigationTimeout = 60000;
        Page.DefaultTimeout = 60000;

        await Page.GoToAsync(_startUrl);
        await Page.WaitForNavigationAsync(new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
        });

        await ScrapeArchivePageAsync(Page);

        await Page.WaitForSelectorAsync("div.namefollowerholder a")
            .WithTimeout(TimeSpan.FromSeconds(ScraperSettings.DefaultPuppeteerTimeout));
        await Task.Delay(500);

        var pageNum = 1;
        while (nextPage)
        {
            _scraperLogger.LogDebug("--- Processing Page {PageNum} ---", pageNum);
            await Task.Delay(500);

            var nodes = await Page.QuerySelectorAllAsync("div.namefollowerholder");
            _scraperLogger.LogDebug("Found {NodeCount} potential blog entries on page {PageNum}.", nodes?.Length ?? 0, pageNum);

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
                            await hrefHandle.DisposeAsync();
                        if (linkHandle != null)
                            await linkHandle.DisposeAsync();
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
                        await Page.GoToAsync(_startUrl, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
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
                    await nextHrefHandle.DisposeAsync();
                if (nextAnchorHandle != null)
                    await nextAnchorHandle.DisposeAsync();
            }
        }

        _scraperLogger.LogDebug("Exited pagination loop.");
    }

    /// <summary>
    /// Scrapes an archive page, optionally at a specific URL.
    /// </summary>
    private async Task ScrapeArchivePageAsync(IPage page, string? archivePgUrl = null)
    {
        _scraperLogger.LogDebug("ScrapeArchivePage method being called.");
        ArgumentNullException.ThrowIfNull(page, nameof(page));

        if (archivePgUrl is null)
        {
            var arcElement = await page.QuerySelectorAsync(ScraperSettings.GroupingSelector);
            if (arcElement != null)
            {
                // TODO: Implement archive page scraping logic
            }
        }
        else
        {
            await page.GoToAsync(archivePgUrl, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
        }
    }

    /// <summary>
    /// Wraps the login logic in a task, handling exceptions and logging errors.
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
    /// Performs login on the target site using credentials from environment variables.
    /// </summary>
    private static async Task DoLoginAsync(IPage page)
    {
        await page.GoToAsync("https://www.bdsmlr.com/login");
        await Task.Delay(5000);

        var element1 = await page.QuerySelectorAsync<PuppeteerSharp.Dom.HtmlElement>("input#email");
        if (element1 == null)
            return;

        var email = Environment.GetEnvironmentVariable("SCRAPER_EMAIL");
        await element1.TypeAsync(email);

        var element2 = await page.QuerySelectorAsync<PuppeteerSharp.Dom.HtmlElement>("input#password");
        if (element2 == null)
            return;

        var password = Environment.GetEnvironmentVariable("SCRAPER_PASSWORD");
        await element2.TypeAsync(password);

        await page.ClickAsync("button[type=submit]");
        await Task.Delay(5000);
    }

    /// <summary>
    /// Disposes resources used by the Scrapers instance.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        if (Frontier != null)
            Frontier.StatusUpdate -= Frontier_StatusUpdate;

        await base.DisposeAsync();
        _disposed = true;
    }

    internal async Task GetPageSourceAsync(MediaRecycler.ScraperSettings scraperSettings)
    {
        throw new NotImplementedException();
    }
}
