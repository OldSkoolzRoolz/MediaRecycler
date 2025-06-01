// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers



// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Text.Json;
using System.Threading;

using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Polly;
using Polly.Retry;



namespace MediaRecycler.Modules;


// For NullLogger
// For jitter


public class DownloaderModule : IAsyncDisposable
{

    private readonly SemaphoreSlim _concurrencySemaphore;
    private readonly object _failureLock = new(); // Lock for modifying _consecutiveFailures
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _masterCts; // Master cancellation for the entire module
    private readonly string _queueFilePath;


    private readonly object _queueSaveLock = new(); // Add this field to the class for locking

      private readonly AsyncRetryPolicy _retryPolicy;

    private readonly DownloaderOptions _settings;

    private readonly ConcurrentDictionary<Uri, Uri> _urlStates; // Tracks URLs and prevents duplicates

    private readonly List<Task> _workerTasks;
    private readonly BlockingCollection<Uri> _workQueue; // Holds URLs ready to be processed
    private int _consecutiveFailures;
    private bool _isDisposed;
    private CancellationTokenSource? _processingCts; // Cancellation specifically for the processing loop (can be reset)
    private Task? _processingTask; // Task representing the completion of all workers for a run







    // --- Constructor ---
    public DownloaderModule(DownloaderOptions settings, ILogger? logger = null)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? NullLogger.Instance;

        // Validate settings
        if (string.IsNullOrWhiteSpace(_settings.DownloadPath))
        {
            throw new ArgumentException("DownloadPath cannot be empty.", nameof(settings));
        }

        if (string.IsNullOrWhiteSpace(_settings.QueuePersistencePath))
        {
            throw new ArgumentException("QueuePersistencePath cannot be empty.", nameof(settings));
        }

