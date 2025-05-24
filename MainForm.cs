// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using KC.Crawler;

using MediaRecycler.Modules;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace MediaRecycler;

public partial class MainForm : Form
{
    
    
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public static ILogger<MainForm>? Logger { get; set; }

    
    private readonly ScraperSettings _scraperSettings;
    private readonly LauncherSettings _launcherSettings;
    private readonly MiniFrontierSettings _frontierSettings;
    private readonly DownloaderSettings _downloaderSettings;

    public RichTextBox MainLogRichTextBox => rtb_main;






    public MainForm(IOptionsMonitor<ScraperSettings> scraperSettings,IOptionsMonitor<LauncherSettings> launcherSettings ,IOptionsMonitor<MiniFrontierSettings> frontierSettings, IOptionsMonitor<DownloaderSettings> downloaderSettings,ILogger<MainForm> logger)
    {
        InitializeComponent();
        Logger = logger;
        _scraperSettings = scraperSettings.CurrentValue;
        _launcherSettings = launcherSettings.CurrentValue;
        _frontierSettings = frontierSettings.CurrentValue;
        _downloaderSettings = downloaderSettings.CurrentValue;

        SetStatusLabelText("Ready");

    }

 














    // Fix for CS1061: 'ToolStripStatusLabel' does not contain a definition for 'InvokeRequired'.
    // Explanation: ToolStripStatusLabel does not inherit from Control, so it does not have the InvokeRequired property.
    // Instead, you should check the InvokeRequired property of the parent control (e.g., the Form or StatusStrip).
    public void SetStatusLabelText(string text)
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






    // Example usage in AppendToMainViewer
    private void AppendToMainViewer(string text)
    {
        //_logger.LogInformation("Appending text to main viewer: {Text}", text);

        if (rtb_main.InvokeRequired)
        {
            rtb_main.Invoke(new Action<string>(AppendToMainViewer), text);
        }
        else
        {
            rtb_main.AppendText(text);
        }
    }





    private void Button1_Click(object sender, EventArgs e) => Logger?.LogInformation("Button 1 clicked.");






    private void button4_Click(object sender, EventArgs e)
    {




        for (var x = 0; x < 10; x++)
        {
            AppendToMainViewer("Testing " + x);
        }
    }



    private void downloaderSettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        //Menu Click DownloaderSettings > Form opening
        DownloaderSettings downloaderSettings = new DownloaderSettings();
        downloaderSettings.ShowDialog(this);


        Logger?.LogTrace("DownloaderSettings form closing.");
        downloaderSettings.Close(); // Dispose of the form after use
    }

    private void scraperSettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        //Menu Click ScraperSettings > Form opening
        Logger?.LogTrace("ScraperSettings form opening.");
        ScraperSettings scraperSettings = new ScraperSettings();
        scraperSettings.ShowDialog(this);


        Logger?.LogTrace("ScraperSettings form closing.");
        scraperSettings.Close(); // Dispose of the form after use
    }

    private async void btn_GetPage_Click(object sender, EventArgs e)
    {
        // Create a new instance of the Scrapers class

     var scrapersobj =await Scrapers.CreateAsync();

        try
        {
            await scrapersobj.GetPageSourceAsync(_scraperSettings);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error initializing scrapers: {Message}", ex.Message);
            this.AppendToMainViewer("Error initializing scrapers: " + ex.Message);

            MessageBox.Show("Error initializing scrapers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        finally {             // Ensure the scrapers are disposed of properly
            await scrapersobj.DisposeAsync();
        }

    }
}
