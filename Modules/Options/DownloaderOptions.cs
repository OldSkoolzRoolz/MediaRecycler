#region Header

// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers

#endregion



using System.Configuration;



namespace MediaRecycler.Modules.Options;


public class DownloaderOptions : ApplicationSettingsBase
{

    /// <summary>
    ///     Path where downloaded files will be stored.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("d:\\Downloads")] // Default download path set to "Downloads"
    public string? DownloadPath { get; set; }

    /// <summary>
    ///     Maximum number of concurrent download tasks.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("5")] // Default concurrency set to 5
    public int MaxConcurrency { get; set; } // Default concurrency

    /// <summary>
    ///     Maximum number of retry attempts for a single URL before skipping.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("3")] // Default retries set to 3
    public int MaxRetries { get; set; } // Default retries

    /// <summary>
    ///     Path to the file used to persist the download queue state.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("queue.json")] // Default persistence file name
    public string? QueuePersistencePath { get; set; } // Default persistence file

    /// <summary>
    ///     Maximum number of consecutive download failures before aborting processing.
    ///     Set to 0 or less to disable this check.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("5")] // Default consecutive failure limit of 5
    public int MaxConsecutiveFailures { get; set; } // Default consecutive failure limit

    /// <summary>
    ///     Delay between retry attempts for a failed URL.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("00:00:05")] // Default retry delay of 5 seconds
    public TimeSpan RetryDelay { get; set; } // Default retry delay

    // Optional: Network monitoring settings if implemented
    // public string NetworkCheckHost { get; set; } = "www.google.com";
    // public TimeSpan NetworkCheckInterval { get; set; } = TimeSpan.FromMi

}
