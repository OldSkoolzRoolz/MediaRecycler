// Project Name: MediaRecycler
// File Name: PuppeteerManager.cs
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using MediaRecycler.Logging;
using MediaRecycler.Modules.Options;
using MediaRecycler.Properties;

using PuppeteerSharp;
using PuppeteerSharp.BrowserData;



namespace MediaRecycler.Modules;


/// <summary>
///     Manages the lifecycle of the Puppeteer IBrowser and IPage instances.
///     Its single responsibility is to launch and dispose of the browser correctly.
/// </summary>
public class PuppeteerManager : IAsyncDisposable
{

    private bool _disposed;



    /// <summary>
    /// </summary>
    public IBrowserContext? Context { get; set; }



    /// <summary>
    ///     Gets or sets the Puppeteer <see cref="PuppeteerSharp.IPage" /> instance.
    ///     This property represents the current page being managed by the Puppeteer browser.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Page" /> is initialized during the browser setup process and is used
    ///     for navigation, interaction, and automation of web pages. Ensure that the browser
    ///     and context are properly initialized before accessing this property.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <see cref="Page" /> fails to initialize during the setup process.
    /// </exception>
    public IPage Page { get; set; }


    /// <summary>
    ///     Gets or sets the instance of the Puppeteer <see cref="PuppeteerSharp.IBrowser" />.
    ///     This property represents the browser instance managed by the <see cref="PuppeteerManager" />.
    /// </summary>
    /// <value>
    ///     The <see cref="PuppeteerSharp.IBrowser" /> instance used for managing browser operations.
    /// </value>
    /// <remarks>
    ///     Ensure that the browser is properly initialized before accessing this property.
    ///     If the browser fails to initialize, an <see cref="ArgumentNullException" /> may be thrown.
    /// </remarks>
    public IBrowser? Browser { get; set; }






    /// <summary>
    ///     Asynchronously releases the resources used by the <see cref="PuppeteerManager" /> instance.
    /// </summary>
    /// <remarks>
    ///     This method ensures that the <see cref="IBrowser" /> and <see cref="IPage" /> instances are properly closed and
    ///     disposed of.
    ///     It logs any errors encountered during the disposal process and notifies other components if necessary.
    /// </remarks>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs while closing the browser or page.
    /// </exception>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _disposed = true;

        Log.LogInformation("Disposing Puppeteer resources...");

        if (Browser != null)
        {
            try
            {
                await Browser.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing browser: {ex.Message}");
            }
            finally
            {
                Browser?.Dispose();
                Browser = null;
            }
        }

