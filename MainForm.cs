// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using MediaRecycler.Modules;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;



namespace MediaRecycler;


public partial class MainForm : Form
{

    private readonly IEventAggregator _aggregator = new EventAggregator();
    private readonly ILogger<MainForm> _logger;
    private PuppeteerManager? _puppeteerManager;






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
    public MainForm(ILogger<MainForm> logger)
    {
        InitializeComponent();

        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;


        // subscribe to the aggregator for the events we are interested in and assign the handlers
        _aggregator.Subscribe<StatusMessage>(OnStatusMessageReceived);
        _aggregator.Subscribe<PageNumberMessage>(OnPageCountUpdates);





    }






#pragma warning disable IDE0032
    public RichTextBox MainLogRichTextBox => rtb_main;
#pragma warning restore IDE0032






    private void AppendToMainViewer(object? sender, string text)
    {
        if (InvokeRequired)
        {
            _ = BeginInvoke(() => AppendToMainViewer(sender, text));
            return;
        }

        MainLogRichTextBox.AppendText(text);
        MainLogRichTextBox.AppendText(Environment.NewLine);
        MainLogRichTextBox.ScrollToCaret();

    }






    private async void btn_5_Click(object sender, EventArgs e)
    {
        try
        {
            if (_puppeteerManager == null)
            {
                _puppeteerManager = await PuppeteerManager.CreateAsync(_aggregator);
                IWebAutomationService automationService = new PuppeteerAutomationService(_puppeteerManager?.Page ?? throw new InvalidOperationException("PuppeteerManager is not initialized."), _aggregator);
                await automationService.GoToAsync(ScrapingOptions.Default.StartingWebPage!);
                await automationService.DoSiteLoginAsync();
            }
        }
        catch (Exception exception)
        {
            _ = MessageBox.Show(exception.Message);

            if (_puppeteerManager != null)
            {
                await _puppeteerManager.DisposeAsync();
            }

        }



    }






    private async void btn_8_Click(object sender, EventArgs e)
    {
        try
        {
            if (_puppeteerManager != null)
            {

                await _puppeteerManager.DisposeAsync();
            }

            SetStatusLabelText(this, "PuppeteerManager has been disposed.");
        }
        catch (Exception exception)
        {
            AppendToMainViewer(this, exception.Message);
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

            downloadManager = await DownloaderModule.CreateAsync(_logger);
            downloadManager.StatusUpdated += AppendToMainViewer;
            downloadManager.DownloadQueCountUpdated += DlQueUpdated;
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
            downloadManager.DownloadQueCountUpdated -= DlQueUpdated;

            await downloadManager.DisposeAsync();

        }
    }






