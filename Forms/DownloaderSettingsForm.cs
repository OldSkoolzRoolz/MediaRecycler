// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers



// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using System.ComponentModel;

using MediaRecycler.Modules.Options;



namespace MediaRecycler;


public partial class DownloaderSettingsForm : Form
{

    private readonly BindingSource bindingSource = [];
    private readonly DownloaderOptions settings = DownloaderOptions.Default;

    // ErrorProvider for validation feedback







    public DownloaderSettingsForm()
    {
        InitializeComponent();

        settings = DownloaderOptions.Default;
        BindControls();
    }







    private void BindControls()
    {



        // Clear previous bindings to avoid duplicates
        textBoxDownloadPath.DataBindings.Clear();
        textBoxQueuePersistencePath.DataBindings.Clear();
        textBoxMaxConcurrency.DataBindings.Clear();
        textBoxMaxRetries.DataBindings.Clear();
        textBoxMaxConsecutiveFailures.DataBindings.Clear();
        textBoxRetryDelay.DataBindings.Clear();
        // Rebind all controls
        _ = textBoxDownloadPath.DataBindings.Add("Text", settings, nameof(DownloaderOptions.Default.DownloadPath), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = textBoxQueuePersistencePath.DataBindings.Add("Text", settings, nameof(DownloaderOptions.Default.QueueFilename), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = textBoxMaxConcurrency.DataBindings.Add("Text", settings, nameof(DownloaderOptions.Default.MaxConcurrency), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = textBoxMaxRetries.DataBindings.Add("Text", settings, nameof(DownloaderOptions.Default.MaxRetries), true, DataSourceUpdateMode.OnPropertyChanged);
        _ = textBoxMaxConsecutiveFailures.DataBindings.Add("Text", settings, nameof(DownloaderOptions.Default.MaxConsecutiveFailures), true, DataSourceUpdateMode.OnPropertyChanged);
        //_ = textBoxRetryDelay.DataBindings.Add("Text", settings, nameof(RetryDelaySeconds), false, DataSourceUpdateMode.OnPropertyChanged);
    }




    // Helper property for binding TimeSpan as seconds
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public double RetryDelaySeconds
    {
        get => settings.RetryDelay.TotalSeconds;
        set => settings.RetryDelay = TimeSpan.FromSeconds(value);
    }








    private void btn_save_Click(object? sender, EventArgs e)
    {
        try
        {
            settings.Save();
            _ = MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception)
        {
            _ = MessageBox.Show("Error saving settings to file.");
            //Log.LogError(exception, "Error saving settings.");
            settings.ReloadSettings();
        }
    }
















}