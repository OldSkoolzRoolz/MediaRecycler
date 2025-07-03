// Project Name: MediaRecycler
// File Name: MainForm.cs
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using MediaRecycler.Logging;
using MediaRecycler.Modules.Interfaces;
using MediaRecycler.Modules.Loggers;
using MediaRecycler.Modules.Options;



namespace MediaRecycler;


public partial class MainForm : Form
{


    //private UrlDownloader? _downloadManager;
    //private readonly IScraper? _scraper;
    private readonly IBlogScraper _blogScraper;
    private readonly CancellationTokenSource cts = new();






    /// <inheritdoc />
    public MainForm(IBlogScraper blogScraper)
    {
        InitializeComponent();

        _blogScraper = blogScraper;

        // subscribe to the aggregator for the events we are interested in and assign the handlers

        Log.UIUpdateQueue += (i) => UpdateQueueDelegate(i);
        Log.UIUpdateLinkCount += (i) => UpdateLinkCount(i);
        Log.UIUpdatePageCount += (i) => UpdatePageCount(i);



    }






    private void UpdatePageCount(int i)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<int>(UpdatePageCount), i);
            return;
        }

        tb_pages.Text = i.ToString();
    }






    private void UpdateLinkCount(int i)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<int>(UpdateLinkCount), i);
            return;
        }

        tb_videos.Text = i.ToString();
    }






    private void UpdateQueueDelegate(int i)
    {
        if (InvokeRequired)
        {
           Invoke(new Action<int>(UpdateQueueDelegate),i);
        }

        tb_dlque.Text = i.ToString();
    }



#pragma warning disable IDE0032
    /// <summary>
    ///     Text box used for logging output in the main viewer.
    /// </summary>
    public RichTextBox MainLogRichTextBox => rtb_main;
#pragma warning restore IDE0032






    /// <summary>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="text"></param>
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






    private void btn_Cancel_Click(object sender, EventArgs e)
    {
        cts.Cancel();
    }






    private async void btn_download_Click(object sender, EventArgs e)
    {
        btn_Process.Enabled = false;

        try
        {




            await _blogScraper.DownloadCollectedLinksAsync();
        }
        catch (Exception ex)
        {

            Log.LogError(ex, "An unhandled error occured. Attempting to save progress before aborting...");



        }
        finally
        {


            Log.LogInformation("Video Queue finished.");
        }

        btn_Process.Enabled = true;
    }






    private async void btn_Process_Click(object sender, EventArgs e)
    {
        btn_download.Enabled = false;

        try
        {
            await _blogScraper.ExtractTargetLinksAsync(cts.Token);
        }
        catch (Exception)
        {


        }

        btn_download.Enabled = true;

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
    private void btn_queue_Click(object sender, EventArgs e)
    {


        /*
        _downloadManager = new UrlDownloader(_aggregator);

        // --- Subscribe to Events ---
        _downloadManager.DownloadCompleted += (o, args) =>
        {
            Log.LogInformation($"[SUCCESS] Downloaded: {args.Url}");
            Log.LogInformation($"          -> Saved to: {args.FilePath}");
            Log.LogInformation($"          -> Size: {args.FileSizeBytes / 1024.0:F2} KB");
        };

        _downloadManager.DownloadFailed += (o, downloadFailedEventArgs) =>
        {
            Log.LogInformation($"[FAILURE] Failed: {downloadFailedEventArgs.Url}");
            Log.LogInformation($"          -> Reason: {downloadFailedEventArgs.Exception.Message}");
        };

        _downloadManager.QueueFinished += (o, args) => { Log.LogInformation("\n--- All downloads have been processed. ---"); };

        try
        {
            _logger?.LogInformation("Download button clicked.");


            // Example: If you have a DownloadManager or similar service, you would resolve and start it here.
            // This is a placeholder for actual download logic.
            AppendToMainViewer(this, "Starting download process...");

            await _downloadManager.StartDownloadsAsync();



        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error running download process.");
            AppendToMainViewer(this, $"Error running download: {ex.Message}");

        }
        finally
        {
            await _downloadManager.DisposeAsync();

        }
        */
    }






    /// <summary>
    ///     Handles the click event of the scrape button, initiating the blog scraping process.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">The event arguments.</param>
    /// <remarks>
    ///     This method creates an instance of the blog scraper, starts the scraping process, and handles any exceptions that
    ///     may occur.
    ///     It also ensures that the scraper is properly disposed of after the process is complete.
    /// </remarks>
    private async void btn_Scrape_Click(object sender, EventArgs e)
    {
        btn_GetVidPages.Enabled = false;

        try
        {


            // 1. Create the automation service, injecting the page from our manager.

            // 2. Create the scraper, injecting the automation service.
            //  IBlogScraper? bScraper = new BlogScraper(_aggregator);
            // _scraper = bScraper; // Store the scraper instance in parent interface variable so it can be accessed by the form or other methods without knowing the specific type that has been created.

            // This is useful for polymorphism and allows us to call methods on the interface without needing to know the concrete implementation.


            await _blogScraper.BeginScrapingTargetBlogAsync(cts.Token);


        }
        catch (Exception ex)
        {
            // This will catch errors from the scraper or automation service
            // that were not handled by the retry logic.
            AppendToMainViewer(this, $"An unrecoverable error occurred in the main application: {ex.Message}");
        }
        finally
        {

            AppendToMainViewer(this, "Blog scraping action has completed...");
        }


        btn_GetVidPages.Enabled = true;
    }






    private void btn_Testing_Click(object sender, EventArgs e)
    {
        btn_Testing.Enabled = false;

        //btn_Testing.Visible = false;




    }






    private void chk_OnClick(object sender, EventArgs e)
    {
        // This method is triggered when the checkbox is clicked.
        // It can be used to handle any specific logic related to the checkbox state change.
        if (sender is CheckBox checkBox)
        {
            Log.LogDebug($"Checkbox '{checkBox.Name}' clicked. Checked: {checkBox.Checked}");

            if (checkBox.Checked)
            {
                Log.LogInformation($"Checkbox '{checkBox.Name}' is checked.");
                ScrapingOptions.Default.SinglePageScan = true;
                ScrapingOptions.Default.SaveSettings();
            }
            else
            {
                ScrapingOptions.Default.SinglePageScan = false;
                ScrapingOptions.Default.SaveSettings();
                Log.LogInformation($"Checkbox '{checkBox.Name}' is unchecked.");
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






    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Ensure resources are stopped and disposed of properly
        try
        {

            _blogScraper?.CancelAsync();
            _blogScraper?.DisposeAsync();


        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Error during MainForm closing cleanup.");
        }
        finally
        {
            Log.LogInformation("MainForm is closing. PuppeteerManager disposed.");
        }
    }






    private void MainForm_Load(object sender, EventArgs e)
    {
        AppendToMainViewer(this, "ctor finished");
    }









    private void OnStatusBarMessageReceivced(StatusBarMessage message)
    {

        if (InvokeRequired)
        {
            _ = BeginInvoke(() => OnStatusBarMessageReceivced(message));
            tsl_status.Text = message.Text;
        }
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
        Log.LogDebug("ScraperSettings form opening.");

        ScraperSettingsForm scraperSettingsForm = new();
        _ = scraperSettingsForm.ShowDialog(this);


    }

}