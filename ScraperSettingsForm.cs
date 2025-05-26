// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"



// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Options;



namespace MediaRecycler;


public partial class ScraperSettingsForm : Form
{

    private readonly Scraping _settings;






    public ScraperSettingsForm()
    {
        InitializeComponent();

        // Load settings instance
        _settings = Scraping.Default;
        BindControls();
    }








    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Save settings before closing
        _settings.Save();
        base.OnFormClosing(e);
    }






    private void BindControls()
    {
        // Clear previous bindings to avoid duplicates
        txtDefaultTimeout.DataBindings.Clear();
        txtDefaultPuppeteerTimeout.DataBindings.Clear();
        txtArchivePageUrlSuffix.DataBindings.Clear();
        txtPaginationSelector.DataBindings.Clear();
        txtGroupingSelector.DataBindings.Clear();
        txtTargetElementSelector.DataBindings.Clear();
        txtTargetPropertySelector.DataBindings.Clear();
        chkStartDownloader.DataBindings.Clear();
        txtStartingWebPage.DataBindings.Clear();
       

        // Numeric bindings with conversion
        var timeoutBinding = txtDefaultTimeout.DataBindings.Add(
                    "Text", _settings, nameof(Scraping.DefaultTimeout), true, DataSourceUpdateMode.OnPropertyChanged);
        timeoutBinding.Format += (s, e) => e.Value = e.Value?.ToString();
        timeoutBinding.Parse += (s, e) => e.Value = int.TryParse(e.Value?.ToString(), out var v) ? v : 0;

        var puppeteerTimeoutBinding = txtDefaultPuppeteerTimeout.DataBindings.Add(
                    "Text", _settings, nameof(Scraping.DefaultPuppeteerTimeout), true,
                    DataSourceUpdateMode.OnPropertyChanged);
        puppeteerTimeoutBinding.Format += (s, e) => e.Value = e.Value?.ToString();
        puppeteerTimeoutBinding.Parse += (s, e) => e.Value = int.TryParse(e.Value?.ToString(), out var v) ? v : 0;

        // String and bool bindings
        txtArchivePageUrlSuffix.DataBindings.Add("Text", _settings, nameof(Scraping.ArchivePageUrlSuffix), true,
                    DataSourceUpdateMode.OnPropertyChanged);
        txtPaginationSelector.DataBindings.Add("Text", _settings, nameof(Scraping.PaginationSelector), true,
                    DataSourceUpdateMode.OnPropertyChanged);
        txtGroupingSelector.DataBindings.Add("Text", _settings, nameof(Scraping.GroupingSelector), true,
                    DataSourceUpdateMode.OnPropertyChanged);
        txtTargetElementSelector.DataBindings.Add("Text", _settings, nameof(Scraping.TargetElementSelector), true,
                    DataSourceUpdateMode.OnPropertyChanged);
        txtTargetPropertySelector.DataBindings.Add("Text", _settings, nameof(Scraping.TargetPropertySelector), true,
                    DataSourceUpdateMode.OnPropertyChanged);
        chkStartDownloader.DataBindings.Add("Checked", _settings, nameof(Scraping.StartDownloader), true,
                    DataSourceUpdateMode.OnPropertyChanged);
        txtStartingWebPage.DataBindings.Add("Text", _settings, nameof(Scraping.StartingWebPage), true,
                    DataSourceUpdateMode.OnPropertyChanged);
       
    }






    private void btnSave_Click(object? sender, EventArgs e)
    {

        try
        {
            // Save the settings
            _settings.Save();

            MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while saving settings: {ex.Message}", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
        }
       
    }






    private void btnCancel_Click(object? sender, EventArgs e)
    {
        // Optionally reload settings to discard changes
        _settings.Reload();
        Close();
    }

}
