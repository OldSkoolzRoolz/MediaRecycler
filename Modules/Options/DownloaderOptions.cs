// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using System.Configuration;



namespace MediaRecycler.Modules.Options;


/// <summary>
///     Represents configuration options for the downloader module.
/// </summary>
/// <remarks>
///     This class provides user-scoped settings for managing downloader behavior,
///     including download path, concurrency limits, retry policies, and queue persistence.
/// </remarks>
public class DownloaderOptions : ApplicationSettingsBase
{

    private const string DefaultDownloadPath = "d:\\Downloads";
    private const string DefaultQueuePersistencePath = "queue.json";
    private const string DefaultRetryDelay = "00:00:05";

    /// <summary>
    ///     Gets or sets the download path for the downloader.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue(DefaultDownloadPath)]
    public string? DownloadPath
    {
        get => (string?)this[nameof(DownloadPath)];
        set => this[nameof(DownloadPath)] = value;
    }

    /// <summary>
    ///     Gets or sets the maximum number of concurrent downloads.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("5")]
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
    [DefaultSettingValue(DefaultQueuePersistencePath)]
    public string? QueuePersistencePath
    {
        get => (string?)this[nameof(QueuePersistencePath)];
        set => this[nameof(QueuePersistencePath)] = value;
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

    /// <summary>
    ///     Gets or sets the delay between retry attempts.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue(DefaultRetryDelay)]
    public TimeSpan RetryDelay
    {
        get => (TimeSpan)this[nameof(RetryDelay)];
        set => this[nameof(RetryDelay)] = value;
    }

    /// <summary>
    ///     Gets the default configuration options for the downloader.
    /// </summary>
    /// <remarks>
    ///     The default options are thread-safe and can be used as a baseline for creating custom configurations.
    /// </remarks>
    public static DownloaderOptions Default { get; } = (DownloaderOptions)Synchronized(new DownloaderOptions());






    /// <summary>
    ///     Saves the current settings to the configuration file.
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            Save();
        }
        catch (ConfigurationErrorsException ex)
        {
            throw new InvalidOperationException("Failed to save settings.", ex);
        }
    }






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

}
