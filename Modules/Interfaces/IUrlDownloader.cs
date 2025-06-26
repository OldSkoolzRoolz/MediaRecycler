// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers

namespace MediaRecycler.Modules.Interfaces;

public interface IUrlDownloader
{
    int QueueCount { get; }

    event EventHandler<DownloadCompletedEventArgs>? DownloadCompleted;
    event EventHandler<DownloadFailedEventArgs>? DownloadFailed;
    event EventHandler? QueueFinished;

    ValueTask DisposeAsync();
    void QueueUrl(string url);
    void QueueUrls(IEnumerable<string> urls);
    Task StartDownloadsAsync();
    Task StopAllTasksAsync();
}