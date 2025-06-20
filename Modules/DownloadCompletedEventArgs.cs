// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




namespace MediaRecycler.Modules;


/// <summary>
///     Provides data for the DownloadCompleted event.
/// </summary>
public class DownloadCompletedEventArgs(
            string url,
            string filePath,
            long fileSizeBytes) : EventArgs
{



    /// <summary>
    ///     The URL of the file that was successfully downloaded.
    /// </summary>
    public string Url { get; } = url;

    /// <summary>
    ///     The path to the downloaded file on the local disk.
    /// </summary>
    public string FilePath { get; } = filePath;

    /// <summary>
    ///     The size of the downloaded file in bytes.
    /// </summary>
    public long FileSizeBytes { get; } = fileSizeBytes;

}