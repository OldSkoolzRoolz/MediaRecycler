// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;



namespace MediaRecycler;


public partial class PuppeteerSettingsForm : Form
{

    private readonly HeadlessBrowserOptions settings;






    public PuppeteerSettingsForm()
    {
        InitializeComponent();

        settings = HeadlessBrowserOptions.Default;
        BindControlsToSettings();
    }






    private ILogger logger => Program.Logger;






    private void AcceptButton_Click(object sender, EventArgs e)
    {
        // This method can be used to handle the Accept button click event
        // For example, you might want to validate settings or perform an action before closing the form.
        try
        {
            settings.SaveSettings();
            logger.LogInformation("Puppeteer settings saved successfully.");
            _ = MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close(); // Close the form after saving
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save Puppeteer settings.");
            _ = MessageBox.Show("Failed to save settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }






    private void BindControlsToSettings()

    {

        // CheckBox bindings (bool)
        _ = chkHeadless.DataBindings.Add("Checked", settings, nameof(settings.Headless), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = chkDevtools.DataBindings.Add("Checked", settings, nameof(settings.Devtools), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = chkIgnoreHTTPSErrors.DataBindings.Add("Checked", settings, nameof(settings.IgnoreHTTPSErrors), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = chkDumpIO.DataBindings.Add("Checked", settings, nameof(settings.DumpIO), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = chkIgnoreDefaultArgs.DataBindings.Add("Checked", settings, nameof(settings.IgnoreDefaultArgs), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = chkNoSandbox.DataBindings.Add("Checked", settings, nameof(settings.NoSandbox), true, DataSourceUpdateMode.OnPropertyChanged);

        // TextBox bindings (string/int)
        _ = txtExecutablePath.DataBindings.Add("Text", settings, nameof(settings.ExecutablePath), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtArgs.DataBindings.Add("Text", settings, nameof(settings.Args), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtUserDataDir.DataBindings.Add("Text", settings, nameof(settings.UserDataDir), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtDefaultViewport.DataBindings.Add("Text", settings, nameof(settings.DefaultViewport), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtTimeout.DataBindings.Add("Text", settings, nameof(settings.Timeout), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtPath.DataBindings.Add("Text", settings, nameof(settings.Path), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtRemoteDebuggingPort.DataBindings.Add("Text", settings, nameof(settings.RemoteDebuggingPort), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtRemoteDebuggingAddress.DataBindings.Add("Text", settings, nameof(settings.RemoteDebuggingAddress), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtRemoteDebuggingPipe.DataBindings.Add("Text", settings, nameof(settings.RemoteDebuggingPipe), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtWebSocketEndpoint.DataBindings.Add("Text", settings, nameof(settings.WebSocketEndpoint), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtUserAgent.DataBindings.Add("Text", settings, nameof(settings.UserAgent), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtLanguage.DataBindings.Add("Text", settings, nameof(settings.Language), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtWindowSize.DataBindings.Add("Text", settings, nameof(settings.WindowSize), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = txtWindowPosition.DataBindings.Add("Text", settings, nameof(settings.WindowPosition), true, DataSourceUpdateMode.OnPropertyChanged);

        // If you add more properties/controls, repeat the pattern above.
    }






    private void CancelButton_Click(object sender, EventArgs e)
    {
        // This method can be used to handle the Cancel button click event
        // For example, you might want to discard changes or simply close the form.
        settings.ReloadSettings(); // Reload settings to discard changes made in the form
        logger.LogInformation("Puppeteer settings reloaded, discarding changes.");
        Close(); // Close the form without saving changes

    }






    public new void FormClosing(object sender, FormClosingEventArgs e)
    {
        // Save settings when the form is closing
        try
        {
            settings.SaveSettings();
            logger.LogInformation("Puppeteer settings saved successfully.");
            _ = MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save Puppeteer settings.");
            _ = MessageBox.Show("Failed to save settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            e.Cancel = true; // Cancel closing if saving fails
        }
    }






    public void FormLoad(object sender, EventArgs e)
    {
        // This method can be used to initialize any additional settings or UI elements
        // when the form is loaded.
        logger.LogInformation("PuppeteerSettingsForm loaded with settings");
    }

}
