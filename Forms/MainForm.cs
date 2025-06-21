// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using MediaRecycler.Modules;
using MediaRecycler.Modules.Interfaces;
using MediaRecycler.Modules.Loggers;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;



namespace MediaRecycler;


public partial class MainForm : Form
{

    private readonly IEventAggregator _aggregator = new EventAggregator();
    private UrlDownloader _downloadManager;
    private IScraper _scraper;







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
    public MainForm()
    {
        InitializeComponent();



        // subscribe to the aggregator for the events we are interested in and assign the handlers
        _aggregator.Subscribe<StatusMessage>(OnStatusMessageReceived);
        _aggregator.Subscribe<PageNumberMessage>(OnPageCountUpdates);
        _aggregator.Subscribe<QueueCountMessage>(OnQueueCountUpdates);
        _aggregator.Subscribe<StatusBarMessage>(OnStatusBarMessageReceivced);





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
        _downloadManager = new UrlDownloader(_aggregator);

        // --- Subscribe to Events ---
        _downloadManager.DownloadCompleted += (o, args) =>
        {
            Program.Logger.LogInformation($"[SUCCESS] Downloaded: {args.Url}");
            Program.Logger.LogInformation($"          -> Saved to: {args.FilePath}");
            Program.Logger.LogInformation($"          -> Size: {args.FileSizeBytes / 1024.0:F2} KB");
        };

        _downloadManager.DownloadFailed += (o, downloadFailedEventArgs) =>
        {
            Program.Logger.LogInformation($"[FAILURE] Failed: {downloadFailedEventArgs.Url}");
            Program.Logger.LogInformation($"          -> Reason: {downloadFailedEventArgs.Exception.Message}");
        };

        _downloadManager.QueueFinished += (o, args) => { Program.Logger.LogInformation("\n--- All downloads have been processed. ---"); };

        try
        {
            Program.Logger?.LogInformation("Download button clicked.");


            // Example: If you have a DownloadManager or similar service, you would resolve and start it here.
            // This is a placeholder for actual download logic.
            AppendToMainViewer(this, "Starting download process...");
            await _downloadManager.LoadQueueAsync();

            SetStatusLabelText(this, "Download manager running...");
            await _downloadManager.StartDownloadsAsync();


            SetStatusLabelText(this, "Download Manager is complete.");

        }
        catch (Exception ex)
        {
            await _downloadManager.SaveQueueAsync();
            Program.Logger?.LogError(ex, "Error running download process.");
            AppendToMainViewer(this, $"Error running download: {ex.Message}");
            SetStatusLabelText(this, "Download failed.");

        }
        finally
        {
            await _downloadManager.DisposeAsync();

        }
    }







    private async void btn_QueueVids_Click(object sender, EventArgs e)
    {
        btn_QueVideos.Enabled = false;
        try
        {
            _aggregator.Publish(new StatusMessage("Queuing videos..."));


            IBlogScraper scraper = new BlogScraper(_aggregator);
            _scraper = scraper;

            if (_scraper is IBlogScraper bscraper)
            {
                await bscraper.DownloadCollectedLinksAsync();
            }
        }
        catch (Exception ex)
        {

            Program.Logger.LogError(ex, "An unhandled error occured. Attempting to save progress before aborting...");
            SetStatusLabelText(this, ex.Message);

            await _scraper.CancelAsync();
            await _scraper.DisposeAsync();
        }
        finally
        {
            _aggregator.Publish(new StatusMessage("Queuing videos complete."));
            SetStatusLabelText(null, "Video Queue finished.");
        }

        btn_QueVideos.Enabled = true;
    }







