// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers

namespace MediaRecycler.Modules.Interfaces;

public interface IDownloaderModule : IAsyncDisposable
{
    bool IsRunning { get; }
    event EventHandler<string>? DownloadQueCountUpdated;
    event EventHandler<string>? StatusUpdated;
    bool EnqueueUrl(Uri url);
    void SignalNoMoreUrls();
    void Start();
    Task StopAsync();
    Task WaitForDownloadsAsync();
}