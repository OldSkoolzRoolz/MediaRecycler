// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using MediaRecycler.Modules;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace MediaRecycler;


public partial class MainForm : Form
{

    private readonly DownloaderOptions _downloaderSettings;
    private readonly IOptionsMonitor<DownloaderOptions> _downloaderSettingsMonitor;
    private readonly MiniFrontierSettings _frontierSettings;
    private readonly IOptionsMonitor<MiniFrontierSettings> _frontierSettingsMonitor;
    private readonly HeadlessBrowserOptions _launcherSettings;
    private readonly IOptionsMonitor<HeadlessBrowserOptions> _launcherSettingsMonitor;
    private readonly ILogger<MainForm> _logger;


    private readonly Scraping _scraperSettings;
    private readonly IOptionsMonitor<Scraping> _scraperSettingsMonitor;






    public MainForm(
                IOptionsMonitor<Scraping> scraperSettings,
                IOptionsMonitor<MiniFrontierSettings> miniOptionsMonitor,
                IOptionsMonitor<HeadlessBrowserOptions> launcherSettings,
                IOptionsMonitor<DownloaderOptions> downloaderSettings,
                ILogger<MainForm> logger)
    {
        InitializeComponent();
        _logger = logger;
        _scraperSettingsMonitor = scraperSettings;
        _launcherSettingsMonitor = launcherSettings;
        _downloaderSettingsMonitor = downloaderSettings;
        _frontierSettingsMonitor = miniOptionsMonitor;

        AppendToMainViewer("Initializer finished");

    }






    public RichTextBox MainLogRichTextBox => rtb_main;






    public void SetStatusLabelText(
                string text)
    {
        if (statusStrip1.InvokeRequired) // Use the parent control's InvokeRequired property
        {
            statusStrip1.Invoke(new Action<string>(SetStatusLabelText), text);
        }
        else
        {
            tsl_status.Text = text;
        }
    }






    private void AppendToMainViewer(
                string text)
    {
        if (rtb_main.InvokeRequired)
        {
            rtb_main.Invoke(new Action<string>(AppendToMainViewer), text);
        }
        else
        {
            rtb_main.AppendText(text);
        }
    }






    private void Button1_Click(
                object sender,
                EventArgs e)
    {
        _logger?.LogInformation("Button 1 clicked.");
    }






    private void button4_Click(
                object sender,
                EventArgs e)
    {
        for (var x = 0; x < 10; x++)
        {
            AppendToMainViewer("Testing " + x);
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

        ScraperSettingsForm scraperSettingsForm = new(_scraperSettingsMonitor);
        scraperSettingsForm.ShowDialog(this);


    }






    private void btn_GetPage_Click(
                object sender,
                EventArgs e)
    {
        // Create a new instance of the Scrapers class

        // var scrapersobj =await Scrapers.CreateAsync(_launcherSettings, _scraperSettings, _downloaderSettings, _logger);

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

}
