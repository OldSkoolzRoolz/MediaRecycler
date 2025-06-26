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