    /// <summary>
    ///     Handles the click event for the "Get Page" button, initiating the web scraping process.
    /// </summary>
    /// <remarks>
    ///     This method performs the following actions:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Initializes the scraping components asynchronously.</description>
    ///         </item>
    ///         <item>
    ///             <description>Subscribes to status and log update events for real-time feedback.</description>
    ///         </item>
    ///         <item>
    ///             <description>Executes the scraping process to retrieve the page source.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Logs any errors encountered during the process and ensures proper
    ///                 cleanup.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     The button's active status is toggled at the start and end of the
    ///     operation.
    /// </remarks>
    /// <param name="sender">The source of the event, typically the button that was clicked.</param>
    /// <param name="e">The event data associated with the click event.</param>
    private async void btn_GetPage_Click(object sender, EventArgs e)
    {
        Scrapers scraperso = null!;


        try
        {
            //_scrapers = await Scrapers.CreateAsync(_launcherSettings, _scraperSettings, _downloaderSettings, _logger);
            // _scrapers.StatusBarUpdated += SetStatusLabelText;
            // _scrapers.MainLogUpdated += AppendToMainViewer;


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






    private async void btn_scrape_Click(object sender, EventArgs e)
    {
        try
        {

            await using var manager = await PuppeteerManager.CreateAsync(_aggregator);

            if (manager == null)
            {
                MessageBox.Show("Unable to initialize the browser object.");
                return;
            }

            IWebAutomationService automationService = new PuppeteerAutomationService(manager.Page!, _aggregator);

            BlogScraper scraper = new(automationService, _aggregator, _logger);

            await scraper.DownloadCollectLinksAsync();



        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }

    }






    private async void Button1_Click(object sender, EventArgs e)
    {
        var s = sender as Button;
        s.Enabled = false; // Disable the button to prevent multiple clicks

        //ToggleActiveStatus(btn_load);

        try
        {

            // The 'await using' statement ensures the PuppeteerManager is always disposed correctly.
            await using var puppeteerManager = await PuppeteerManager.CreateAsync(_aggregator);

            // 1. Create the automation service, injecting the page from our manager.
            if (puppeteerManager.Page != null)
            {
                IWebAutomationService automationService = new PuppeteerAutomationService(puppeteerManager.Page, _aggregator);

                // 2. Create the scraper, injecting the automation service.
                await using var blogScraper = new BlogScraper(automationService, _aggregator, _logger);


                // Control passed to the specific scraper implementation. each implementation can have its own logic for scraping.
                await blogScraper.BeginScrapingTargetBlogAsync();
            }




        }
        catch (Exception ex)
        {
            // This will catch errors from the scraper or automation service
            // that were not handled by the retry logic.
            AppendToMainViewer(this, $"An unrecoverable error occurred in the main application: {ex.Message}");
        }
        finally
        {
            //Objects created with 'await using' will be disposed automatically here.
            AppendToMainViewer(this, "Example finished.");
        }



        //ToggleActiveStatus(btn_load);
        s.Enabled = true; // Re-enable the button after the operation is complete

    }






    private void DlQueUpdated(object? sender, string text)
    {
        if (InvokeRequired)
        {
            _ = BeginInvoke(() => DlQueUpdated(sender, text));
            return;
        }

        tb_dlque.Text = text;
    }






    private void downloaderSettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        //Menu Click DownloaderSettings > Form opening
        DownloaderSettingsForm downloaderSettings = new();
        _ = downloaderSettings.ShowDialog(this);

        _logger?.LogTrace("DownloaderSettings form closing.");
        downloaderSettings.Close(); // Dispose of the form after use
    }






    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Ensure PuppeteerManager is disposed when the form is closing
        _ = _puppeteerManager?.DisposeAsync();

        _logger?.LogInformation("MainForm is closing. PuppeteerManager disposed.");

        _aggregator.Unsubscribe<StatusMessage>(OnStatusMessageReceived);
        _aggregator.Unsubscribe<PageNumberMessage>(OnPageCountUpdates);


    }






    private void MainForm_Load(object sender, EventArgs e)
    {
        AppendToMainViewer(this, "ctor finished");
    }






    /// <summary>
    ///     A registered handler for <see cref="EventAggregator" />  Handles updates to the page count by processing the
    ///     provided <see cref="PageNumberMessage" />.
    /// </summary>
    /// <param name="pageNumberMessage">
    ///     The message containing the updated page number information.
    /// </param>
    private void OnPageCountUpdates(PageNumberMessage pageNumberMessage)
    {
        if (InvokeRequired)
        {
            _ = BeginInvoke(() => OnPageCountUpdates(pageNumberMessage));
        }

        tb_pages.Text = pageNumberMessage.PageNumber.ToString();
    }






    private void OnStatusMessageReceived(StatusMessage obj)
    {
        if (InvokeRequired)
        {
            _ = BeginInvoke(() => OnStatusMessageReceived(obj));
            return;
        }

        MainLogRichTextBox.AppendText($"{obj.Text}{Environment.NewLine}");
        MainLogRichTextBox.ScrollToCaret();
    }






    private void puppeteerSettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        PuppeteerSettingsForm puppetSettings = new();
        _ = puppetSettings.ShowDialog(this);
    }






    private void scraperSettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        //Menu Click ScraperSettings > Form opening
        _logger?.LogTrace("ScraperSettings form opening.");

        ScraperSettingsForm scraperSettingsForm = new();
        _ = scraperSettingsForm.ShowDialog(this);


    }






    public void SetStatusLabelText(object? sender, string text)
    {
        if (statusStrip1.InvokeRequired) // Use the parent control's InvokeRequired property
        {
            _ = BeginInvoke(() => SetStatusLabelText(sender, text));
            return;
        }

        tsl_status.Text = text;
    }

}
