// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using MediaRecycler.Modules;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace MediaRecycler;


public partial class MainForm : Form
{

    private readonly DownloaderOptions _downloaderSettings;
    private readonly MiniFrontierSettings _frontierSettings;
    private readonly HeadlessBrowserOptions _launcherSettings;
    private readonly ILogger<MainForm> _logger;
    private readonly Scraping _scraperSettings;
    private Scrapers? _scrapers;







    /// <summary>
    ///     Initializes a new instance of the <see cref="MainForm" /> class.
    /// </summary>
    /// <param name="scraperSettingsMonitor">
    ///     The settings monitor for scraper configuration, providing dynamic updates to scraper-related options.
    /// </param>
    /// <param name="miniOptionsMonitor">
    ///     The settings monitor for MiniFrontier configuration, enabling dynamic updates to frontier-related options.
    /// </param>
    /// <param name="launcherSettingsMonitor">
    ///     The settings monitor for headless browser configuration, allowing dynamic updates to browser-related options.
    /// </param>
    /// <param name="downloaderSettingsMonitor">
    ///     The settings monitor for downloader configuration, enabling dynamic updates to downloader-related options.
    /// </param>
    /// <param name="logger">
    ///     The logger instance for logging messages and errors related to the <see cref="MainForm" />.
    /// </param>
    public MainForm(
                IOptionsMonitor<Scraping> scraperSettingsMonitor,
                IOptionsMonitor<MiniFrontierSettings> miniOptionsMonitor,
                IOptionsMonitor<HeadlessBrowserOptions> launcherSettingsMonitor,
                IOptionsMonitor<DownloaderOptions> downloaderSettingsMonitor,
                ILogger<MainForm> logger)
    {
        ArgumentNullException.ThrowIfNull(scraperSettingsMonitor);
        ArgumentNullException.ThrowIfNull(miniOptionsMonitor);
        ArgumentNullException.ThrowIfNull(launcherSettingsMonitor);
        ArgumentNullException.ThrowIfNull(downloaderSettingsMonitor);
        ArgumentNullException.ThrowIfNull(logger);
        InitializeComponent();
        _logger = logger;
        _scraperSettings = scraperSettingsMonitor.CurrentValue;
        _launcherSettings = launcherSettingsMonitor.CurrentValue;
        _downloaderSettings = downloaderSettingsMonitor.CurrentValue;
        _frontierSettings = miniOptionsMonitor.CurrentValue;

        AppendToMainViewer(this, "Initializer finished");

    }







    public RichTextBox MainLogRichTextBox => rtb_main;







    public void SetStatusLabelText(object? sender,
                string text)
    {
        if (statusStrip1.InvokeRequired) // Use the parent control's InvokeRequired property
        {
            BeginInvoke(() => SetStatusLabelText(sender, text));
            return;
        }

        tsl_status.Text = text;
    }







