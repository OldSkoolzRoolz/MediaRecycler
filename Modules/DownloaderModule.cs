// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;

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

    private readonly AsyncRetryPolicy _retryPolicy;

    // --- Private State ---
    private readonly DownloaderSettings _settings;

    private readonly ConcurrentDictionary<Uri, DownloadAttemptInfo>
        _urlStates; // Tracks URLs and their attempt count, ensures uniqueness

    private readonly List<Task> _workerTasks;
    private readonly BlockingCollection<Uri> _workQueue; // Holds URLs ready to be processed
    private int _consecutiveFailures;
    private bool _isDisposed;
    private bool _isStarted;
    private CancellationTokenSource? _processingCts; // Cancellation specifically for the processing loop (can be reset)
    private Task? _processingTask; // Task representing the completion of all workers for a run






    // --- Constructor ---
    public DownloaderModule(DownloaderSettings settings, ILogger? logger = null)
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

        if (_settings.MaxRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings.MaxRetries), "MaxRetries cannot be negative.");
        }

        _queueFilePath = Path.GetFullPath(_settings.QueuePersistencePath);
        _logger.LogDebug("Queue persistence path set to: {QueuePath}", _queueFilePath);

        _httpClient = new HttpClient(); // Use a single HttpClient instance
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
        _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        _httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");



        _urlStates = new ConcurrentDictionary<Uri, DownloadAttemptInfo>();
        _workQueue = new BlockingCollection<Uri>(new ConcurrentQueue<Uri>()); // Underlying queue is thread-safe
        _concurrencySemaphore = new SemaphoreSlim(_settings.MaxConcurrency, _settings.MaxConcurrency);
        _masterCts = new CancellationTokenSource();
        _workerTasks = [];

        // Configure Polly retry policy
        _retryPolicy = Policy
            .Handle<HttpRequestException>(ShouldRetryHttpRequest)
            .Or<IOException>()
            .Or<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
            .WaitAndRetryAsync(
                _settings.MaxRetries,
                retryAttempt =>
                {
                    TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    TimeSpan jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
                    return delay + jitter;
                },
                async (exception, timespan, retryAttempt, context) =>
                {
                    Uri? url = context.ContainsKey("Url") ? context["Url"] as Uri : null;
                    _logger.LogWarning(exception,
                        "Retry {RetryAttempt}/{MaxRetries} for URL {Url}. Delaying for {Delay:ss\\.fff}s due to error: {ErrorMessage}",
                        retryAttempt, _settings.MaxRetries, url?.ToString() ?? "N/A", timespan, exception.Message);
                    await Task.CompletedTask;
                });


        // Load previous queue state synchronously in constructor (alternative: async factory pattern)
        if (File.Exists(_queueFilePath))
        {
            _logger.LogInformation("Queue persistence file found at {QueuePath}. Loading previous state...",
                _queueFilePath);
            _logger.LogInformation("Attempting to resume downloads in queue.");
            LoadQueueFromFile();
            Start();
        }
        else
        {

            _logger.LogInformation("No queue persistence file found at {QueuePath}. Starting with an empty queue.",
                _queueFilePath);
        }
    }






    // --- IAsyncDisposable ---






    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        _logger.LogInformation("Disposing DownloaderModule...");

        try
        {
            // Trigger graceful shutdown if not already stopped
            if (_isStarted)
            {
                await StopAsync().ConfigureAwait(false);
            }

            // Signal master cancellation if not already done
            if (!_masterCts.IsCancellationRequested)
            {
                _masterCts.Cancel();
            }
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
        }
    }






    private static string GetSafeFileName(Uri url)
    {
        string? fileName = Path.GetFileName(url.AbsolutePath);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = url.Host + ".html";
        }

        fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));

        // Add a hash to avoid collisions
        string? hash = url.GetHashCode().ToString("X8");
        return $"{fileName}_{hash}";
    }






    // Remove the [MemberNotNull] attribute as it is causing the CS8776 error.
    // Instead, ensure that the _settings.DownloadPath is validated and not null in the constructor.






    private async Task PerformDownloadCoreAsync(Uri url, CancellationToken cancellationToken)
    {
        // _settings.DownloadPath is already validated in the constructor, so no need for [MemberNotNull].
        string fileName = GetSafeFileName(url);
#pragma warning disable CS8604 // Possible null reference argument.
        string filePath = Path.Combine(_settings.DownloadPath, fileName);
#pragma warning restore CS8604 // Possible null reference argument.

        using HttpResponseMessage response = await _httpClient
            .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

        await contentStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("Saved {Url} to {FilePath}", url, filePath);
    }






    // Helper method to determine if an HttpRequestException should be retried
    private bool ShouldRetryHttpRequest(HttpRequestException ex)
    {
        // Don't retry if there's no status code or if it's Forbidden (handled separately)
        if (ex.StatusCode is null or HttpStatusCode.Forbidden)
        {
            return false;
        }

        int statusCode = (int)ex.StatusCode;

        // Retry on 5xx server errors, 408 Timeout, 429 Too Many Requests
        return statusCode is (>= 500 and <= 599) or
               ((int)HttpStatusCode.RequestTimeout) or // 408
               ((int)HttpStatusCode.TooManyRequests); // 429

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

        DownloadAttemptInfo attemptInfo = new(url);

        if (_urlStates.TryAdd(url, attemptInfo))
        {
            // Only add to work queue if it's new to our state tracker
            _workQueue.Add(url, _masterCts.Token); // Use master token for adding
            _logger.LogTrace("Enqueued URL: {Url}", url);
            return true;
        }

        _logger.LogTrace("URL already in queue or processed: {Url}", url);
        return false; // Already exists
    }






    /// <summary>
    ///     Starts the download processing. Does nothing if already started.
    /// </summary>
    public void Start()
    {
        if (_isStarted || _masterCts.IsCancellationRequested)
        {
            _logger.LogWarning("Downloader already started or is stopping/disposed.");
            return;
        }

        _logger.LogInformation("Downloader Module is started...");
        _isStarted = true;
        _processingCts = CancellationTokenSource.CreateLinkedTokenSource(_masterCts.Token);

        // Ensure download directory exists. Redundant already in the constructor, but here for clarity.

        try
        {
            if (!Directory.Exists(_settings.DownloadPath) && _settings.DownloadPath != null)
            {
                _ = Directory.CreateDirectory(_settings.DownloadPath);
            }

#pragma warning disable CS8604 // Possible null reference argument.
            _logger.LogTrace("Ensured download directory exists: {DownloadPath}",
                Path.GetFullPath(_settings.DownloadPath));

        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to create download directory: {DownloadPath}. Aborting start.",
                Path.GetFullPath(_settings.DownloadPath));
            _isStarted = false;
            _processingCts?.Dispose();
            _processingCts = null;
            _processingTask = null; // Ensure processing task is null if start fails
            return; // Cannot proceed without download directory
        }
