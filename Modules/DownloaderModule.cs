// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Retry;



namespace MediaRecycler.Modules;


/// <summary>
///     Handles concurrent downloading of files with retry, queue persistence, and robust error handling.
/// </summary>
public class DownloaderModule : IAsyncDisposable, Interfaces.IDownloaderModule
{

    private readonly SemaphoreSlim _concurrencySemaphore;
    private readonly object _failureLock = new();
    private readonly HttpClient _httpClient;
    private readonly CancellationTokenSource _masterCts;
    private readonly string _queueFilePath;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ConcurrentDictionary<Uri, Uri> _urlStates;
    private readonly ConcurrentBag<Task> _workerTasks;
    private readonly BlockingCollection<Uri> _workQueue;
    private int _consecutiveFailures;
    private bool _isDisposed;
    private CancellationTokenSource? _processingCts;
    private Task? _processingTask;







    /// <summary>
    ///     Initializes a new instance of the <see cref="DownloaderModule" /> class.
    /// </summary>
    public DownloaderModule()
    {
        _queueFilePath = Path.GetFullPath(DownloaderOptions.Default.QueueFilename);
        _httpClient = new HttpClient();
        ConfigureHttpClient(_httpClient);
        _urlStates = new ConcurrentDictionary<Uri, Uri>();
        _workQueue = new BlockingCollection<Uri>(new ConcurrentQueue<Uri>());
        _concurrencySemaphore = new SemaphoreSlim(DownloaderOptions.Default.MaxConcurrency, DownloaderOptions.Default.MaxConcurrency);
        _masterCts = new CancellationTokenSource();
        _workerTasks = [];

        _retryPolicy = Policy.Handle<HttpRequestException>(ShouldRetryHttpRequest).Or<IOException>().Or<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
                    .WaitAndRetryAsync(DownloaderOptions.Default.MaxRetries,
                                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)),
                                (exception, timespan, retryAttempt, context) =>
                                {
                                    var url = context.ContainsKey("Url") ? context["Url"] as Uri : null;
                                    Program.Logger.LogWarning(exception, "Retry {RetryAttempt}/{MaxRetries} for URL {Url}. Delaying for {Delay:ss\\.fff}s due to error: {ErrorMessage}",
                                                 retryAttempt, DownloaderOptions.Default.MaxRetries, url?.ToString() ?? "N/A", timespan, exception.Message);
                                    return Task.CompletedTask;
                                });
    }







    /// <summary>
    ///     Indicates if the downloader is currently running.
    /// </summary>
    public bool IsRunning { get; private set; }







    /// <summary>
    ///     Asynchronously disposes the downloader, ensuring all resources are released and the queue is saved.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        Program.Logger.LogInformation("Disposing DownloaderModule...");

        try
        {
            if (IsRunning)
            {
                await StopAsync().ConfigureAwait(false);
            }

            if (!_masterCts.IsCancellationRequested)
            {
                _masterCts.Cancel();
            }

            await SaveQueueToFileAsync();
        }
        finally
        {
            _workQueue.Dispose();
            _concurrencySemaphore.Dispose();
            _processingTask = null;
            _processingCts?.Dispose();
            _masterCts.Dispose();
            _httpClient.Dispose();
            _urlStates.Clear();
            Program.Logger.LogDebug("DownloaderModule disposed.");
            GC.SuppressFinalize(this);
        }
    }







    /// <summary>
    ///     Asynchronously creates a new <see cref="DownloaderModule" /> and loads the persisted queue state.
    /// </summary>
    public static async Task<DownloaderModule> CreateAsync()
    {
        var module = new DownloaderModule();
        await module.LoadQueueFromFileAsync();
        return module;
    }







    /// <summary>
    ///     Aborts the processing of the queue and cancels all ongoing tasks.
    /// </summary>
    private async Task AbortProcessingAsync(string reason)
    {
        Program.Logger.LogWarning("AbortProcessingAsync called. Reason: {Reason}", reason);
        await SaveQueueToFileAsync();

        if (_processingCts != null && !_processingCts.IsCancellationRequested)
        {
            _processingCts.Cancel();
            Program.Logger.LogInformation("Processing cancellation requested due to abort condition.");
        }
    }







    /// <summary>
    ///     Configures the <see cref="HttpClient" /> with appropriate headers and settings.
    /// </summary>
    private void ConfigureHttpClient(HttpClient client)
    {
        client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        client.DefaultRequestHeaders.Pragma.Add(new NameValueHeaderValue("no-cache"));
        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true, MustRevalidate = true };
        client.DefaultRequestHeaders.ConnectionClose = true;
        client.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow;
        client.DefaultRequestHeaders.Referrer = new Uri("https://www.bdsmlr.com");
        client.Timeout = TimeSpan.FromMinutes(5);
        client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.3029.110 Safari/537.36");
    }







    /// <summary>
    ///     Downloads the specified URL with retry logic and updates the state tracking upon success or failure.
    /// </summary>
    private async Task DownloadAndHandleRetriesAsync(Uri url, CancellationToken cancellationToken)
    {
        if (!_urlStates.ContainsKey(url))
        {
            Program.Logger.LogWarning("Attempted to download URL not found in state tracking: {Url}", url);
            return;
        }

        bool success = false;
        Program.Logger.LogInformation("DownloadAndHandleRetriesAsync started for {Url}", url);

        try
        {
            await _retryPolicy.ExecuteAsync(async (ctx, ct) => await PerformDownloadCoreAsync(url, ct).ConfigureAwait(false), new Context { ["Url"] = url }, cancellationToken)
                        .ConfigureAwait(false);

            success = true;
            Program.Logger.LogInformation("Successfully downloaded {Url}", url);

            if (_urlStates.TryRemove(url, out _))
            {
                Program.Logger.LogTrace("Removed successfully downloaded URL from state: {Url}", url);
            }

            lock (_failureLock)
            {
                _consecutiveFailures = 0;
            }
        }
        catch (Exception ex)
        {
            Program.Logger.LogInformation("Error during download attempt: {ErrorMessage}", ex);

            bool abort = false;

            lock (_failureLock)
            {
                _consecutiveFailures++;

                if (DownloaderOptions.Default.MaxConsecutiveFailures > 0 && _consecutiveFailures >= DownloaderOptions.Default.MaxConsecutiveFailures)
                {
                    abort = true;
                }
            }

            if (abort)
            {
                await AbortProcessingAsync($"Exceeded max consecutive failures ({DownloaderOptions.Default.MaxConsecutiveFailures}).");
            }
        }

        if (!success && !cancellationToken.IsCancellationRequested)
        {
            Program.Logger.LogError("Failed to download {Url} after exhausting retries or encountering a non-retried error. Skipping.", url);
        }
    }







    /// <summary>
    ///     Raised when the download queue count changes.
    /// </summary>
    public event EventHandler<string>? DownloadQueCountUpdated;







    /// <summary>
    ///     Adds a URL to the download queue if it hasn't been processed or isn't already queued.
    /// </summary>
    /// <param name="url">The URL to enqueue.</param>
    /// <returns>True if the URL was added, false if it was already present or module is stopping.</returns>
    public bool EnqueueUrl(Uri url)
    {
        if (_masterCts.IsCancellationRequested || _workQueue.IsAddingCompleted)
        {
            Program.Logger.LogWarning("Cannot enqueue URL {Url}, downloader is stopping or disposed.", url);
            return false;
        }

        if (_urlStates.TryAdd(url, url))
        {
            _workQueue.Add(url, _masterCts.Token);
            Program.Logger.LogTrace("Enqueued URL: {Url}", url);
            Program.Logger.LogInformation("EnqueueUrl called for {Url}. Queue size: {QueueSize}", url, _workQueue.Count);
            OnStatusUpdated($"Enqueued URL: {url}");
            OnDownloadQueCountUpdated(_workQueue.Count.ToString());
            return true;
        }

        Program.Logger.LogTrace("URL already in queue or processed: {Url}", url);
        OnStatusUpdated("Failed to queue url or duplicate");
        return false;
    }







    /// <summary>
    ///     Ensures the download directory exists, creating it if necessary.
    /// </summary>
    /// <returns>True if the directory exists or was created successfully, false otherwise.</returns>
    private bool EnsureDownloadDirectory()
    {
        try
        {
            if (!Directory.Exists(DownloaderOptions.Default.DownloadPath) && DownloaderOptions.Default.DownloadPath != null)
            {
                _ = Directory.CreateDirectory(DownloaderOptions.Default.DownloadPath);
            }

            Program.Logger.LogTrace("Ensured download directory exists: {DownloadPath}", Path.GetFullPath(DownloaderOptions.Default.DownloadPath));
            OnStatusUpdated($"Ensured download directory exists: {Path.GetFullPath(DownloaderOptions.Default.DownloadPath)}");
            return true;
        }
        catch (Exception ex)
        {
            Program.Logger.LogCritical(ex, "Failed to create download directory: {DownloadPath}. Aborting start.", Path.GetFullPath(DownloaderOptions.Default.DownloadPath));
            OnStatusUpdated($"Failed to create download directory: {Path.GetFullPath(DownloaderOptions.Default.DownloadPath)}. Aborting start.");
            return false;
        }
    }







    /// <summary>
    ///     Loads the persisted queue state from file, adding valid URLs to the work queue and state tracking.
    /// </summary>
    private async Task LoadQueueFromFileAsync()
    {
        if (!File.Exists(_queueFilePath))
        {
            Program.Logger.LogInformation("Queue persistence file not found, starting with an empty queue.");
            return;
        }

        try
        {
            Program.Logger.LogInformation("Loading queue state from {QueuePath}...", _queueFilePath);
            string json = await File.ReadAllTextAsync(_queueFilePath);
            var urlsToLoad = JsonSerializer.Deserialize<List<string>>(json);

            if (urlsToLoad != null)
            {
                int loadedCount = 0;
                int skippedCount = 0;

                foreach (string urlString in urlsToLoad)
                {
                    if (Uri.TryCreate(urlString, UriKind.Absolute, out var uri))
                    {
                        if (_urlStates.TryAdd(uri, uri))
                        {
                            _workQueue.Add(uri);
                            loadedCount++;
                        }
                        else
                        {
                            Program.Logger.LogWarning("Skipping URL from persistence file as it's already tracked: {Url}", uri);
                            skippedCount++;
                        }
                    }
                    else
                    {
                        Program.Logger.LogWarning("Failed to parse URL from persistence file: {UrlString}", urlString);
                        skippedCount++;
                    }
                }

                Program.Logger.LogInformation("Loaded {LoadedCount} URLs from persistence file. Skipped {SkippedCount} invalid or duplicate entries.", loadedCount, skippedCount);
            }
        }
        catch (Exception ex)
        {
            Program.Logger.LogError(ex, "Failed to load queue state from {QueuePath}. Starting with an empty queue.", _queueFilePath);
            _urlStates.Clear();

            while (_workQueue.TryTake(out _))
            {
            }
        }
    }







    /// <summary>
    ///     Handles logic to be executed after all worker tasks have completed, including cleanup and shutdown.
    /// </summary>
    private async Task OnAllWorkersCompletedAsync()
    {
        Program.Logger.LogInformation("All workers have exited. Queue is empty. DownloaderModule is stopping.");
        OnStatusUpdated("All downloads complete and queue is empty. DownloaderModule is stopping.");
        await StopAsync();
    }







    /// <summary>
    ///     Raises the <see cref="DownloadQueCountUpdated" /> event.
    /// </summary>
    private void OnDownloadQueCountUpdated(string message) => DownloadQueCountUpdated?.Invoke(this, message);







    /// <summary>
    ///     Raises the <see cref="StatusUpdated" /> event.
    /// </summary>
    private void OnStatusUpdated(string message) => StatusUpdated?.Invoke(this, message);







    /// <summary>
    ///     Performs the actual download of a file from the specified URL.
    /// </summary>
    private async Task PerformDownloadCoreAsync(Uri url, CancellationToken cancellationToken)
    {
        string fileName = Path.GetFileName(url.ToString());
        string filePath = Path.Combine(DownloaderOptions.Default.DownloadPath, fileName);

        if (File.Exists(filePath))
        {
            Program.Logger.LogInformation("Skipping duplicate file: {FilePath}", filePath);
            return;
        }

        Program.Logger.LogInformation("Starting download: {Url}", url);

        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

        await contentStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);

        Program.Logger.LogDebug("Saved {Url} to {FilePath}", url, filePath);
        OnStatusUpdated($"Saved {url} to {filePath}");
    }







    /// <summary>
    ///     Saves the current state of the download queue to a file for persistence.
    /// </summary>
    private async Task SaveQueueToFileAsync()
    {
        OnStatusUpdated("DownloaderModule::SaveQueueToFileAsync() started..");

        if (_workQueue.Count == 0)
        {
            return;
        }

        var urlsToSave = _workQueue.Where(uri => uri != null).Select(uri => uri.ToString()).ToList();

        if (!urlsToSave.Any())
        {
            Program.Logger.LogCritical("We were unable to process the work queue items and save the queue. NO QUEUE IS SAVED!");
            return;
        }

        try
        {
            string json = JsonSerializer.Serialize(urlsToSave, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_queueFilePath, json).ConfigureAwait(false);
            Program.Logger.LogInformation("Queue state saved successfully. {UrlCount} URLs saved to {QueuePath}.", urlsToSave.Count, _queueFilePath);
        }
        catch (OperationCanceledException)
        {
            Program.Logger.LogWarning("Saving queue state was cancelled.");
        }
        catch (Exception ex)
        {
            Program.Logger.LogError(ex, "Fatal Error! Download queue could NOT be saved to {QueuePath}", _queueFilePath);
        }
    }







    /// <summary>
    ///     Determines whether an HTTP request should be retried based on the exception.
    /// </summary>
    private bool ShouldRetryHttpRequest(HttpRequestException ex)
    {
        if (ex.StatusCode is null or HttpStatusCode.Forbidden)
        {
            return false;
        }

        int statusCode = (int)ex.StatusCode;
        return statusCode is >= 500 and <= 599 or (int)HttpStatusCode.RequestTimeout or (int)HttpStatusCode.TooManyRequests;
    }







    /// <summary>
    ///     Signals that no more URLs will be added to the queue for the current run.
    /// </summary>
    public void SignalNoMoreUrls()
    {
        if (!IsRunning)
        {
            Program.Logger.LogWarning("Cannot signal no more URLs, downloader is not started.");
            return;
        }

        if (_workQueue.IsAddingCompleted)
        {
            Program.Logger.LogDebug("SignalNoMoreUrls called, but adding was already completed.");
            return;
        }

        Program.Logger.LogInformation("Signaling that no more URLs will be added to the download queue.");
        _workQueue.CompleteAdding();
    }







    /// <summary>
    ///     Starts the download processing. Does nothing if already started.
    /// </summary>
    public void Start()
    {
        if (IsRunning || _masterCts.IsCancellationRequested)
        {
            Program.Logger.LogWarning("Downloader already started or is stopping/disposed.");
            OnStatusUpdated("Downloader already started or is stopping/disposed.");
            return;
        }

        Program.Logger.LogInformation("DownloaderModule starting with {WorkerCount} workers. Queue contains {QueueCount} items.", DownloaderOptions.Default.MaxConcurrency,
                     _workQueue.Count);
        OnStatusUpdated("Downloader Module is started...");
        IsRunning = true;
        _processingCts = CancellationTokenSource.CreateLinkedTokenSource(_masterCts.Token);

        if (!EnsureDownloadDirectory())
        {
            IsRunning = false;
            _processingCts?.Dispose();
            _processingCts = null;
            _processingTask = null;
            return;
        }

        StartWorkerTasks();
    }







    /// <summary>
    ///     Starts the initial set of worker tasks and manages their lifecycle.
    /// </summary>
    private void StartWorkerTasks()
    {
        for (int i = 0; i < DownloaderOptions.Default.MaxConcurrency; i++)
        {
            int workerId = i;
            var workerTask = Task.Run(async () =>
            {
                Program.Logger.LogInformation("Worker {WorkerId} started.", workerId);

                try
                {
                    while (!_processingCts.IsCancellationRequested && !_masterCts.IsCancellationRequested)
                    {
                        Uri uri;

                        try
                        {
                            uri = _workQueue.Take(_processingCts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            Program.Logger.LogInformation("Worker {WorkerId} cancelled.", workerId);
                            break;
                        }
                        catch (InvalidOperationException)
                        {
                            Program.Logger.LogInformation("Worker {WorkerId} detected queue completion.", workerId);
                            break;
                        }

                        Program.Logger.LogInformation("Worker {WorkerId} processing download: {Url}", workerId, uri);

                        try
                        {
                            await DownloadAndHandleRetriesAsync(uri, _processingCts.Token).ConfigureAwait(false);
                            Program.Logger.LogInformation("Worker {WorkerId} completed download: {Url}", workerId, uri);
                        }
                        catch (Exception ex)
                        {
                            Program.Logger.LogError(ex, "Worker {WorkerId} failed to download {Url}: {Error}", workerId, uri, ex.Message);
                            OnStatusUpdated($"Worker {workerId} failed: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    Program.Logger.LogInformation("Worker {WorkerId} exiting.", workerId);
                }
            }, _processingCts.Token);

            _workerTasks.Add(workerTask);
            OnStatusUpdated("Worker started. number: " + workerId);
        }

        _processingTask = Task.WhenAll(_workerTasks).ContinueWith(async _ => await OnAllWorkersCompletedAsync(), TaskScheduler.Default).Unwrap();
    }







    /// <summary>
    ///     Raised when the status of the downloader changes.
    /// </summary>
    public event EventHandler<string>? StatusUpdated;







    /// <summary>
    ///     Stops the download processing gracefully, saving the queue state.
    /// </summary>
    public async Task StopAsync()
    {
        if (!IsRunning || _masterCts.IsCancellationRequested)
        {
            await SaveQueueToFileAsync();
            Program.Logger.LogInformation("Downloader not running or already stopping/disposed.");
            return;
        }

        Program.Logger.LogInformation("StopAsync called. Waiting for all workers to finish. Queue size: {QueueSize}", _workQueue.Count);
        _workQueue.CompleteAdding();

        if (_processingCts != null && !_processingCts.IsCancellationRequested)
        {
            _processingCts.Cancel();
        }

        try
        {
            if (_processingTask != null)
            {
                await _processingTask.ConfigureAwait(false);
            }

            Program.Logger.LogInformation("All worker tasks completed.");
        }
        catch (OperationCanceledException)
        {
            Program.Logger.LogInformation("Worker tasks cancelled gracefully.");
        }
        catch (Exception ex)
        {
            Program.Logger.LogError(ex, "Exception occurred while waiting for worker tasks to complete.");

            if (ex is AggregateException aggEx)
            {
                foreach (var innerEx in aggEx.Flatten().InnerExceptions)
                {
                    Program.Logger.LogError(innerEx, "Inner exception from worker task.");
                }
            }
        }

        _processingCts?.Dispose();
        _processingTask = null;
        _processingCts = null;
        IsRunning = false;
        await SaveQueueToFileAsync().ConfigureAwait(false);
        Program.Logger.LogInformation("Downloader Module stopped.");
    }







    /// <summary>
    ///     Waits asynchronously for all currently queued downloads to complete processing.
    /// </summary>
    public async Task WaitForDownloadsAsync()
    {
        if (!IsRunning)
        {
            Program.Logger.LogWarning("Cannot wait for downloads, downloader is not started.");
            return;
        }

        Program.Logger.LogInformation("Waiting for all active download tasks to complete...");

        if (_processingTask != null)
        {
            await _processingTask.ConfigureAwait(false);
        }

        Program.Logger.LogInformation("All active download tasks have completed.");
    }

}