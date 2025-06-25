// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using MediaRecycler.Modules.Loggers;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;
using PuppeteerSharp.BrowserData;



namespace MediaRecycler.Modules;


/// <summary>
///     Manages the lifecycle of the Puppeteer IBrowser and IPage instances.
///     Its single responsibility is to launch and dispose of the browser correctly.
/// </summary>
public class PuppeteerManager : IAsyncDisposable
{

    private readonly IEventAggregator _aggregator;
    private bool _disposed = false;







    public PuppeteerManager(IEventAggregator aggregator)
    {
        _aggregator = aggregator;


    }


















    public async Task InitAsync()
    {

        /*
         await CreateBrowserTaskAsync(HeadlessBrowserOptions.Default);
          if (Browser is null) throw new ArgumentNullException(nameof(Browser),"Browser failed to initialize.");
          await CreateContextTaskAsync();
          if (PuppeteerSharp.Browser.Context is null) throw new ArgumentNullException(nameof(Context),"Browser context failed to initialize.");
          await CreatePageTaskAsync();
          if (Page is null) throw new ArgumentNullException(nameof(Page), "Page failed to initialize.");
          */
        //   Browser = await AttemptToConnectAsync();
        var fo = new BrowserFetcherOptions
        {
            Path = Path.GetFullPath(nameof(Environment.SpecialFolder.ApplicationData))
        };
        var fetch = new PuppeteerSharp.BrowserFetcher();

        Task<InstalledBrowser> t = fetch.DownloadAsync();
        try
        {

            await t;
        }
        catch (Exception e)
        {
            _aggregator.Publish(new StatusBarMessage($"Failed to download browser: {e.Message}"));
            Program.Logger.LogError(e, "Failed to download browser.");
        }


        try
        {

            if (Browser is null)
            {
                Program.Logger.LogInformation("Launching new browser with saved settings....");
                _aggregator.Publish(new StatusBarMessage("Launching browser...."));

                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = HeadlessBrowserOptions.Default.Headless,
                    ExecutablePath = t.Result.GetExecutablePath(),
                    DefaultViewport = null,
                    LogProcess = true,
                    Timeout = ScrapingOptions.Default.DefaultPuppeteerTimeout,
                    UserDataDir = HeadlessBrowserOptions.Default.UserDataDir




                });

                var pages = await browser.PagesAsync();
                Page = pages.Length > 0 ? pages.First() : await browser.NewPageAsync();



                Browser = browser;
                // await CreateBrowserTaskAsync(HeadlessBrowserOptions.Default);
                if (Browser is null)
                {
                    throw new ArgumentNullException(nameof(Browser), "Browser failed to initialize.");
                }

                if (Page is null)
                {
                    throw new ArgumentNullException(nameof(Page), "Page failed to initialize.");
                }
            }

        }
        catch (Exception e)
        {

            Program.Logger.LogError(e, "Error initializing puppet manager.");
            _aggregator.Publish(new StatusMessage("Unable to create browser object. Check settings and try again."));
        }
    }







    private async Task CreateBrowserTaskAsync(HeadlessBrowserOptions @default)
    {
        try
        {

            var launchOptions = new PuppeteerSharp.LaunchOptions
            {
                Headless = false, //HeadlessBrowserOptions.Default.Headless,
                ExecutablePath = Properties.Settings.Default.PuppeteerExecutablePath,
                UserDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChromeData"),
                DefaultViewport = null,
                Timeout = ScrapingOptions.Default.DefaultPuppeteerTimeout,
            };

            Browser = await Puppeteer.LaunchAsync(launchOptions);
            Browser.Disconnected += OnBrowserDisconnect;
            Browser.TargetDestroyed += OnTargetDestroyed;
            Browser.Closed += OnBrowserClosed;


            Program.Logger.LogInformation("Browser launched successfully.");

            var pages = await Browser.PagesAsync();
            Page = pages.Length > 0 ? pages.First() : await Browser.NewPageAsync();

            _ = await Page.GoToAsync(ScrapingOptions.Default.StartingWebPage);



        }
        catch (Exception e)
        {

            Program.Logger.LogError(e, "Failed to initialize the browsesr..");
        }





    }







    private void OnBrowserClosed(object? sender, EventArgs e)
    {
        Program.Logger.LogError("Browser has been closed.");

        // Notify other components about the browser closure
        NotifyRecovery("The browser instance has been closed.");

        // Dispose of any resources if necessary
        DisposeAsync().AsTask().Wait();
        throw new BrowserAbortedException("The browser instance has been closed.");
    }








    private void OnTargetDestroyed(object? sender, TargetChangedArgs e)
    {
        Program.Logger.LogError("A target has been destroyed: {TargetUrl}", e.Target.Url);

        // Notify other components about the target destruction
        NotifyRecovery($"Target destroyed: {e.Target.Url}");

        DisposeAsync().AsTask().Wait();
        throw new BrowserAbortedException("The browser instance has been closed.");
    }







    private void OnBrowserDisconnect(object? sender, EventArgs e)
    {
        Program.Logger.LogWarning("Browser has been disconnected.");

        // Notify other components about the disconnection
        NotifyRecovery("The browser instance has been disconnected.");

        // Attempt to clean up resources
        DisposeAsync().AsTask().Wait();
        throw new BrowserAbortedException("The browser instance has been disconnected.");
    }







    public IPage Page
    { get; set; }



    public IBrowser? Browser { get; set; }







    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        Program.Logger.LogInformation("Disposing Puppeteer resources...");

        try
        {
            if (Page is { IsClosed: false })
            {
                await Page.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            Program.Logger.LogWarning($"Error closing page: {ex.Message}");
            NotifyRecovery("Error closing page");
        }

        try
        {
            if (Browser != null)
            {
                await Browser.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            Program.Logger.LogWarning($"Error closing browser: {ex.Message}");
            NotifyRecovery("Problem closing browser.");
        }

        Program.Logger.LogInformation("Resources disposed.");
    }















    /*

    public static async Task<PuppeteerManager?> CreateAsync(IEventAggregator aggregator)
    {
        var manager = new PuppeteerManager(aggregator);
        _logger = Program.Logger;
        _ = HeadlessBrowserOptions.Default;
        var df = new LaunchOptions
        {
                    Headless = false,
                    ExecutablePath = "D:\\Chrome\\Win64-132.0.6834.83\\chrome-win64\\chrome.exe",
                    Timeout = 60000,
                    UserDataDir = "D:\\chromeuserdata",
                    DefaultViewport = new ViewPortOptions { Width = 1100, Height = 1000 },
                    ProtocolTimeout = 30000
        };

        try
        {

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[FATAL] Failed to initialize Puppeteer: {ex.Message}");
            await manager.DisposeAsync();
            return null;
        }
    }



    */



    /// <summary>
    ///     Attempts to connect to an existing Puppeteer browser instance using the last known WebSocket endpoint.
    /// </summary>
    /// <returns>
    ///     An <see cref="IBrowser" /> instance if the connection is successful; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    ///     If the connection fails, the method logs the error, clears the last used WebSocket endpoint,
    ///     and saves the updated settings.
    /// </remarks>
    /// <exception cref="Exception">
    ///     Thrown if an unexpected error occurs during the connection attempt.
    /// </exception>
    private async Task<IBrowser?> AttemptToConnectAsync()
    {
        string? lastWsEndpoint = Properties.Settings.Default.LastWSEndPoint;

        if (string.IsNullOrEmpty(lastWsEndpoint))
        {
            return null;
        }

        try
        {
            Program.Logger.LogInformation($"Attempting to connect to browser at {lastWsEndpoint}...");
            var browser = await Puppeteer.ConnectAsync(new ConnectOptions { BrowserWSEndpoint = lastWsEndpoint }).ConfigureAwait(false);
            Program.Logger.LogInformation("Successfully connected to the browser.");
            return browser;
        }
        catch (Exception ex)
        {
            Program.Logger.LogError($"Failed to connect to browser: {ex.Message}");
            NotifyRecovery("Failed to connect to browser.");

            //clear the last used endpoint if connection fails
            Properties.Settings.Default.LastWSEndPoint = null;
            Properties.Settings.Default.Save();
            return null;
        }
    }









    private void NotifyRecovery(string message)
    {
        _aggregator?.Publish(new PuppeteerRecoveryEvent(message));
    }









}






internal class BrowserAbortedException(string message) : Exception(message)
{
}