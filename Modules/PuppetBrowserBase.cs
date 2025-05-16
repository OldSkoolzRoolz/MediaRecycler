// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options; // Added for NullLogger
using PuppeteerSharp;

namespace Scrapper;

/// <summary>
/// Base class for managing a Puppeteer Browser instance.
/// Handles initialization, disposal, and provides default launch options.
/// </summary>
public class PuppetBrowserBase : IAsyncDisposable
{
    protected readonly ILogger<PuppetBrowserBase> _browserLogger;
    private bool _disposed = false; // To detect redundant calls
    protected LauncherSettings _launchOptions;


    /// <summary>
    /// Gets the managed Puppeteer Browser instance.
    /// Null if not initialized or after disposal.
    /// </summary>
    protected IBrowser? Browser { get; private set; } // Changed setter to private, made explicitly nullable

      

   
    public PuppetBrowserBase(ILoggerFactory factory)
    {
        // Use NullLogger if null is passed, preventing NullReferenceExceptions later
        _browserLogger = factory.CreateLogger<PuppetBrowserBase>() ?? NullLogger<PuppetBrowserBase>.Instance;
    }

    /// <summary>
    /// Initializes the Puppeteer browser instance asynchronously.
    /// If the browser is already initialized, this method will return without creating a new instance.
    /// </summary>
    /// <exception cref="Exception">Propagates exceptions from PuppeteerSharp during browser launch.</exception>
    public virtual async Task InitializeAsync(IOptions<LauncherSettings> launchOptions)
    {
        _launchOptions = launchOptions.Value; // Store the launch options for later use
        // Prevent re-initialization
        if (Browser != null)
        {
            _browserLogger.LogWarning("InitializeAsync called but browser is already initialized.");
            return;
        }

        if (_disposed)
        {
            _browserLogger.LogError("InitializeAsync called on a disposed instance.");
            throw new ObjectDisposedException(nameof(PuppetBrowserBase));
        }

        ArgumentNullException.ThrowIfNull(launchOptions, nameof(launchOptions));



      //  var fetcher=Puppeteer.CreateBrowserFetcher(new BrowserFetcherOptions());
      //  var browserTask = fetcher.DownloadAsync();
      //  _browserLogger.LogInformation("BrowserFetcher downloaded version: {Version}", browserTask);
        // Check if the browser is already downloaded
   
     //   await browserTask;

       // _launchOptions.ExecutablePath= browserTask.Result.GetExecutablePath();

        try
        {
            _browserLogger.LogInformation("Initializing browser...");
            //_browserLogger.LogDebug("Using LaunchOptions: Headless={Headless}, Width={Width}, Height={Height}",
        

            //_launchOptions.ExecutablePath = fetcher.GetExecutablePath(browserTask.ToString());

            // Initialize the browser with the provided launch options
          //  var dargs = Puppeteer.GetDefaultArgs();
            Browser= await Puppeteer.LaunchAsync(new LaunchOptions{ Headless = false, UserDataDir = _launchOptions.UserDataDir});

           
            

            //Browser = await Puppeteer.LaunchAsync(_launchOptions);
            Browser.DefaultWaitForTimeout = 60_000;
            
            _browserLogger.LogInformation("Browser initialized successfully. Endpoint: {Endpoint}", Browser.WebSocketEndpoint);
        }
        catch (Exception ex)
        {
            _browserLogger.LogError(ex, "Error initializing browser.");
            Browser = null; // Ensure browser is null if initialization fails
            throw; // Re-throw the exception after logging
        }
    }

       








       

    /// <summary>
    /// Placeholder method intended for setting up network monitoring.
    /// This base implementation throws NotImplementedException.
    /// Derived classes can override this to provide specific monitoring logic.
    /// </summary>
    protected virtual void ConfigureNetworkMonitor() // Changed to protected virtual
    {
        // Method will setup a network monitor mechanism that will watch
        // for invalid connection situations and prevent browser instantiation or renew it.
        _browserLogger.LogWarning("ConfigureNetworkMonitor is not implemented in the base class.");
        // Keep or remove based on actual need. If keeping, derived classes should override.
        // throw new NotImplementedException("Network monitoring setup is not implemented.");
    }

    // --- IAsyncDisposable Implementation (Standard Pattern) ---

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        // Do not change this code. Put cleanup code in 'DisposeAsyncCore' method.
        await DisposeAsync(disposing: true);
        GC.SuppressFinalize(this); // Suppress finalization since we've cleaned up
    }

    /// <summary>
    /// Protected disposable implementation pattern.
    /// </summary>
    /// <param name="disposing">True if called from DisposeAsync(), false if called from finalizer.</param>
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects).
                _browserLogger.LogInformation("Disposing browser resources (disposing={Disposing})...", disposing);
                if (Browser != null)
                {
                    try
                    {
                        await Browser.CloseAsync(); // Prefer CloseAsync then DisposeAsync for browser
                        await Browser.DisposeAsync();
                        _browserLogger.LogInformation("Browser disposed successfully.");
                    }
                    catch (Exception ex)
                    {
                        // Log error but do not throw from Dispose
                        _browserLogger.LogError(ex, "Error disposing browser instance. Swallowing exception.");
                    }
                    finally
                    {
                        Browser = null; // Set to null even if disposal failed
                    }
                }
                else
                {
                    _browserLogger.LogDebug("Browser instance was already null. No browser disposal needed.");
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // Example: if you had file handles or pointers

            _disposed = true; // Mark as disposed
        }
        else
        {
            _browserLogger.LogDebug("DisposeAsync called on an already disposed instance.");
        }
    }

    // Optional: Override finalizer only if you have unmanaged resources
    // ~PuppetBrowserBase()
    // {
    //     // Do not change this code. Put cleanup code in 'DisposeAsync(bool disposing)' method
    //     DisposeAsync(disposing: false).AsTask().Wait(); // Or use other sync-over-async approach carefully
    // }
}