        Log.LogInformation("Resources disposed.");
    }






    /*

    public static async Task<PuppeteerManager?> CreateAsync(IEventAggregator aggregator)
    {
        var manager = new PuppeteerManager(aggregator);
        _logger = _logger;
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
            Log.LogError($"[FATAL] Failed to initialize Puppeteer: {ex.Message}");
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
        string? lastWsEndpoint = Settings.Default.LastWSEndPoint;

        if (string.IsNullOrEmpty(lastWsEndpoint)) return null;

        try
        {
            Log.LogInformation($"Attempting to connect to browser at {lastWsEndpoint}...");
            var browser = await Puppeteer.ConnectAsync(new ConnectOptions { BrowserWSEndpoint = lastWsEndpoint }).ConfigureAwait(false);
            Log.LogInformation("Successfully connected to the browser.");
            return browser;
        }
        catch (Exception ex)
        {
            Log.LogError($"Failed to connect to browser: {ex.Message}");

            //clear the last used endpoint if connection fails
            Settings.Default.LastWSEndPoint = null;
            Settings.Default.Save();
            return null;
        }
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
        var fetch = new BrowserFetcher();

        Task<InstalledBrowser> t = fetch.DownloadAsync();

        try
        {

            await t;
        }
        catch (Exception e)
        {
            // _aggregator.Publish(new StatusBarMessage($"Failed to download browser: {e.Message}"));
            Log.LogError(e, "Failed to download browser.");
        }


        try
        {

            if (Browser is null)
            {
                Log.LogInformation("Launching new browser with saved settings....");

                //  _aggregator.Publish(new StatusBarMessage("Launching browser...."));

                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                            Headless = HeadlessBrowserOptions.Default.Headless,
                            ExecutablePath = t.Result.GetExecutablePath(),
                            DefaultViewport = null,
                            Timeout = ScrapingOptions.Default.DefaultPuppeteerTimeout,
                            UserDataDir = HeadlessBrowserOptions.Default.UserDataDir,
                            Args = ["--disable-web-security", "--disable-features=VizDisplayCompositor", "--no-sandbox", "--disable-dev-shm-usage"]
                });

                Browser = browser;
            }

            // Context = await browser.CreateBrowserContextAsync();
            Page = await Browser.NewPageAsync();
            Page.DefaultTimeout = 90000;
            Page.DefaultNavigationTimeout = 90000;
            await Page.SetBypassCSPAsync(true);


            /*
                            await Page.SetRequestInterceptionAsync(true);

                            // Add an event listener to intercept requests
                            Page.Request += async (sender, e) =>
                            {
                                // Here you can check for specific requests and block them
                                if (e.Request.ResourceType != ResourceType.Document)
                                {
                                    // Block the request
                                    await e.Request.AbortAsync();
                                }
                                else
                                {
                                    // Allow the request to continue
                                    await e.Request.ContinueAsync();
                                }
                            };

                            */
            // await CreateBrowserTaskAsync(HeadlessBrowserOptions.Default);
  

        }
        catch (NavigationException ex)
        {
            // Handle navigation-specific errors
            Log.LogInformation($"Navigation failed: {ex.Message}");
        }
        catch (TimeoutException ex)
        {
            // Handle timeout errors specifically
            Log.LogInformation($"Operation timed out: {ex.Message}");
        }
        catch (PuppeteerException ex)
        {
            // Handle other Puppeteer-specific exceptions
            Log.LogInformation($"Puppeteer error: {ex.Message}");
        }
        catch (Exception e)
        {

            Log.LogError(e, "Error initializing puppet manager.");

            // _aggregator.Publish(new StatusMessage("Unable to create browser object. Check settings and try again."));
            if(Page != null) await Page.CloseAsync();
            if (Browser != null) await Browser.CloseAsync();

        }
    }



    public void LogException(Exception ex, string operation, Dictionary<string, object>? context = null)
    {
        var logData = new Dictionary<string, object>
        {
                    ["Operation"] = operation,
                    ["ExceptionType"] = ex.GetType().Name,
                    ["Message"] = ex.Message,
                    ["StackTrace"] = ex.StackTrace
        };

        if (context != null)
        {
            foreach (var kvp in context)
            {
                logData[kvp.Key] = kvp.Value;
            }
        }

        Log.LogError(ex, "Puppeteer operation failed: {Operation}", operation);
    }


    /*

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


                Log.LogInformation("Browser launched successfully.");

                var pages = await Browser.PagesAsync();
                Page = pages.Length > 0 ? pages.First() : await Browser.NewPageAsync();

                _ = await Page.GoToAsync(ScrapingOptions.Default.StartingWebPage);



            }
            catch (Exception e)
            {

                Log.LogError(e, "Failed to initialize the browsesr..");
            }





        }



        */






    private void OnBrowserClosed(object? sender, EventArgs e)
    {
        Log.LogError("Browser has been closed.");

        // Notify other components about the browser closure

        // Dispose of any resources if necessary
        DisposeAsync().AsTask().Wait();
        throw new BrowserAbortedException("The browser instance has been closed.");
    }






    private void OnBrowserDisconnect(object? sender, EventArgs e)
    {
        Log.LogWarning("Browser has been disconnected.");

        // Notify other components about the disconnection

        // Attempt to clean up resources
        DisposeAsync().AsTask().Wait();
        throw new BrowserAbortedException("The browser instance has been disconnected.");
    }






    private void OnTargetDestroyed(object? sender, TargetChangedArgs e)
    {
        Log.LogError("A target has been destroyed: {TargetUrl}", e.Target.Url);

        // Notify other components about the target destruction

        DisposeAsync().AsTask().Wait();
        throw new BrowserAbortedException("The browser instance has been closed.");
    }

}


internal class BrowserAbortedException(
            string message) : Exception(message)
{

}