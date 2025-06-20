// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers

namespace MediaRecycler.Modules;

public interface IUrlDownloader
{
    int QueueCount { get; }

    event EventHandler<DownloadCompletedEventArgs>? DownloadCompleted;
    event EventHandler<DownloadFailedEventArgs>? DownloadFailed;
    event EventHandler? QueueFinished;

    ValueTask DisposeAsync();
    Task LoadQueueAsync();
    void QueueUrl(string url);
    void QueueUrls(IEnumerable<string> urls);
    Task SaveQueueAsync();
    Task StartDownloadsAsync();
    Task StopAllTasksAsync();
}