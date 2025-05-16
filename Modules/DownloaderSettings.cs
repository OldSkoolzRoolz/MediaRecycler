// Project Name: MediaRecycler
// Author:  Kyle Crowder [InvalidReference]
// **** Distributed under Open Source License ***
// ***   Do not remove file headers ***




namespace KC.Downloader;

// --- Supporting Files (Assume these exist) ---

// Settings.cs (Example Structure)

public class DownloaderSettings
{
    /// <summary>
    ///     Path where downloaded files will be stored.
    /// </summary>
    public string DownloadPath { get; set; } = "//Storage/Data/Downloads"; // Default path

    /// <summary>
    ///     Maximum number of concurrent download tasks.
    /// </summary>
    public int MaxConcurrency { get; set; } = 5; // Default concurrency

    /// <summary>
    ///     Maximum number of retry attempts for a single URL before skipping.
    /// </summary>
    public int MaxRetries { get; set; } = 3; // Default retries

    /// <summary>
    ///     Path to the file used to persist the download queue state.
    /// </summary>
    public string QueuePersistencePath { get; set; } = "./downloader_queue.json"; // Default persistence file

    /// <summary>
    ///     Maximum number of consecutive download failures before aborting processing.
    ///     Set to 0 or less to disable this check.
    /// </summary>
    public int MaxConsecutiveFailures { get; set; } = 10; // Default consecutive failure limit

    /// <summary>
    ///     Delay between retry attempts for a failed URL.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5); // Default retry delay

    // Optional: Network monitoring settings if implemented
    // public string NetworkCheckHost { get; set; } = "www.google.com";
    // public TimeSpan NetworkCheckInterval { get; set; } = TimeSpan.FromMinutes(1);
}