#pragma warning restore CS8604 // Possible null reference argument.

        // Start worker tasks
        // Let exceptions propagate if task creation fails. The caller of Start should handle this.
        for (int i = 0; i < _settings.MaxConcurrency; i++)
        {
            Task? workerTask = Task.Run(() => ProcessWorkQueueAsync(_processingCts.Token), _processingCts.Token);
            _workerTasks.Add(workerTask);
        }

        // Store the task that represents the completion of all workers
        _processingTask = Task.WhenAll(_workerTasks);


    }






    /// <summary>
    ///     Stops the download processing gracefully, saving the queue state.
    /// </summary>
    /// <returns>A task that completes when shutdown is finished.</returns>
    public async Task StopAsync()
    {
        if (!_isStarted || _masterCts.IsCancellationRequested)
        {
            _logger.LogInformation("Downloader not running or already stopping/disposed.");
            return;
        }

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
                foreach (Exception? innerEx in aggEx.Flatten().InnerExceptions)
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
        _isStarted = false; // Mark as stopped

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
        if (!_isStarted)
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
        if (!_isStarted)
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






    private async Task ProcessWorkQueueAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Worker task started.");

        try
        {
            foreach (Uri? url in _workQueue.GetConsumingEnumerable(cancellationToken))
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
    /// </summary>
    /// <param name="url"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task DownloadAndHandleRetriesAsync(Uri url, CancellationToken cancellationToken)
    {
        if (!_urlStates.ContainsKey(url))
        {
            _logger.LogWarning("Attempted to download URL not found in state tracking: {Url}", url);
            return;
        }

        bool success = false;
        Context pollyContext = new($"Download-{url}") { ["Url"] = url };

        try
        {
            await _retryPolicy.ExecuteAsync(async (ctx, ct) =>
            {
                await PerformDownloadCoreAsync(url, ct).ConfigureAwait(false);
            }, pollyContext, cancellationToken);

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
            bool abort = false;

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
                AbortProcessing($"Exceeded max consecutive failures ({_settings.MaxConsecutiveFailures}).");
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
    private void AbortProcessing(string reason)
    {
        _logger.LogWarning("AbortProcessing called. Reason: {Reason}", reason);

        // Saving the queue state during abnormal shutdown
        SaveQueueToFileAsync().Wait();

        // Use the processing CTS to cancel current operations without affecting the master CTS
        if (_processingCts != null && !_processingCts.IsCancellationRequested)
        {
            _processingCts.Cancel();
            _logger.LogInformation("Processing cancellation requested due to abort condition.");
        }

        // Note: Saving the queue should happen when StopAsync is called or during disposal
    }






    // Placeholder for network monitoring - requires careful implementation
    private async Task CheckNetworkConditionAsync(CancellationToken cancellationToken)
    {
        // Basic check example (ping) - requires System.Net.NetworkInformation
        // This can be unreliable (ICMP blocked) and synchronous.
        // A better check might involve trying a HEAD request to a known reliable server.
        bool networkAvailable = NetworkInterface.GetIsNetworkAvailable(); // Quick OS check

        if (!networkAvailable)
        {
            _logger.LogWarning("Network condition check: NetworkInterface.GetIsNetworkAvailable() returned false.");
            AbortProcessing("Network appears unavailable (OS check).");
            return;
        }

        // Example: Try a HEAD request (less data than GET)
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Head, "https://www.google.com"); // Or configurable host
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5)); // Timeout for the check

            _ = await _httpClient.SendAsync(request, cts.Token).ConfigureAwait(false);
            _logger.LogTrace("Network condition check: HEAD request successful.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or
                                   OperationCanceledException)
        {
            _logger.LogWarning(ex, "Network condition check failed (HEAD request). Potential network issue.");
            AbortProcessing("Network check failed.");
        }
    }






    // --- Persistence ---






    private void LoadQueueFromFile()
    {
        if (!File.Exists(_queueFilePath))
        {
            _logger.LogInformation("Queue persistence file not found, starting with an empty queue.");
            return;
        }

        try
        {
            _logger.LogInformation("Loading queue state from {QueuePath}...", _queueFilePath);
            string json = File.ReadAllText(_queueFilePath);
            List<string>? urlsToLoad = JsonSerializer.Deserialize<List<string>>(json);

            if (urlsToLoad != null)
            {
                int loadedCount = 0;
                int skippedCount = 0;

                foreach (string urlString in urlsToLoad)
                {
                    if (Uri.TryCreate(urlString, UriKind.Absolute, out Uri? uri))
                    {
                        // Use EnqueueUrl logic to add to both state and work queue if new
                        if (_urlStates.TryAdd(uri, new DownloadAttemptInfo(uri)))
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






    /// <summary>
    ///     Saves the current state of the download queue to a file.
    /// </summary>
    /// <returns></returns>
    private async Task SaveQueueToFileAsync()
    {
        _logger.LogDebug("Saving queue state to {QueuePath}...", _queueFilePath);


        // Get URLs currently being tracked (not yet successfully downloaded or failed max retries)
        List<string> urlsToSave = _urlStates.Keys.Select(uri => uri.ToString()).ToList();

        if (!urlsToSave.Any())
        {
            _logger.LogInformation("Queue is empty, nothing to save to persistence file.");

            // Optionally delete the persistence file if it exists
            // working que empty so log and bail out.

            return;
        }

        try
        {
            _logger.LogDebug("Saving {UrlCount} pending URLs to {QueuePath}...", urlsToSave.Count, _queueFilePath);
            string json = JsonSerializer.Serialize(urlsToSave, new JsonSerializerOptions { WriteIndented = true });

            //TODO: Should we try and merge any new URLs into the existing file?
            // Write asynchronously
            await File.WriteAllTextAsync(_queueFilePath, json, _masterCts.Token)
                .ConfigureAwait(false); // Use master token for saving during shutdown
            _logger.LogInformation("Queue state saved successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Saving queue state was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal Error! Download Que could NOT be saved to {QueuePath}", _queueFilePath);
        }
    }






    public static async Task<DownloaderModule> CreateAsync(DownloaderSettings settings, ILogger? logger = null)
    {
        DownloaderModule module = new(settings, logger);
        await module.LoadQueueFromFileAsync();
        return module;
    }






    // Add the missing method 'LoadQueueFromFileAsync' to the DownloaderModule class.






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
            string json = await File.ReadAllTextAsync(_queueFilePath);
            List<string>? urlsToLoad = JsonSerializer.Deserialize<List<string>>(json);

            if (urlsToLoad != null)
            {
                int loadedCount = 0;
                int skippedCount = 0;

                foreach (string urlString in urlsToLoad)
                {
                    if (Uri.TryCreate(urlString, UriKind.Absolute, out Uri? uri))
                    {
                        // Use EnqueueUrl logic to add to both state and work queue if new
                        if (_urlStates.TryAdd(uri, new DownloadAttemptInfo(uri)))
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






    // --- Helper Class for State ---
    private class DownloadAttemptInfo
    {

        public DownloadAttemptInfo(Uri url)
        {
            Url = url;
        }






        public Uri Url { get; }
        public int Attempts { get; set; } = 0;

    }

}
