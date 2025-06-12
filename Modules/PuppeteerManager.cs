// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;



namespace MediaRecycler.Modules;


/// <summary>
///     Manages the lifecycle of the Puppeteer IBrowser and IPage instances.
///     Its single responsibility is to launch and dispose of the browser correctly.
/// </summary>
public class PuppeteerManager : IAsyncDisposable
{

    private readonly IEventAggregator _aggregator;
    private bool _disposed;






    private PuppeteerManager(IEventAggregator aggregator)
    {
        _aggregator = aggregator;
    }






    public IBrowser? Browser { get; private set; }
    public IPage? Page { get; private set; }






    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _logger.LogInformation("Disposing Puppeteer resources...");

        try
        {
            if (Page is { IsClosed: false })
            {
                await Page.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error closing page: {ex.Message}");
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
            _logger.LogWarning($"Error closing browser: {ex.Message}");
            NotifyRecovery("Problem closing browser.");
        }

        _logger.LogInformation("Resources disposed.");
    }






    private static ILogger _logger;



    private static string BrowserWSEndpoint { get; set; }






    public static async Task<PuppeteerManager?> CreateAsync(IEventAggregator aggregator)
    {
        var manager = new PuppeteerManager(aggregator);
        _logger = Program.Logger;
        _ = HeadlessBrowserOptions.Default;
        var df = new LaunchOptions
        {
                    Headless = true,
                    ExecutablePath = "D:\\Chrome\\Win64-132.0.6834.83\\chrome-win64\\chrome.exe",
                    Timeout = 60000,
                    UserDataDir = "D:\\chromeuserdata",
                    DefaultViewport = new ViewPortOptions { Width = 1100, Height = 1000 },
                    ProtocolTimeout = 30000
        };

        try
        {
            Program.Logger.LogInformation("Launching browser...");

            manager.Browser = await Puppeteer.LaunchAsync(df);



            manager.Browser.Disconnected += manager.OnBrowserDisconnected;

            // TODO: Attempt to connect to an existing browser instance if available
            // store latest endpoint for recovery
            Properties.Settings.Default.LastUsedWSEndpoint = manager.Browser.WebSocketEndpoint;

            var pages = await manager.Browser.PagesAsync();
            manager.Page = pages.Length == 0 ? await manager.Browser.NewPageAsync() : pages.First();

            _ = await manager.Page.GoToAsync("http://www.google.com");


            return manager;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[FATAL] Failed to initialize Puppeteer: {ex.Message}");
            await manager.DisposeAsync();
            return null;
        }
    }






    public async Task EnsureBrowserAndPageAsync()
    {
        if (Browser == null || !Browser.IsConnected)
        {
            _logger.LogWarning("Browser is not connected. Reinitializing...");
            await DisposeAsync();
            var newManager = await CreateAsync(_aggregator);
            Browser = newManager.Browser;
            Page = newManager.Page;
        }
        else if (Page == null || Page.IsClosed)
        {
            _logger.LogWarning("Page is closed. Creating new page...");
            Page = await Browser.NewPageAsync();
        }
    }






    private void NotifyRecovery(string message)
    {
        _aggregator?.Publish(new PuppeteerRecoveryEvent { Message = message });
    }






    private void OnBrowserDisconnected(object? sender, EventArgs e)
    {
        _logger.LogWarning("Browser disconnected.");
        NotifyRecovery("Browser disconnected.");
        _ = ResetAsync();
    }






    public async Task ResetAsync()
    {
        await DisposeAsync();
        var newManager = await CreateAsync(_aggregator);
        Browser = newManager.Browser;
        Page = newManager.Page;
    }






    public class PuppeteerRecoveryEvent
    {

        public string Message { get; set; } = string.Empty;

    }

}
