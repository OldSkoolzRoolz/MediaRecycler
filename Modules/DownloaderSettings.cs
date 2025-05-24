// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using System.Configuration;

namespace MediaRecycler.Modules;

// --- Supporting Files (Assume these exist) ---

// Settings.cs (Example Structure)

public class DownloaderSettings : ApplicationSettingsBase
{
    /// <summary>
    ///     Path where downloaded files will be stored.
    /// </summary>
    public string? DownloadPath { get; set; } 

    /// <summary>
    ///     Maximum number of concurrent download tasks.
    /// </summary>
    public int MaxConcurrency { get; set; }  // Default concurrency

    /// <summary>
    ///     Maximum number of retry attempts for a single URL before skipping.
    /// </summary>
    public int MaxRetries { get; set; }  // Default retries

    /// <summary>
    ///     Path to the file used to persist the download queue state.
    /// </summary>
    public string? QueuePersistencePath { get; set; }  // Default persistence file

    /// <summary>
    ///     Maximum number of consecutive download failures before aborting processing.
    ///     Set to 0 or less to disable this check.
    /// </summary>
    public int MaxConsecutiveFailures { get; set; } // Default consecutive failure limit

    /// <summary>
    ///     Delay between retry attempts for a failed URL.
    /// </summary>
    public TimeSpan RetryDelay { get; set; }  // Default retry delay

    // Optional: Network monitoring settings if implemented
    // public string NetworkCheckHost { get; set; } = "www.google.com";
    // public TimeSpan NetworkCheckInterval { get; set; } = TimeSpan.FromMinutes(1);
}
