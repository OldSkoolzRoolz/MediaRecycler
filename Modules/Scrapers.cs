// Project Name: MediaRecycler
// Author:  Kyle Crowder [InvalidReference]
// **** Distributed under Open Source License ***
// ***   Do not remove file headers ***




using KC.Crawler.MiniFrontier;
using KC.Downloader;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PuppeteerSharp;
using PuppeteerSharp.Helpers;
// Added for ILogger
// Added for NullLoggerFactory as fallback
using dom = PuppeteerSharp.Dom;

namespace Scrapper;

public class Scrapers : PuppetPageBase
{
    private readonly DownloaderSettings _downloaderOptions;
    private readonly MiniFrontierSettings _frontierSettings;
    private readonly ILogger<Scrapers> _scraperLogger;
    private readonly ScraperSettings _scraperSettings;
    private string _startUrl;







    // Accepts logger(s) and settings via DI
    public Scrapers(
        IOptions<LauncherSettings> launchOptions,
        IOptions<ScraperSettings> scraperOptions,
        IOptions<MiniFrontierSettings> frontierOptions,
        IOptions<DownloaderSettings> downloaderOptions,
        ILoggerFactory factory)
        : base(factory)
    {
        _scraperLogger = factory.CreateLogger<Scrapers>();
        _scraperSettings = scraperOptions.Value;
        _frontierSettings = frontierOptions.Value;
        _downloaderOptions = downloaderOptions.Value;
        _startUrl = _scraperSettings.StartingWebPage;
        _launchOptions = launchOptions.Value;
        _scraperLogger.LogInformation("Scraper instance constructed. Awaiting initialization...");
    }







    internal MiniFrontier Frontier
    {
        get;
        private set;
    }







    // Responsible for creating the instance AND performing async initialization.
    public static async Task<Scrapers> CreateAsync(
        IOptions<LauncherSettings> launchOptions,
        IOptions<ScraperSettings> scraperOptions,
        IOptions<MiniFrontierSettings> frontierOptions,
        IOptions<DownloaderSettings> downloaderOptions,
        ILoggerFactory factory)
    {
        var instance = new Scrapers(launchOptions, scraperOptions, frontierOptions, downloaderOptions, factory);

        try
        {
            instance._scraperLogger.LogInformation("Verifiying distribution status and initializing URL frontier...");
            await instance.CheckFrontierStatusAsync();

            instance._scraperLogger.LogInformation("Starting asynchronous initialization...");
            ArgumentNullException.ThrowIfNull(launchOptions, nameof(launchOptions));
            await instance.InitializeAsync(launchOptions);
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
            await (instance as IAsyncDisposable).DisposeAsync();
            throw;
        }
    }







    private async Task CheckFrontierStatusAsync()
    {
        // Use settings from DI
        Frontier = new MiniFrontier(_frontierSettings);
        Frontier.StatusUpdate += Frontier_StatusUpdate;
        await Task.CompletedTask;
    }







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







    public async Task RunAsync()
    {
        _scraperLogger.LogDebug("Creating Downloader module for enqueing video urls");
        // You may want to inject DownloaderSettings via IOptions as well
        var downloader = await DownloaderModule.CreateAsync(_downloaderOptions, _scraperLogger);
        downloader.Start();


        _scraperLogger.LogDebug("Scraper starting to gather followed blogs for the Url Frontier...");

        ArgumentNullException.ThrowIfNull(Page, nameof(Page));

        try
        {
            // If starting from followers page

            // _scraperLogger.LogInformation("Gathering blog links from 'following' pages  ");
            // await GatherFollowedBlogsAsync();//  _scraperLogger.LogInformation("Scraper run finished. Found {BlogCount} unique blog URLs.", Frontier.Count().ToString());
            // _scraperLogger.LogDebug("Loading of frontier with targets complete");



            // --- Archive Page Scraping ---
            _scraperSettings.StartDownloader = true;

            await VideoLinkExtractor.ProcessSingleBlogAsync(new Uri(_startUrl), _scraperLogger, Page, _scraperSettings,
                downloader);


            //await VideoLinkExtractor.ParseBlogArchivePagesAsync(new Uri(_startUrl), _startUrl,_scraperLogger, Page, _scraperSettings);




            //   await VideoLinkExtractor.StartBulkParsingAsync(Frontier, _scraperLogger, Page, _scraperSettings, downloader);

            _scraperLogger.LogTrace("Enable Downloader Check: {StartDownloader}", _scraperSettings.StartDownloader);
            if (_scraperSettings.StartDownloader)
            {
                _scraperLogger.LogInformation("downloader module enabled. Starting download process...");
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
            Frontier.StatusUpdate -= Frontier_StatusUpdate;
            await Frontier.ShutDownAsync();
            _scraperLogger.LogInformation("RunAsync completing.");
            await (this as IAsyncDisposable).DisposeAsync();
        }
    }







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



        // Wait for the page to load and the login button to appear
        // await DoLoginAsync(Page);

        // Login was successful, proceed to navigate to the initial URL
        await Page.GoToAsync(_startUrl);
        await Page.WaitForNavigationAsync(new NavigationOptions
        {
            WaitUntil = new[]
            {
                WaitUntilNavigation.Networkidle0
            }
        });

        await ScrapeArchivePageAsync(Page);



        await Page.WaitForSelectorAsync("div.namefollowerholder a")
            .WithTimeout(TimeSpan.FromSeconds(_scraperSettings.DefaultPuppeteerTimeout));
        await Task.Delay(500);

        var pageNum = 1;
        while (nextPage)
        {
            _scraperLogger.LogDebug("--- Processing Page {PageNum} ---", pageNum);
            await Task.Delay(500);

            var nodes = await Page.QuerySelectorAllAsync("div.namefollowerholder");
            _scraperLogger.LogDebug("Found {NodeCount} potential blog entries on page {PageNum}.", nodes?.Length ?? 0,
                pageNum);

            if (nodes != null)
            {
                foreach (var nodeHandle in nodes)
                {
                    IElementHandle linkHandle = null!;
                    IJSHandle hrefHandle = null!;
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

            IElementHandle nextAnchorHandle = null!;
            IJSHandle nextHrefHandle = null!;
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
                        await Page.GoToAsync(_startUrl,
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







    private async Task ScrapeArchivePageAsync(IPage page, string? ArchivePgUrl = null)
    {
        _scraperLogger.LogDebug("ScrapeArchivePage method being called.");
        ArgumentNullException.ThrowIfNull(page, nameof(page));

        if (ArchivePgUrl is null)
        {

            var arcElement = await page.QuerySelectorAsync(_scraperSettings.ArchiveLinkSelector);
            if (arcElement != null)
            {

            }
        }
        else
        {



            await page.GoToAsync(ArchivePgUrl,
                new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
        }

    }







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







    private static async Task DoLoginAsync(IPage page)
    {
        await page.GoToAsync("https://www.bdsmlr.com/login");
        await Task.Delay(5000);

        var element1 = await page.QuerySelectorAsync<HtmlElement>("input#email");
        if (element1 == null)
        {
            return;
        }

        await element1.TypeAsync("fetishmaster1969@gmail.com");

        var element2 = await page.QuerySelectorAsync<HtmlElement>("input#password");
        if (element2 == null)
        {
            return;
        }

        await element2.TypeAsync("!Cubby2022");

        await page.ClickAsync("button[type=submit]");
        await Task.Delay(5000);
    }
}
