// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers



// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using MediaRecycler.Modules.Options;



namespace MediaRecycler;


public partial class ScraperSettingsForm : Form
{

    private readonly ScrapingOptions _settings;






    public ScraperSettingsForm()
    {
        InitializeComponent();

        // Load settings instance
        _settings = ScrapingOptions.Default;
        BindControls();
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
        var timeoutBinding = txtDefaultTimeout.DataBindings.Add("Text", _settings, nameof(ScrapingOptions.Default.DefaultTimeout), true, DataSourceUpdateMode.OnPropertyChanged);
        timeoutBinding.Format += (s, e) => e.Value = e.Value?.ToString();
        timeoutBinding.Parse += (s, e) => e.Value = int.TryParse(e.Value?.ToString(), out int v) ? v : 0;

        var puppeteerTimeoutBinding = txtDefaultPuppeteerTimeout.DataBindings.Add("Text", _settings, nameof(ScrapingOptions.Default.DefaultPuppeteerTimeout), true, DataSourceUpdateMode.OnPropertyChanged);
        puppeteerTimeoutBinding.Format += (s, e) => e.Value = e.Value?.ToString();
        puppeteerTimeoutBinding.Parse += (s, e) => e.Value = int.TryParse(e.Value?.ToString(), out int v) ? v : 0;

        // String and bool bindings
        _ = txtArchivePageUrlSuffix.DataBindings.Add("Text", _settings, nameof(ScrapingOptions.Default.ArchivePageUrlSuffix), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtPaginationSelector.DataBindings.Add("Text", _settings, nameof(ScrapingOptions.Default.PaginationSelector), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtGroupingSelector.DataBindings.Add("Text", _settings, nameof(ScrapingOptions.Default.GroupingSelector), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtTargetElementSelector.DataBindings.Add("Text", _settings, nameof(ScrapingOptions.Default.TargetElementSelector), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtTargetPropertySelector.DataBindings.Add("Text", _settings, nameof(ScrapingOptions.Default.TargetPropertySelector), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = chkStartDownloader.DataBindings.Add("Checked", _settings, nameof(ScrapingOptions.Default.StartDownloader), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtStartingWebPage.DataBindings.Add("Text", _settings, nameof(ScrapingOptions.Default.StartingWebPage), true, DataSourceUpdateMode.OnPropertyChanged);

    }






    private void btnCancel_Click(object? sender, EventArgs e)
    {
        // Optionally reload settings to discard changes
        _settings.Reload();
        Close();
    }






    private void btnSave_Click(object? sender, EventArgs e)
    {

        try
        {
            // Save the settings
            _settings.Save();

            _ = MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"An error occurred while saving settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }






    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Save settings before closing
        _settings.SaveSettings();
        base.OnFormClosing(e);
    }

}