    private void AppendToMainViewer(object? sender,
                string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendToMainViewer(sender, text));
            return;
        }

        rtb_main.AppendText(text);
        rtb_main.AppendText(Environment.NewLine);
        rtb_main.ScrollToCaret();

    }







    private void Button1_Click(
                object sender,
                EventArgs e)
    {
        _logger?.LogInformation("Button 1 clicked.");
        var path = "d:\\downloads";
        var path2 = "d:\\downloads\\masterkyle";
        var files = Directory.GetFiles(path);
        var subdir = Directory.GetFiles(path2);

        foreach (var file in files)
        {
            var fi = new FileInfo(file);

            if (File.Exists(Path.Combine(path2, fi.Name)))
            {
                File.Delete(file);
            }

            AppendToMainViewer(this, $"File {fi.Name} moved to {fi.Name}.mp4");
        }

    }







    private void button4_Click(
                object sender,
                EventArgs e)
    {
        for (var x = 0; x < 10; x++)
        {
            AppendToMainViewer(this, "Testing " + x);
        }
    }







    private void downloaderSettingsToolStripMenuItem_Click(
                object sender,
                EventArgs e)
    {
        //Menu Click DownloaderSettings > Form opening
        DownloaderSettingsForm downloaderSettings = new();
        downloaderSettings.ShowDialog(this);

        _logger?.LogTrace("DownloaderSettings form closing.");
        downloaderSettings.Close(); // Dispose of the form after use
    }







    private void scraperSettingsToolStripMenuItem_Click(
                object sender,
                EventArgs e)
    {
        //Menu Click ScraperSettings > Form opening
        _logger?.LogTrace("ScraperSettings form opening.");

        ScraperSettingsForm scraperSettingsForm = new();
        scraperSettingsForm.ShowDialog(this);


    }







    private async void btn_GetPage_Click(object sender, EventArgs e)
    {
        Scrapers scraperso = null!;


        try
        {
            _scrapers = await Scrapers.CreateAsync(_launcherSettings, _scraperSettings, _downloaderSettings, _logger);
            _scrapers.StatusBarUpdated += SetStatusLabelText;
            _scrapers.MainLogUpdated += AppendToMainViewer;

            await scraperso.InitializeAsync(_launcherSettings);

            await scraperso.GetPageSourceAsync();





        }
        catch (Exception exception)
        {
            _logger?.LogError(exception, "An error occurred during scraping initialization.");

        }
        finally
        {
            SetStatusLabelText(this, "Scraping Complete");
            _logger?.LogInformation("Scraping process finished.");
            await scraperso.DisposeAsync();
        }



    }







    private void puppeteerSettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        PuppeteerSettingsForm puppetSettings = new();
        puppetSettings.ShowDialog(this);
    }







    private async void btn_scrape_Click(object sender, EventArgs e)
    {
        await using var scrapers =
                    await Scrapers.CreateAsync(_launcherSettings, _scraperSettings, _downloaderSettings, _logger);
        var startingUrl = "https://chinkerbell.bdsmlr.com";
        scrapers.StatusBarUpdated += SetStatusLabelText;
        scrapers.MainLogUpdated += AppendToMainViewer;
        VideoLinkExtractor.ExtractionUpdates += AppendToMainViewer;

        try
        {
            await scrapers.ScrapeArchivePageAsync(startingUrl);


        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
        finally
        {
            SetStatusLabelText(this, "Scraping Complete");
            _logger?.LogInformation("Scraping process finished.");
            scrapers.StatusBarUpdated -= SetStatusLabelText;
            scrapers.MainLogUpdated -= AppendToMainViewer;
            VideoLinkExtractor.ExtractionUpdates -= AppendToMainViewer;

            await scrapers.DisposeAsync();
        }





    }







    /// <summary>
    ///     Handles the click event for the download button. Gets the Download services from DI container and calls the Start()
    ///     method to initiate the download process.
    /// </summary>
    /// <remarks>
    ///     This method is triggered when the user clicks the download button in the UI.  It performs
    ///     actions related to initiating a download process.
    /// </remarks>
    /// <param name="sender">The source of the event, typically the download button.</param>
    /// <param name="e">An <see cref="EventArgs" /> object containing event data.</param>
    private async void btn_download_Click(object sender, EventArgs e)
    {
        DownloaderModule? downloadManager = null!;

        try
        {
            _logger?.LogInformation("Download button clicked.");

            // Example: If you have a DownloadManager or similar service, you would resolve and start it here.
            // This is a placeholder for actual download logic.
            AppendToMainViewer(this, "Starting download process...");

            downloadManager = await DownloaderModule.CreateAsync(_downloaderSettings, _logger);
            downloadManager.StatusUpdated += AppendToMainViewer;
            downloadManager.Start();
            SetStatusLabelText(this, "Download manager started.");

            while (downloadManager.IsRunning)
            {
                // Wait for the download to complete or for a cancellation request
                await Task.Delay(1000); // Adjust the delay as needed
            }


        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error starting download process.");
            AppendToMainViewer(this, $"Error starting download: {ex.Message}");
            SetStatusLabelText(this, "Download failed.");
        }
        finally
        {
            downloadManager.StatusUpdated -= AppendToMainViewer;
            await downloadManager.DisposeAsync();

        }
    }







    /*

    private Task btn_GetPage_ClickAsync(
                object sender,
                EventArgs e)
    {
        // Create a new instance of the Scrapers class

        var scrapersobj =await Scrapers.CreateAsync(_launcherSettings, _scraperSettings, _downloaderSettings, _logger);

        try
        {
            //await scrapersobj.GetPageSourceAsync(_scraperSettings);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error initializing scrapers: {Message}", ex.Message);
            AppendToMainViewer("Error initializing scrapers: " + ex.Message);
        }
    }
    */

}
