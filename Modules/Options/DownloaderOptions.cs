// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using System.ComponentModel;
using System.Configuration;



namespace MediaRecycler.Modules.Options;


/// <summary>
///     Represents configuration options for the downloader module.
/// </summary>
/// <remarks>
///     This class provides user-scoped settings for managing downloader behavior,
///     including download path, concurrency limits, retry policies, and queue persistence.
/// </remarks>
[SettingsGroupName("DownloadOptions")]
[SettingsProvider(typeof(LocalFileSettingsProvider))]
public class DownloaderOptions : ApplicationSettingsBase
{

    /// <summary>
    ///     Gets or sets the download path for the downloader.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue(DefaultDownloadPath)]
    public string DownloadPath
    {
        get => (string)this[nameof(DownloadPath)];
        set => this[nameof(DownloadPath)] = value;
    }

    /// <summary>
    ///     Gets or sets the maximum number of concurrent downloads.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("6")]
    public int MaxConcurrency
    {
        get => (int)this[nameof(MaxConcurrency)];
        set => this[nameof(MaxConcurrency)] = value;
    }

    /// <summary>
    ///     Gets or sets the maximum number of retry attempts for failed downloads.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("3")]
    public int MaxRetries
    {
        get => (int)this[nameof(MaxRetries)];
        set => this[nameof(MaxRetries)] = value;
    }

    /// <summary>
    ///     Gets or sets the file path for queue persistence.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue(DefaultQueueFilename)]
    public string QueueFilename
    {
        get => (string)this[nameof(QueueFilename)];
        set => this[nameof(QueueFilename)] = value;
    }

    /// <summary>
    ///     Gets or sets the maximum number of consecutive failures allowed.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("5")]
    public int MaxConsecutiveFailures
    {
        get => (int)this[nameof(MaxConsecutiveFailures)];
        set => this[nameof(MaxConsecutiveFailures)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue(DefaultRetryDelay)]
    public string RetryDelayString
    {
        get => (string)this[nameof(RetryDelayString)];
        set => this[nameof(RetryDelayString)] = value;
    }

    [Browsable(false)]
    public TimeSpan RetryDelay
    {
        get => TimeSpan.TryParse(RetryDelayString, out var ts) ? ts : TimeSpan.FromSeconds(5);
        set => RetryDelayString = value.ToString();
    }

    /// <summary>
    ///     Gets the default configuration options for the downloader.
    /// </summary>
    /// <remarks>
    ///     The default options are thread-safe and can be used as a baseline for creating custom configurations.
    /// </remarks>
    public static DownloaderOptions Default { get; } = (DownloaderOptions)Synchronized(new DownloaderOptions());

    private const string DefaultDownloadPath = "d:\\Downloads";
    private const string DefaultQueueFilename = "queue.json";
    private const string DefaultRetryDelay = "00:00:05";







    /// <summary>
    ///     Reloads the settings from the configuration file.
    /// </summary>
    public void ReloadSettings()
    {
        try
        {
            Reload();
        }
        catch (ConfigurationErrorsException ex)
        {
            throw new InvalidOperationException("Failed to reload settings.", ex);
        }
    }







    /// <summary>
    ///     Saves the current settings to the configuration file.
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            Validate();
            Save();
            base.Save();
        }
        catch (ConfigurationErrorsException ex)
        {
            throw new InvalidOperationException("Failed to save settings.", ex);
        }
    }







    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(DownloadPath) || !Path.IsPathRooted(DownloadPath)) throw new ArgumentException("DownloadPath must be a valid absolute path.");

        if (MaxConcurrency is <= 0 or > 100) throw new ArgumentOutOfRangeException(nameof(MaxConcurrency), "MaxConcurrency must be between 1 and 100.");

        if (MaxRetries is < 0 or > 10) throw new ArgumentOutOfRangeException(nameof(MaxRetries), "MaxRetries must be between 0 and 10.");


        if (MaxConsecutiveFailures is < 0 or > 20) throw new ArgumentOutOfRangeException(nameof(MaxConsecutiveFailures), "MaxConsecutiveFailures must be between 0 and 20.");

        if (!TimeSpan.TryParse(RetryDelayString, out var delay) || delay <= TimeSpan.Zero || delay > TimeSpan.FromHours(1))
            throw new ArgumentException("RetryDelay must be a valid TimeSpan greater than zero and less than or equal to 1 hour.");
    }

}