    private async void btn_Scrape_Click(object sender, EventArgs e)
    {
        btn_GetVidPages.Enabled = false;

        try
        {


            // 1. Create the automation service, injecting the page from our manager.

            // 2. Create the scraper, injecting the automation service.
            IBlogScraper bScraper = new BlogScraper(_aggregator);
            _scraper = bScraper; // Store the scraper instance in parent interface variable so it can be accessed by the form or other methods without knowing the specific type that has been created.

            // This is useful for polymorphism and allows us to call methods on the interface without needing to know the concrete implementation.

            if (_scraper is IBlogScraper scraper)
            {
                await scraper.BeginScrapingTargetBlogAsync();
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
            //call dispose on parent interface variable to ensure the scraper is disposed of correctly.
            if (_scraper != null)
            {
                await _scraper.DisposeAsync();

            }
            AppendToMainViewer(this, "Blog scraping action has completed...");
        }


        btn_GetVidPages.Enabled = true;
    }


    private void chk_OnClick(object sender, EventArgs e)
    {
        // This method is triggered when the checkbox is clicked.
        // It can be used to handle any specific logic related to the checkbox state change.
        if (sender is CheckBox checkBox)
        {
            Program.Logger?.LogDebug($"Checkbox '{checkBox.Name}' clicked. Checked: {checkBox.Checked}");
            if (checkBox.Checked)
            {
                Program.Logger?.LogInformation($"Checkbox '{checkBox.Name}' is checked.");
                ScrapingOptions.Default.SinglePageScan = true;
                ScrapingOptions.Default.SaveSettings();
            }
            else
            {
                ScrapingOptions.Default.SinglePageScan = false;
                ScrapingOptions.Default.SaveSettings();
                Program.Logger?.LogInformation($"Checkbox '{checkBox.Name}' is unchecked.");
            }
        }
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

    }







    private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Ensure resources are stopped and disposed of properly
        try
        {

            if (_scraper != null)
            {
                await _scraper.CancelAsync();
                await _scraper.DisposeAsync();
            }


            if (_downloadManager != null)
            {
                await _downloadManager.StopAllTasksAsync();
                await _downloadManager.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            Program.Logger?.LogError(ex, "Error during MainForm closing cleanup.");
        }
        finally
        {
            Program.Logger?.LogInformation("MainForm is closing. PuppeteerManager disposed.");
            _aggregator.Unsubscribe<StatusMessage>(OnStatusMessageReceived);
            _aggregator.Unsubscribe<PageNumberMessage>(OnPageCountUpdates);
        }
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
            return;
        }

        tb_pages.Text = pageNumberMessage.PageNumber.ToString();
    }







    private void OnQueueCountUpdates(QueueCountMessage obj)
    {
        if (InvokeRequired)
        {
            _ = BeginInvoke(() => OnQueueCountUpdates(obj));
            return;


        }

        tb_dlque.Text = obj.QueueCount.ToString();
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







    private void OnStatusBarMessageReceivced(StatusBarMessage message)
    {

        if (InvokeRequired)
        {
            _ = BeginInvoke(() => OnStatusBarMessageReceivced(message));
            return;

            tsl_status.Text = message.Text;
        }
    }





    private void puppeteerSettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        PuppeteerSettingsForm puppetSettings = new();
        _ = puppetSettings.ShowDialog(this);
    }







    private void scraperSettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        //Menu Click ScraperSettings > Form opening
        Program.Logger?.LogTrace("ScraperSettings form opening.");

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







    /// <summary>
    ///     Method to toggle the enabled state of buttons based on the sender and a boolean value.
    ///     It also attached a cancel event handler that is appropriate for the button clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="isEnabled"></param>
    private async Task ToggleButtonsAsync(object sender, bool isEnabled)
    {
        if (sender is Button button)
        {
            switch (button.Name)
            {
                case nameof(btn_QueVideos):
                    button.Enabled = isEnabled;

                    await _scraper.CancelAsync();
                    break;
                case nameof(btn_download):
                    button.Enabled = isEnabled;
                    await _downloadManager.StopAllTasksAsync();
                    break;
                case nameof(btn_GetVidPages):
                    button.Enabled = isEnabled;
                    break;
            }
        }
    }

    private async void btn_Testing_Click(object sender, EventArgs e)
    {
        btn_Testing.Enabled = false;
        //btn_Testing.Visible = false;

        _aggregator.Publish(new StatusMessage("Testing button clicked;"));
        try
        {
            Program.Logger?.LogInformation("Testing button clicked.");
            await Task.Delay(3000); // Simulate some work
            AppendToMainViewer(this, "Testing complete.");
        }
        catch (Exception ex)
        {
            Program.Logger?.LogError(ex, "Error during testing.");
            AppendToMainViewer(this, $"Error during testing: {ex.Message}");
        }
        finally
        {
            btn_Testing.Enabled = true;
            //    btn_Testing.Visible = true;
        }
    }



}