#region Header

// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"

#endregion



// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"



using MediaRecycler.Modules;



namespace MediaRecycler;


public partial class ScraperSettingsForm : Form
{

    ScraperSettings _settings;






    public ScraperSettingsForm()
    {
        InitializeComponent();
        BindControls();
    }






    private void BindControls()
    {
        txtDefaultTimeout.DataBindings.Add("Text", _settings, nameof(ScraperSettings.DefaultTimeout));
        txtDefaultPuppeteerTimeout.DataBindings.Add("Text", _settings, nameof(ScraperSettings.DefaultPuppeteerTimeout));
        txtArchivePageUrlSuffix.DataBindings.Add("Text", _settings, nameof(ScraperSettings.ArchivePageUrlSuffix));
        txtPaginationSelector.DataBindings.Add("Text", _settings, nameof(ScraperSettings.PaginationSelector));
        txtGroupingSelector.DataBindings.Add("Text", _settings, nameof(ScraperSettings.GroupingSelector));
        txtTargetElementSelector.DataBindings.Add("Text", _settings, nameof(ScraperSettings.TargetElementSelector));
        txtTargetPropertySelector.DataBindings.Add("Text", _settings, nameof(ScraperSettings.TargetPropertySelector));
        chkStartDownloader.DataBindings.Add("Checked", _settings, nameof(ScraperSettings.StartDownloader));
        txtStartingWebPage.DataBindings.Add("Text", _settings, nameof(ScraperSettings.StartingWebPage));
        txtUserDataDir.DataBindings.Add("Text", _settings, nameof(ScraperSettings.UserDataDir));
    }

}
