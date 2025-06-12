// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"



// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;



namespace MediaRecycler;


public partial class DownloaderSettingsForm : Form
{

    private readonly BindingSource bindingSource = [];
    private readonly DownloaderOptions settings = new();

    // ErrorProvider for validation feedback
    private ErrorProvider? _errorProvider;






    public DownloaderSettingsForm()
    {
        InitializeComponent();

        // Use a BindingSource to allow easy rebinding after loading
        bindingSource.DataSource = settings;

        // String properties
        _ = textBoxDownloadPath.DataBindings.Add("Text", bindingSource, "DownloadPath", false, DataSourceUpdateMode.OnPropertyChanged);
        _ = textBoxQueuePersistencePath.DataBindings.Add("Text", bindingSource, "QueuePersistencePath", false, DataSourceUpdateMode.OnPropertyChanged);

        // Integer properties
        _ = textBoxMaxConcurrency.DataBindings.Add("Text", bindingSource, "MaxConcurrency", false, DataSourceUpdateMode.OnPropertyChanged);
        _ = textBoxMaxRetries.DataBindings.Add("Text", bindingSource, "MaxRetries", false, DataSourceUpdateMode.OnPropertyChanged);
        _ = textBoxMaxConsecutiveFailures.DataBindings.Add("Text", bindingSource, "MaxConsecutiveFailures", false, DataSourceUpdateMode.OnPropertyChanged);

        // TimeSpan property (bind as seconds via helper property)
        _ = textBoxRetryDelay.DataBindings.Add("Text", this, nameof(RetryDelaySeconds), false, DataSourceUpdateMode.OnPropertyChanged);

        // Validation event handlers
        textBoxMaxConcurrency.Validating += ValidateIntTextBox;
        textBoxMaxRetries.Validating += ValidateIntTextBox;
        textBoxMaxConsecutiveFailures.Validating += ValidateIntTextBox;
        textBoxRetryDelay.Validating += ValidateDoubleTextBox;

        Load += DownloaderSettings_Load;
        FormClosing += DownloaderSettings_FormClosing;
        btn_save.Click += btn_save_Click;
    }






    // Helper property for binding TimeSpan as seconds
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public double RetryDelaySeconds
    {
        get => settings.RetryDelay.TotalSeconds;
        set => settings.RetryDelay = TimeSpan.FromSeconds(value);
    }

    private ErrorProvider errorProvider1
    {
        get
        {
            _errorProvider ??= new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink, ContainerControl = this };

            return _errorProvider;
        }
    }

    private static readonly string SettingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MediaRecycler", "downloader_settings.json");






    private void btn_save_Click(object? sender, EventArgs e)
    {
        SaveSettings();
        Close();
    }






    private void DownloaderSettings_FormClosing(object? sender, FormClosingEventArgs e)
    {
        SaveSettings();
    }






    private void DownloaderSettings_Load(object? sender, EventArgs e)
    {
        LoadSettings();

        // Reset the binding source to update all controls
        bindingSource.ResetBindings(false);

        // Also update the RetryDelaySeconds textbox manually
        textBoxRetryDelay.Text = settings.RetryDelay.TotalSeconds.ToString(CultureInfo.InvariantCulture);
    }






    private void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                var loaded = DownloaderOptions.Default;

                if (loaded != null)
                {
                    Program.Logger?.LogInformation("Settings loaded successfully from path:{path}", SettingsFilePath);

                    // Copy loaded values to the current settings instance
                    settings.DownloadPath = loaded.DownloadPath;
                    settings.MaxConcurrency = loaded.MaxConcurrency;
                    settings.MaxRetries = loaded.MaxRetries;
                    settings.QueuePersistencePath = loaded.QueuePersistencePath;
                    settings.MaxConsecutiveFailures = loaded.MaxConsecutiveFailures;
                    settings.RetryDelay = loaded.RetryDelay;
                }
            }
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show("Failed to load settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }






    private void SaveSettings()
    {
        Program.Logger?.LogInformation("Saving settings...");

        try
        {
            var dir = Path.GetDirectoryName(SettingsFilePath);

            if (!Directory.Exists(dir))
            {
                _ = Directory.CreateDirectory(dir!);
            }

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
            Program.Logger?.LogInformation("Settings saved successfully to path:{path}", SettingsFilePath);
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show("Failed to save settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (Application.OpenForms["MainForm"] is MainForm)
            {
                Program.Logger?.LogError(ex, "Failed to save downloader settings.");
            }
        }
    }






    // Double (seconds) validation for TimeSpan
    private void ValidateDoubleTextBox(object? sender, CancelEventArgs e)
    {
        if (sender is TextBox tb)
        {
            if (!double.TryParse(tb.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var val) || val < 0)
            {
                e.Cancel = true;
                errorProvider1.SetError(tb, "Please enter a valid non-negative number (seconds).");
            }
            else
            {
                errorProvider1.SetError(tb, "");
            }
        }
    }






    // Integer validation
    private void ValidateIntTextBox(object? sender, CancelEventArgs e)
    {
        if (sender is TextBox tb)
        {
            if (!int.TryParse(tb.Text, out var val) || val < 0)
            {
                e.Cancel = true;
                errorProvider1.SetError(tb, "Please enter a valid non-negative integer.");
            }
            else
            {
                errorProvider1.SetError(tb, "");
            }
        }
    }

}
