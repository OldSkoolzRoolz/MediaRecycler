// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace MediaRecycler;
public partial class ScraperSettings : Form
{
    private readonly Modules.ScraperSettings settings = new Modules.ScraperSettings();
    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MediaRecycler", "scraper_settings.json");

    private readonly BindingSource bindingSource = new BindingSource();

    public ScraperSettings()
    {
        InitializeComponent();

        bindingSource.DataSource = settings;

        // Integer properties
        textBoxDefaultTimeout.DataBindings.Add("Text", bindingSource, "DefaultTimeout", false, DataSourceUpdateMode.OnPropertyChanged);
        textBoxDefaultPuppeteerTimeout.DataBindings.Add("Text", bindingSource, "DefaultPuppeteerTimeout", false, DataSourceUpdateMode.OnPropertyChanged);

        // String properties
        textBoxArchivePageUrlSuffix.DataBindings.Add("Text", bindingSource, "ArchivePageUrlSuffix", false, DataSourceUpdateMode.OnPropertyChanged);
        textBoxPaginationSelector.DataBindings.Add("Text", bindingSource, "PaginationSelector", false, DataSourceUpdateMode.OnPropertyChanged);
        textBoxGroupingSelector.DataBindings.Add("Text", bindingSource, "GroupingSelector", false, DataSourceUpdateMode.OnPropertyChanged);
        textBoxTargetElementSelector.DataBindings.Add("Text", bindingSource, "TargetElementSelector", false, DataSourceUpdateMode.OnPropertyChanged);
        textBoxTargetPropertySelector.DataBindings.Add("Text", bindingSource, "TargetPropertySelector", false, DataSourceUpdateMode.OnPropertyChanged);
        textBoxStartingWebPage.DataBindings.Add("Text", bindingSource, "StartingWebPage", false, DataSourceUpdateMode.OnPropertyChanged);
        textBoxUserDataDir.DataBindings.Add("Text", bindingSource, "UserDataDir", false, DataSourceUpdateMode.OnPropertyChanged);

        // Boolean property
        checkBoxStartDownloader.DataBindings.Add("Checked", bindingSource, "StartDownloader", false, DataSourceUpdateMode.OnPropertyChanged);

        // Validation event handlers
        textBoxDefaultTimeout.Validating += ValidateIntTextBox;
        textBoxDefaultPuppeteerTimeout.Validating += ValidateIntTextBox;
        textBoxStartingWebPage.Validating += ValidateRequiredTextBox;

        this.Load += ScraperSettings_Load;
        this.FormClosing += ScraperSettings_FormClosing;
        btn_save.Click += btn_save_Click;
    }

    private void ScraperSettings_Load(object? sender, EventArgs e)
    {
        LoadSettings();
        bindingSource.ResetBindings(false);
    }

    private void ScraperSettings_FormClosing(object? sender, FormClosingEventArgs e)
    {
        SaveSettings();
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                var loaded = JsonSerializer.Deserialize<Modules.ScraperSettings>(json);
                if (loaded != null)
                {
                    settings.DefaultTimeout = loaded.DefaultTimeout;
                    settings.DefaultPuppeteerTimeout = loaded.DefaultPuppeteerTimeout;
                    settings.ArchivePageUrlSuffix = loaded.ArchivePageUrlSuffix;
                    settings.PaginationSelector = loaded.PaginationSelector;
                    settings.GroupingSelector = loaded.GroupingSelector;
                    settings.TargetElementSelector = loaded.TargetElementSelector;
                    settings.TargetPropertySelector = loaded.TargetPropertySelector;
                    settings.StartDownloader = loaded.StartDownloader;
                    settings.StartingWebPage = loaded.StartingWebPage;
                    settings.UserDataDir = loaded.UserDataDir;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to load settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveSettings()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsFilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir!);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to save settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Integer validation
    private void ValidateIntTextBox(object? sender, CancelEventArgs e)
    {
        if (sender is TextBox tb)
        {
            if (!int.TryParse(tb.Text, out int val) || val < 0)
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

    // Required string validation
    private void ValidateRequiredTextBox(object? sender, CancelEventArgs e)
    {
        if (sender is TextBox tb)
        {
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                e.Cancel = true;
                errorProvider1.SetError(tb, "This field is required.");
            }
            else
            {
                errorProvider1.SetError(tb, "");
            }
        }
    }

    // ErrorProvider for validation feedback
    private ErrorProvider? _errorProvider;

    private void btn_save_Click(object? sender, EventArgs e)
    {
        SaveSettings();
        this.Close();
    }

    private ErrorProvider errorProvider1
    {
        get
        {
            if (_errorProvider == null)
            {
                _errorProvider = new ErrorProvider();
                _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
                _errorProvider.ContainerControl = this;
            }
            return _errorProvider;
        }
    }
}
