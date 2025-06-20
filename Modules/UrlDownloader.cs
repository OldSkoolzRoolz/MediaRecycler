﻿// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers



// To run this code, you need to add the Polly NuGet package to your project:
// dotnet add package Polly




using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;

using MediaRecycler.Modules.Loggers;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Retry;



namespace MediaRecycler.Modules;


/// <summary>
///     Manages a queue of URLs to download asynchronously with a specified level of concurrency.
///     Uses Polly for transient error handling and retries. Verifies download integrity by file size.
/// </summary>
public class UrlDownloader : IUrlDownloader, IAsyncDisposable
{

    private readonly IEventAggregator _aggregator;

    private readonly string _downloadDirectory;

    private readonly HttpClient _httpClient;
    private readonly int _maxConcurrency;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ConcurrentQueue<string> _urlQueue = new();







    /// <summary>
    ///     Initializes a new instance of the UrlDownloader.
    /// </summary>
    /// <param name="maxConcurrency">The maximum number of concurrent downloads. Must be greater than 0.</param>
    /// <param name="downloadDirectory">The directory where files will be saved. It will be created if it doesn't exist.</param>
    public UrlDownloader(IEventAggregator aggregator)
    {

        _aggregator = aggregator;
        _maxConcurrency = DownloaderOptions.Default.MaxConcurrency > 0 ? DownloaderOptions.Default.MaxConcurrency : throw new ArgumentOutOfRangeException(nameof(DownloaderOptions.Default.MaxConcurrency), "Max concurrency must be greater than 0.");
        _downloadDirectory = DownloaderOptions.Default.DownloadPath ?? throw new ArgumentNullException(nameof(DownloaderOptions.Default.DownloadPath), "Download directory must be specified.");

        // It's a best practice to reuse HttpClient instances.
        _httpClient = new HttpClient();
        ConfigureHttpClient(_httpClient);

        // Define a retry policy using Polly.
        // This policy will retry up to 3 times on HttpRequestException (network errors) 
        // or on HTTP 5xx server errors. It will use an exponential backoff strategy.
        _retryPolicy = Policy.Handle<HttpRequestException>().Or<IOException>().Or<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)),
                    (exception, timespan, retryAttempt, context) =>
                    {
                        var url = context.ContainsKey("Url") ? context["Url"] as Uri : null;
                        Program.Logger.LogWarning(exception, "Retry {RetryAttempt}/{MaxRetries} for URL {Url}. Delaying for {Delay:ss\\.fff}s due to error: {ErrorMessage}",
                                     retryAttempt, DownloaderOptions.Default.MaxRetries, url?.ToString() ?? "N/A", timespan, exception.Message);
                        return Task.CompletedTask;
                    });






        var _retryPolicy2 = Policy.Handle<HttpRequestException>(ShouldRetryHttpRequest).Or<IOException>().Or<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
              .WaitAndRetryAsync(DownloaderOptions.Default.MaxRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)),
                          (exception, timespan, retryAttempt, context) =>
                          {
                              var url = context.ContainsKey("Url") ? context["Url"] as Uri : null;
                              Program.Logger.LogWarning(exception, "Retry {RetryAttempt}/{MaxRetries} for URL {Url}. Delaying for {Delay:ss\\.fff}s due to error: {ErrorMessage}",
                                           retryAttempt, DownloaderOptions.Default.MaxRetries, url?.ToString() ?? "N/A", timespan, exception.Message);
                              return Task.CompletedTask;
                          });





    }







    public int QueueCount
    {
        get { return _urlQueue.Count; }
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
    ///     Performs application-defined tasks associated with freeing, releasing, or
    ///     resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await SaveQueueAsync();

        if (_httpClient is IAsyncDisposable httpClientAsyncDisposable)
        {
            await httpClientAsyncDisposable.DisposeAsync();
        }
        else
        {
            _httpClient.Dispose();
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
    ///     Fired when a file has been successfully downloaded and verified.
    /// </summary>
    public event EventHandler<DownloadCompletedEventArgs>? DownloadCompleted;

    /// <summary>
    ///     Fired when a file fails to download after all retry attempts.
    /// </summary>
    public event EventHandler<DownloadFailedEventArgs>? DownloadFailed;




    private object _queLock = new();


    /// <summary>
    ///     The core worker method. Each worker runs this method, continuously dequeuing
    ///     and processing URLs until the queue is empty.
    /// </summary>
    private async Task DownloadWorkerAsync()
    {
        while (true)
        {
            string? url = null;
            // Check if the queue is empty before processing.
            // If it is, we exit the worker loop.
            if (_urlQueue.IsEmpty)
            {
                Program.Logger.LogDebug("The download queue is empty. Worker exiting.");
                break;
            }
            lock (_queLock)
            {
            _urlQueue.TryDequeue(out url);
            }
        
            if(string.IsNullOrWhiteSpace(url))
            {
                // If no URL was dequeued, continue to the next iteration.
                continue;
            }
            try
            {
                Program.Logger.LogDebug($"Processing Url {url}...");
                await ProcessUrlAsync(url);
            }
            catch (Exception ex)
            {
                // Catch any unexpected errors during processing a single URL.
                // swallow the exception and log it. so the worker can continue processing the next URL.
                OnDownloadFailed(new DownloadFailedEventArgs(url, ex));
            }
        }
    }







    /// <summary>
    ///     Loads the download queue from a persistent storage file asynchronously.
    /// </summary>
    /// <remarks>
    ///     This method reads URLs from a file specified in <see cref="DownloaderOptions.QueuePersistencePath" />.
    ///     Each valid URL is added to the internal queue. If the file does not exist or is empty, no URLs are loaded.
    /// </remarks>
    /// <exception cref="ArgumentException">
    ///     Thrown when the file path specified in <see cref="DownloaderOptions.QueuePersistencePath" /> is null, empty, or
    ///     whitespace.
    /// </exception>
    /// <exception cref="Exception">
    ///     Thrown when an error occurs while reading the file or processing its contents.
    /// </exception>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LoadQueueAsync()
    {
        string filePath = DownloaderOptions.Default.QueueFilename;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("A valid file path must be provided.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            Program.Logger.LogInformation($"Queue file '{filePath}' does not exist. Nothing to load.");
            return;
        }

        try
        {
            string[] lines = await File.ReadAllLinesAsync(filePath);
            int loadedCount = 0;

            foreach (string line in lines)
            {
                string? url = line?.Trim();

                if (!string.IsNullOrWhiteSpace(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    _urlQueue.Enqueue(url);
                    loadedCount++;
                }
            }

            Program.Logger.LogInformation($"Loaded {loadedCount} URLs from queue file '{filePath}'.");
        }
        catch (Exception ex)
        {
            Program.Logger.LogError(ex, $"Failed to load queue from {filePath}");
            throw;
        }
    }







    protected virtual void OnDownloadCompleted(DownloadCompletedEventArgs e)
    {
        Program.Logger.LogDebug($"Download complete: {e.FilePath}");
        _aggregator.Publish(new QueueCountMessage(QueueCount));
        DownloadCompleted?.Invoke(this, e);

    }







    protected virtual void OnDownloadFailed(DownloadFailedEventArgs e)
    {
        DownloadFailed?.Invoke(this, e);
    }







    protected virtual void OnQueueFinished()
    {
        Program.Logger.LogDebug("The download queue has been processed. Downloader exiting.");
        QueueFinished?.Invoke(this, EventArgs.Empty);
    }







    /// <summary>
    ///     Handles the download and verification for a single URL.
    /// </summary>
    /// <param name="url">The URL to process.</param>
    private async Task ProcessUrlAsync(string url)
    {
        string fileName = Path.GetFileName(new Uri(url).AbsolutePath);
        string destinationPath = Path.Combine(_downloadDirectory, fileName);

        // The context allows passing data to the Polly policy, like the URL for logging.
        var context = new Context { ["url"] = url };

        // Execute the download using the retry policy.
        var response = await _retryPolicy.ExecuteAsync(ctx => _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None), context);

        // Ensure we have a successful response after retries.
        _ = response.EnsureSuccessStatusCode();

        // Get the expected content length from headers.
        long? expectedLength = response.Content.Headers.ContentLength;

        if (!expectedLength.HasValue)
        {
            throw new InvalidOperationException($"Could not determine the file size for '{url}'. Content-Length header is missing.");
        }

        // Download the file content.
        using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
        {
            using var contentStream = await response.Content.ReadAsStreamAsync();
            await contentStream.CopyToAsync(fileStream);
        }

        // Verify that the downloaded file size matches the expected size.
        long actualLength = new FileInfo(destinationPath).Length;

        if (actualLength == expectedLength.Value)
        {
            OnDownloadCompleted(new DownloadCompletedEventArgs(url, destinationPath, actualLength));
        }
        else
        {
            // If sizes don't match, clean up the partial file and raise a failure event.
            File.Delete(destinationPath);
            throw new IOException($"Download verification failed for '{url}'. Expected {expectedLength.Value} bytes but received {actualLength} bytes.");
        }
    }







    /// <summary>
    ///     Fired when the queue has been fully processed and all download tasks have completed.
    /// </summary>
    public event EventHandler? QueueFinished;







    /// <summary>
    ///     Adds a single URL to the download queue.
    /// </summary>
    /// <param name="url">The URL to queue.</param>
    public void QueueUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            throw new ArgumentException("A valid URL must be provided.", nameof(url));
        }

        _urlQueue.Enqueue(url);
    }







    /// <summary>
    ///     Adds multiple URLs to the download queue.
    /// </summary>
    /// <param name="urls">
    ///     A collection of URLs to be added to the queue. Each URL must be a valid, absolute URI.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the <paramref name="urls" /> parameter is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when any URL in the collection is invalid or not well-formed.
    /// </exception>
    public void QueueUrls(IEnumerable<string> urls)
    {
        foreach (string url in urls)
        {
            QueueUrl(url);
        }
    }







    /// <summary>
    ///     Saves the current URL queue to a persistent storage file.
    /// </summary>
    /// <remarks>
    ///     The queue is saved to the file path specified in <see cref="DownloaderOptions.QueuePersistencePath" />.
    ///     If the directory does not exist, it will be created. The method logs the operation's success or failure.
    /// </remarks>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when the file path specified in <see cref="DownloaderOptions.QueuePersistencePath" /> is null, empty, or
    ///     whitespace.
    /// </exception>
    /// <exception cref="Exception">
    ///     Thrown when an error occurs during the save operation.
    /// </exception>
    public async Task SaveQueueAsync()
    {
        string filePath = DownloaderOptions.Default.QueueFilename;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("A valid file path must be provided.", nameof(filePath));
        }

        string? directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }

        try
        {
            string[] urls = _urlQueue.ToArray();
            await File.WriteAllLinesAsync(filePath, urls);
            Program.Logger.LogInformation($"Queue saved to {filePath} ({urls.Length} URLs).");
        }
        catch (Exception ex)
        {
            Program.Logger.LogError(ex, $"Failed to save queue to {filePath}");

        }
    }







    /// <summary>
    ///     Starts the asynchronous download process for all queued URLs.
    /// </summary>
    /// <remarks>
    ///     This method initializes and starts multiple worker tasks, each responsible for processing the download queue.
    ///     The number of concurrent workers is determined by the configured maximum concurrency level.
    ///     Once all queued downloads are completed, the <see cref="QueueFinished" /> event is triggered.
    /// </remarks>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="IOException">Thrown if there is an issue creating the download directory.</exception>
    /// <exception cref="AggregateException">Thrown if one or more worker tasks encounter unhandled exceptions.</exception>
    public async Task StartDownloadsAsync()
    {
        // Ensure the download directory exists.
        _ = Directory.CreateDirectory(_downloadDirectory);

        Program.Logger.LogInformation($"Starting download process with max concurrency of {_maxConcurrency}...");

        var workerTasks = new List<Task>();

        for (int i = 0; i < _maxConcurrency; i++)
        {

            // Each worker is a long-running task that will pull from the queue.
            workerTasks.Add(DownloadWorkerAsync());
        }

        // Wait for all worker tasks to complete. A worker completes when the queue is empty.
        await Task.WhenAll(workerTasks);

        // Signal that the entire queue has been processed.
        OnQueueFinished();
    }







    public async Task StopAllTasksAsync()
    {
        Program.Logger.LogInformation("Stopping all download tasks...");

        await SaveQueueAsync();

        // Clear the URL queue to prevent new downloads from starting.
        while (_urlQueue.TryDequeue(out _))
        {
        }

        // Notify that the queue has been cleared.
        OnQueueFinished();

        Program.Logger.LogInformation("Remaining tasks have been saved to file. All download tasks have been stopped.");
    }

}