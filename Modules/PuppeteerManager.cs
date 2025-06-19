// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;



namespace MediaRecycler.Modules;


/// <summary>
///     Manages the lifecycle of the Puppeteer IBrowser and IPage instances.
///     Its single responsibility is to launch and dispose of the browser correctly.
/// </summary>
public class PuppeteerManager : PuppetPageBase, IAsyncDisposable
{

    private readonly IEventAggregator _aggregator;
    private bool _disposed;







    public PuppeteerManager(IEventAggregator aggregator): base(Program.Logger)
    {
        _aggregator = aggregator;
        Init();
    }







    private static ILogger _logger = Program.Logger;



    //public IBrowser? Browser { get; private set; }
    public IPage? Page { get; private set; }







    private async void Init()
    {
        await InitializeAsync(HeadlessBrowserOptions.Default);
    }




    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _disposed = true;

        _logger.LogInformation("Disposing Puppeteer resources...");

        try
        {
            if (Page is { IsClosed: false }) await Page.CloseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error closing page: {ex.Message}");
            NotifyRecovery("Error closing page");
        }

        try
        {
            if (Browser != null) await Browser.CloseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error closing browser: {ex.Message}");
            NotifyRecovery("Problem closing browser.");
        }

        _logger.LogInformation("Resources disposed.");
    }

















    /// <summary>
    ///     Creates an instance of <see cref="PuppeteerManager" /> asynchronously, initializing the Puppeteer browser and page.
    /// </summary>
    /// <param name="aggregator">
    ///     The event aggregator used for managing event subscriptions and publications within the application.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the initialized
    ///     <see cref="PuppeteerManager" /> instance, or <c>null</c> if initialization fails.
    /// </returns>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs during the initialization of the Puppeteer browser or page.
    /// </exception>
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
        string? lastWsEndpoint = Properties.Settings.Default.LastUsedWSEndpoint;

        if (string.IsNullOrEmpty(lastWsEndpoint)) return null;

        try
        {
            _logger.LogInformation($"Attempting to connect to browser at {lastWsEndpoint}...");
            var browser = await Puppeteer.ConnectAsync(new ConnectOptions { BrowserWSEndpoint = lastWsEndpoint }).ConfigureAwait(false);
            _logger.LogInformation("Successfully connected to the browser.");
            return browser;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to connect to browser: {ex.Message}");
            NotifyRecovery("Failed to connect to browser.");

            //clear the last used endpoint if connection fails
            Properties.Settings.Default.LastUsedWSEndpoint = null;
            Properties.Settings.Default.Save();
            return null;
        }
    }









    private void NotifyRecovery(string message)
    {
        _aggregator?.Publish(new PuppeteerRecoveryEvent(message));
    }









}