// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




namespace MediaRecycler.Modules;


/// <summary>
///     Provides data for the DownloadFailed event.
/// </summary>
public class DownloadFailedEventArgs(
            string url,
            Exception exception) : EventArgs
{



    /// <summary>
    ///     The URL of the file that failed to download.
    /// </summary>
    public string Url { get; } = url;

    /// <summary>
    ///     The exception that caused the download to fail.
    /// </summary>
    public Exception Exception { get; } = exception;

}