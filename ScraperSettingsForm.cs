#region Header

// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers

#endregion



// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"



using MediaRecycler.Modules.Options;



namespace MediaRecycler;


public partial class ScraperSettingsForm : Form
{

    private Scraping _settings;







    public ScraperSettingsForm()
    {
        InitializeComponent();
        LoadCurrentData();
        BindControls();
    }







    private void LoadCurrentData()
    {
        throw new NotImplementedException();
    }







    private void BindControls()
    {
        txtDefaultTimeout.DataBindings.Add("Text", _settings, nameof(Scraping.DefaultTimeout));
        txtDefaultPuppeteerTimeout.DataBindings.Add("Text", _settings, nameof(Scraping.DefaultPuppeteerTimeout));
        txtArchivePageUrlSuffix.DataBindings.Add("Text", _settings, nameof(Scraping.ArchivePageUrlSuffix));
        txtPaginationSelector.DataBindings.Add("Text", _settings, nameof(Scraping.PaginationSelector));
        txtGroupingSelector.DataBindings.Add("Text", _settings, nameof(Scraping.GroupingSelector));
        txtTargetElementSelector.DataBindings.Add("Text", _settings, nameof(Scraping.TargetElementSelector));
        txtTargetPropertySelector.DataBindings.Add("Text", _settings, nameof(Scraping.TargetPropertySelector));
        chkStartDownloader.DataBindings.Add("Checked", _settings, nameof(Scraping.StartDownloader));
        txtStartingWebPage.DataBindings.Add("Text", _settings, nameof(Scraping.StartingWebPage));
        txtUserDataDir.DataBindings.Add("Text", _settings, nameof(Scraping.UserDataDir));
    }







    private void btnSave_Click(object? sender, EventArgs e)
    {
        // Validate and save settings
        if (_settings == null)
        {
            MessageBox.Show("Settings are not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            // Assuming BindControls updates _settings from UI controls
            BindControls();

            // Save the settings (you may need to implement the actual save logic)
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
        throw new NotImplementedException();
    }

}