        if (_settings.MaxConcurrency <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings.MaxConcurrency), "MaxConcurrency must be positive.");
        }



        _queueFilePath = Path.GetFullPath(_settings.QueuePersistencePath);
        _logger.LogDebug("Queue persistence path set to: {QueuePath}", _queueFilePath);

        _httpClient = new HttpClient(); // Use a single HttpClient instance
        ConfigureHttpClient(_httpClient);

        _urlStates = new ConcurrentDictionary<Uri, Uri>();
        _workQueue = new BlockingCollection<Uri>(new ConcurrentQueue<Uri>()); // Underlying queue is thread-safe
        _concurrencySemaphore = new SemaphoreSlim(_settings.MaxConcurrency, _settings.MaxConcurrency);
        _masterCts = new CancellationTokenSource();
        _workerTasks = [];




        // Load previous queue state synchronously in constructor (alternative: async factory pattern)
        if (File.Exists(_queueFilePath))
        {
            LoadQueueFromFileAsync().GetAwaiter().GetResult(); // Blocking call to load queue state
            _logger.LogInformation("Queue persistence file found at {QueuePath}. Loaded previous queue state.",
                        _queueFilePath);
            OnStatusUpdated($"Queue persistence file found at {_queueFilePath}. Loaded previous queue state.");

        }
        else
        {

            _logger.LogInformation("No queue persistence file found at {QueuePath}. Starting with an empty queue.",
                        _queueFilePath);
        }











        _retryPolicy = Policy
      .Handle<HttpRequestException>(ShouldRetryHttpRequest)
      .Or<IOException>()
      .Or<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
      .WaitAndRetryAsync(
                  _settings.MaxRetries,
                  retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)),
                  (exception, timespan, retryAttempt, context) =>
                  {
                      var url = context.ContainsKey("Url") ? context["Url"] as Uri : null;
                      _logger.LogWarning(exception,
                                  "Retry {RetryAttempt}/{MaxRetries} for URL {Url}. Delaying for {Delay:ss\\.fff}s due to error: {ErrorMessage}",
                                  retryAttempt, _settings.MaxRetries, url?.ToString() ?? "N/A", timespan, exception.Message);
                      return Task.CompletedTask;
                  });


    } // End of constructor







    public bool IsRunning { get; set; }








    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }


        _logger.LogInformation("Disposing DownloaderModule...");

        try
        {
            // Trigger graceful shutdown if not already stopped
            if (IsRunning)
            {
                await StopAsync().ConfigureAwait(false);
            }

            // Signal master cancellation if not already done
            if (!_masterCts.IsCancellationRequested)
            {
                _masterCts.Cancel();
            }

            await SaveQueueToFileAsync();
        }
        finally
        {




            // Dispose managed resources
            _workQueue.Dispose();
            _concurrencySemaphore.Dispose();
            _processingTask = null; // Ensure task reference is cleared
            _processingCts?.Dispose(); // Dispose if StopAsync wasn't called or didn't complete fully
            _masterCts.Dispose();
            _httpClient.Dispose();
            _workerTasks.Clear(); // Clear worker tasks list
            _urlStates.Clear(); // Clear URL states

            _logger.LogDebug("DownloaderModule disposed.");

            // Suppress finalization (if a finalizer were present)
            GC.SuppressFinalize(this);
            _isDisposed = true;
        }
    }







    private void ConfigureHttpClient(HttpClient client)
    {
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        _httpClient.DefaultRequestHeaders.Pragma.Add(new NameValueHeaderValue("no-cache"));
        _httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
        {
                    NoCache = true,
                    NoStore = true,
                    MustRevalidate = true
        };
        _httpClient.DefaultRequestHeaders.ConnectionClose = true; // Close connection after each request
        _httpClient.DefaultRequestHeaders.Date = DateTimeOffset.UtcNow; // Set current date for requests
        _httpClient.DefaultRequestHeaders.Referrer =
                    new Uri("https://www.bdsmlr.com"); // Set a referrer to avoid potential issues with some sites
        _httpClient.Timeout = TimeSpan.FromMinutes(5);

        //_httpClient.DefaultRequestHeaders.CacheControl.NoCache.Add(CacheControlHeaderValue.Parse("no-cache")); // Ensure no caching
        _httpClient.DefaultRequestHeaders.Add("user-agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.3029.110 Safari/537.36");



    }







    public event EventHandler<string>? StatusUpdated;







    private void OnStatusUpdated(string message)
    {
        StatusUpdated?.Invoke(this, message);
    }















    private async Task PerformDownloadCoreAsync(Uri url, CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(url.ToString());
        var filePath = Path.Combine(_settings.DownloadPath, fileName);

        _logger.LogInformation("Starting download: {Url}", url);

        using var response = await _httpClient
                    .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

        await contentStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("Saved {Url} to {FilePath}", url, filePath);
        OnStatusUpdated($"Saved {url} to {filePath}");
    }







    private bool ShouldRetryHttpRequest(HttpRequestException ex)
    {
        // Don't retry if there's no status code or if it's Forbidden (handled separately)
        if (ex.StatusCode is null or HttpStatusCode.Forbidden)
        {
            return false;
        }

        var statusCode = (int)ex.StatusCode;

        // Retry on 5xx server errors, 408 Timeout, 429 Too Many Requests
        return statusCode is >= 500 and <= 599 or
                    (int)HttpStatusCode.RequestTimeout or // 408
                    (int)HttpStatusCode.TooManyRequests; // 429

        // Explicitly DO NOT retry on other 4xx client errors like 404 Not Found, 401 Unauthorized, etc.
    }







    // --- Public Methods ---

    /// <summary>
    ///     Adds a URL to the download queue if it hasn't been processed or isn't already queued.
    /// </summary>
    /// <param name="url">The URL to enqueue.</param>
    /// <returns>True if the URL was added, false if it was already present or module is stopping.</returns>
    public bool EnqueueUrl(Uri url)
    {
        if (_masterCts.IsCancellationRequested || _workQueue.IsAddingCompleted)
        {
            _logger.LogWarning("Cannot enqueue URL {Url}, downloader is stopping or disposed.", url);
            return false;
        }

        if (_urlStates.TryAdd(url, url))
        {
            // Only add to work queue if it's new to our dupe watcher simply prevents duupe keys from being entered.
            _workQueue.Add(url, _masterCts.Token); // Use master token for adding
            _logger.LogTrace("Enqueued URL: {Url}", url);
            _logger.LogInformation("EnqueueUrl called for {Url}. Queue size: {QueueSize}", url, _workQueue.Count);
            OnStatusUpdated($"Enqueued URL: {url}");
            return true;
        }

        _logger.LogTrace("URL already in queue or processed: {Url}", url);
        OnStatusUpdated("Failed to queue url or duplicate");
        return false; // Already exists
    }







    /// <summary>
    ///     Starts the download processing. Does nothing if already started.
    ///     - Checks if the downloader is already running or stopping; if so, logs and returns.
    ///     - Logs the start, sets IsRunning, and creates a linked cancellation token source.
    ///     - Ensures the download directory exists, creating it if necessary.
    ///     - Starts the initial set of workers based on MaxConcurrency.
    ///     - Stores a Task representing the completion of all workers.
    /// </summary>
    public void Start()
    {
        if (IsRunning || _masterCts.IsCancellationRequested)
        {
            _logger.LogWarning("Downloader already started or is stopping/disposed.");
            OnStatusUpdated("Downloader already started or is stopping/disposed.");
            return;
        }

        _logger.LogInformation(
                    "DownloaderModule starting with {WorkerCount} workers. Queue contains {QueueCount} items.",
                    _settings.MaxConcurrency, _workQueue.Count);
        _logger.LogInformation("Downloader Module is started...");
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
    ///     Ensures the download directory exists, creating it if necessary.
    /// </summary>
    /// <returns>True if the directory exists or was created successfully, false otherwise.</returns>
    private bool EnsureDownloadDirectory()
    {
        try
        {
            if (!Directory.Exists(_settings.DownloadPath) && _settings.DownloadPath != null)
            {
                _ = Directory.CreateDirectory(_settings.DownloadPath);
            }

            _logger.LogTrace("Ensured download directory exists: {DownloadPath}",
                        Path.GetFullPath(_settings.DownloadPath));
            OnStatusUpdated($"Ensured download directory exists: {Path.GetFullPath(_settings.DownloadPath)}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to create download directory: {DownloadPath}. Aborting start.",
                        Path.GetFullPath(_settings.DownloadPath));
            OnStatusUpdated(
                        $"Failed to create download directory: {Path.GetFullPath(_settings.DownloadPath)}. Aborting start.");
            return false;
        }
    }



    public static async Task<DownloaderModule> CreateAsync(DownloaderOptions settings, ILogger? logger = null)
    {
        var module = new DownloaderModule(settings, logger);
        await module.LoadQueueFromFileAsync();
        return module;
    }



    /// <summary>
    ///     Starts the initial set of worker tasks and manages their lifecycle.
    /// </summary>
    private void StartWorkerTasks()
    {
        var activeWorkers = new ConcurrentDictionary<Task, byte>();

        for (var i = 0; i < _settings.MaxConcurrency; i++)
        {
            var workerId = i; // Capture for logging
            var workerTask = Task.Run(async () =>
            {
                _logger.LogInformation("Worker {WorkerId} started.", workerId);

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
                            _logger.LogInformation("Worker {WorkerId} cancelled.", workerId);
                            break;
                        }
                        catch (InvalidOperationException)
                        {
                            _logger.LogInformation("Worker {WorkerId} detected queue completion.", workerId);
                            break;
                        }

                        _logger.LogInformation("Worker {WorkerId} processing download: {Url}", workerId, uri);

                        try
                        {
                            await _retryPolicy.ExecuteAsync(async (ctx, ct) =>
                            {
                                await PerformDownloadCoreAsync(uri, ct).ConfigureAwait(false);
                            }, new Context { ["Url"] = uri }, _processingCts.Token);

                            _logger.LogInformation("Worker {WorkerId} completed download: {Url}", workerId, uri);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Worker {WorkerId} failed to download {Url}: {Error}", workerId, uri,
                                        ex.Message);
                            OnStatusUpdated($"Worker {workerId} failed: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    _logger.LogInformation("Worker {WorkerId} exiting.", workerId);
                }
            }, _processingCts.Token);

            _workerTasks.Add(workerTask);
            OnStatusUpdated("Worker started. number: " + workerId);
        }

        // When all workers finish, trigger graceful shutdown
        _processingTask = Task.WhenAll(_workerTasks)
                    .ContinueWith(async _ => await OnAllWorkersCompletedAsync(), TaskScheduler.Default)
                    .Unwrap();
    }







    /// <summary>
    /// Handles logic to be executed after all worker tasks have completed, including cleanup and shutdown.
    /// </summary>
    /// <returns>A task that completes when all workers have finished and shutdown is complete.</returns>
    private async Task OnAllWorkersCompletedAsync()
    {
        // This runs after all workers have exited
        _workerTasks.Clear();
        _logger.LogInformation("All workers have exited. Queue is empty. DownloaderModule is stopping.");
        OnStatusUpdated("All downloads complete and queue is empty. DownloaderModule is stopping.");

        // Optionally, trigger any additional cleanup or events here
        await StopAsync(); // Ensure StopAsync is awaited to complete shutdown
    }







    /// <summary>
    ///     Stops the download processing gracefully, saving the queue state.
    /// </summary>
    /// <returns>A task that completes when shutdown is finished.</returns>
    public async Task StopAsync()
    {
        if (!IsRunning || _masterCts.IsCancellationRequested)
        {
            await SaveQueueToFileAsync();
            _logger.LogInformation("Downloader not running or already stopping/disposed.");
            return;
        }

        _logger.LogInformation("StopAsync called. Waiting for all workers to finish. Queue size: {QueueSize}",
                    _workQueue.Count);
        _logger.LogInformation("Stopping Downloader Module...");

        // Signal workers to stop processing new items
        _workQueue.CompleteAdding();

        // Signal cancellation for ongoing operations
        if (_processingCts != null && !_processingCts.IsCancellationRequested)
        {
            _processingCts.Cancel();
        }

        // Wait for worker tasks to finish (using the stored processing task)
        try
        {
            // Add a timeout? Consider what happens if a download hangs indefinitely.
            // Wait even if _processingTask is null (e.g., Start failed but StopAsync is called)
            if (_processingTask != null)
            {
                await _processingTask.ConfigureAwait(false);
            }

            _logger.LogInformation("All worker tasks completed.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker tasks cancelled gracefully.");

            // Expected if cancellation was triggered
        }
        catch (Exception ex)
        {
            // Log exceptions from worker tasks if they weren't handled internally
            _logger.LogError(ex, "Exception occurred while waiting for worker tasks to complete.");

            // AggregateException might be caught here if multiple tasks failed
            if (ex is AggregateException aggEx)
            {
                foreach (var innerEx in aggEx.Flatten().InnerExceptions)
                {
                    _logger.LogError(innerEx, "Inner exception from worker task.");
                }
            }
        }

        // Clean up resources related to the current run
        _workerTasks.Clear();
        _processingCts?.Dispose();
        _processingTask = null; // Clear the processing task
        _processingCts = null;
        IsRunning = false; // Mark as stopped

        // Save remaining queue state
        await SaveQueueToFileAsync().ConfigureAwait(false);

        _logger.LogInformation("Downloader Module stopped.");
    }







    /// <summary>
    ///     Signals that no more URLs will be added to the queue for the current run.
    ///     This allows worker tasks to complete when the queue becomes empty.
    /// </summary>
    public void SignalNoMoreUrls()
    {
        if (!IsRunning)
        {
            _logger.LogWarning("Cannot signal no more URLs, downloader is not started.");
            return;
        }

        if (_workQueue.IsAddingCompleted)
        {
            _logger.LogDebug("SignalNoMoreUrls called, but adding was already completed.");
            return;
        }

        _logger.LogInformation("Signaling that no more URLs will be added to the download queue.");
        _workQueue.CompleteAdding();
    }







    /// <summary>
    ///     Waits asynchronously for all currently queued downloads to complete processing.
    ///     This should typically be called after SignalNoMoreUrls().
    /// </summary>
    /// <returns>A task that completes when the download processing finishes.</returns>
    public async Task WaitForDownloadsAsync()
    {
        if (!IsRunning)
        {
            _logger.LogWarning("Cannot wait for downloads, downloader is not started.");
            return;
        }

        _logger.LogInformation("Waiting for all active download tasks to complete...");

        if (_processingTask != null)
        {
            await _processingTask.ConfigureAwait(false); // Wait for the Task.WhenAll task created in Start()
        }

        _logger.LogInformation("All active download tasks have completed.");
    }







    // --- Core Processing Logic ---

    /// <summary>
    /// Processes the work queue, acquiring concurrency slots and handling downloads for each URL in the queue.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the processing loop.</returns>
    private async Task ProcessWorkQueueAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Worker task started.");

        try
        {
            foreach (var url in _workQueue.GetConsumingEnumerable(cancellationToken))
            {
                await _concurrencySemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogTrace("Concurrency slot acquired for {Url}", url);

                try
                {
                    await DownloadAndHandleRetriesAsync(url, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    _ = _concurrencySemaphore.Release();
                    _logger.LogTrace("Concurrency slot released for {Url}", url);
                }
            }

        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker task cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in worker task loop.");

            // Consider triggering a module shutdown here if the error is critical
            // AbortProcessing("Critical worker error");
        }
        finally
        {
            _logger.LogDebug("Worker task finished.");
        }
    }







    /// <summary>
    /// Handles downloading a URL with retry logic and updates state tracking on success or failure.
    /// </summary>
    /// <param name="url">The URL to download.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the download and retry operation.</returns>
    private async Task DownloadAndHandleRetriesAsync(Uri url, CancellationToken cancellationToken)
    {
        if (!_urlStates.ContainsKey(url))
        {
            _logger.LogWarning("Attempted to download URL not found in state tracking: {Url}", url);
            return;
        }

        var success = false;
        Context pollyContext = new($"Download-{url}") { ["Url"] = url };

        _logger.LogInformation("DownloadAndHandleRetriesAsync started for {Url}", url);

        try
        {
            await PerformDownloadCoreAsync(url, cancellationToken).ConfigureAwait(false);

            success = true;
            _logger.LogInformation("Successfully downloaded {Url}", url);

            if (_urlStates.TryRemove(url, out _))
            {
                _logger.LogTrace("Removed successfully downloaded URL from state: {Url}", url);
            }

            // Reset consecutive failures on success
            lock (_failureLock)
            {
                _consecutiveFailures = 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Error during download attempt: {ErrorMessage}", ex);

            // Increment consecutive failures on error
            var abort = false;

            lock (_failureLock)
            {
                _consecutiveFailures++;

                if (_settings.MaxConsecutiveFailures > 0 &&
                    _consecutiveFailures >= _settings.MaxConsecutiveFailures)
                {
                    abort = true;
                }
            }

            if (abort)
            {
                await AbortProcessingAsync($"Exceeded max consecutive failures ({_settings.MaxConsecutiveFailures}).");
            }
        }

        if (!success && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                        "Failed to download {Url} after exhausting retries or encountering a non-retried error. Skipping.",
                        url);
        }
    }







    /// <summary>
    ///     Aborts the processing of the queue and cancels all ongoing tasks.
    /// </summary>
    /// <example>If we receive too many errors or network failures, we will abort operations.</example>
    /// <param name="reason"></param>
    private async Task AbortProcessingAsync(string reason)
    {
        _logger.LogWarning("AbortProcessingAsync called. Reason: {Reason}", reason);

        // Saving the queue state during abnormal shutdown
        await SaveQueueToFileAsync();

        // Use the processing CTS to cancel current operations without affecting the master CTS
        if (_processingCts != null && !_processingCts.IsCancellationRequested)
        {
            _processingCts.Cancel();
            _logger.LogInformation("Processing cancellation requested due to abort condition.");
        }

        // Note: Saving the queue should happen when StopAsync is called or during disposal
    }










    /// <summary>
    /// Saves the current state of the download queue to a file for persistence.
    /// </summary>
    /// <returns>A task that completes when the queue state has been saved.</returns>
    private async Task SaveQueueToFileAsync()
    {
        OnStatusUpdated("DownloaderModule::SaveQueueToFileAsync() started..");

        List<string> urlsToSave;

        if (_workQueue.Count == 0)
        {
            //nothing to save , queue is empty aborting save operation
            return;
        }
        // Safely snapshot the URLs to save
        lock (_queueSaveLock)
        {
            urlsToSave = _workQueue
                        .Where(uri => uri != null) // Ensure only valid URLs are saved
                        .Select(uri => uri.ToString())
                        .ToList();
        }

        //If control reaches here, we have URLs still in the work queue to save. UrlsToSave should not be empty.
        if (!urlsToSave.Any())
        {
            _logger.LogCritical("We were unable to process the work queue items and save the queue. NO QUE IS SAVED!");
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(urlsToSave, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_queueFilePath, json).ConfigureAwait(false);
            _logger.LogInformation("Queue state saved successfully. {UrlCount} URLs saved to {QueuePath}.",
                        urlsToSave.Count, _queueFilePath);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Saving queue state was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal Error! Download queue could NOT be saved to {QueuePath}", _queueFilePath);
        }
    }







    // --- Other Methods ---

    /// <summary>
    /// Asynchronously creates a new <see cref="DownloaderModule"/> instance and loads the persisted queue state.
    /// </summary>
    /// <param name="settings">The downloader configuration options.</param>
    /// <param name="logger">The logger instance to use, or null for a null logger.</param>
    /// <returns>A task that returns the initialized <see cref="DownloaderModule"/>.</returns>

    /// <summary>
    /// Loads the persisted queue state from file, adding valid URLs to the work queue and state tracking.
    /// </summary>
    /// <returns>A task that completes when the queue state has been loaded.</returns>
    private async Task LoadQueueFromFileAsync()
    {
        if (!File.Exists(_queueFilePath))
        {
            _logger.LogInformation("Queue persistence file not found, starting with an empty queue.");
            return;
        }

        try
        {
            _logger.LogInformation("Loading queue state from {QueuePath}...", _queueFilePath);
            var json = await File.ReadAllTextAsync(_queueFilePath);
            var urlsToLoad = JsonSerializer.Deserialize<List<string>>(json);

            if (urlsToLoad != null)
            {
                var loadedCount = 0;
                var skippedCount = 0;

                foreach (var urlString in urlsToLoad)
                {
                    if (Uri.TryCreate(urlString, UriKind.Absolute, out var uri))
                    {
                        // Use EnqueueUrl logic to add to both state and work queue if new
                        if (_urlStates.TryAdd(uri, uri))
                        {
                            _workQueue.Add(uri); // Add to work queue as well
                            loadedCount++;
                        }
                        else
                        {
                            // Already present for some reason? Log it.
                            _logger.LogWarning("Skipping URL from persistence file as it's already tracked: {Url}",
                                        uri);
                            skippedCount++;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed to parse URL from persistence file: {UrlString}", urlString);
                        skippedCount++;
                    }
                }

                _logger.LogInformation(
                            "Loaded {LoadedCount} URLs from persistence file. Skipped {SkippedCount} invalid or duplicate entries.",
                            loadedCount, skippedCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load queue state from {QueuePath}. Starting with an empty queue.",
                        _queueFilePath);

            // Clear any partially loaded state to be safe
            _urlStates.Clear();

            // Clear the work queue (it might have items added during load attempt)
            while (_workQueue.TryTake(out _))
            {
            }
        }
    }

}
