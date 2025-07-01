// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers



// To run this code, you need to add the Polly NuGet package to your project:
// dotnet add package Polly




using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;

using MediaRecycler.Logging;
using MediaRecycler.Model;
using MediaRecycler.Modules.Interfaces;
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
    /// <param name="aggregator">The maximum number of concurrent downloads. Must be greater than 0.</param>
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
                        Log.LogWarning(exception, "Retry {RetryAttempt}/{MaxRetries} for URL {Url}. Delaying for {Delay:ss\\.fff}s due to error: {ErrorMessage}",
                                     retryAttempt, DownloaderOptions.Default.MaxRetries, url?.ToString() ?? "N/A", timespan, exception.Message);
                        return Task.CompletedTask;
                    });






        var _retryPolicy2 = Policy.Handle<HttpRequestException>(ShouldRetryHttpRequest).Or<IOException>().Or<TaskCanceledException>(ex => !ex.CancellationToken.IsCancellationRequested)
              .WaitAndRetryAsync(DownloaderOptions.Default.MaxRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)),
                          (exception, timespan, retryAttempt, context) =>
                          {
                              var url = context.ContainsKey("Url") ? context["Url"] as Uri : null;
                              Log.LogWarning(exception, "Retry {RetryAttempt}/{MaxRetries} for URL {Url}. Delaying for {Delay:ss\\.fff}s due to error: {ErrorMessage}",
                                           retryAttempt, DownloaderOptions.Default.MaxRetries, url?.ToString() ?? "N/A", timespan, exception.Message);
                              return Task.CompletedTask;
                          });





    }







    /// <inheritdoc/>
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




    private readonly object _queLock = new();
    private readonly ILogger _logger;







    /// <summary>
    ///     The core worker method. Each worker runs this method, continuously dequeuing
    ///     and processing URLs until the queue is empty.
    /// </summary>
    private async Task DownloadWorkerAsync()
    {
        string? postid = null;
        while (true)
        {
            string? url = null;
            // Check if the queue is empty before processing.
            // If it is, we exit the worker loop.
            if (_urlQueue.IsEmpty)
            {
                Log.LogDebug("The download queue is empty. Worker exiting.");
                break;
            }
            lock (_queLock)
            {
                _ = _urlQueue.TryDequeue(out url);
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                // If no URL was dequeued, continue to the next iteration.
                continue;
            }
            try
            {
                postid = Path.GetFileNameWithoutExtension(url).Split("-")[1];
                Log.LogDebug($"Processing Url {url}...");
                await ProcessUrlAsync(url);
            }
            catch (Exception ex)
            {
                // Catch any unexpected errors during processing a single URL.
                // swallow the exception and log it. so the worker can continue processing the next URL.
                OnDownloadFailed(new DownloadFailedEventArgs(url, postid!, ex));
            }
        }
    }












    /// <inheritdoc/>
    protected virtual async Task OnDownloadCompleted(DownloadCompletedEventArgs e)
    {
        Log.LogDebug($"Download complete: {e?.FilePath}");
        _aggregator.Publish(new QueueCountMessage(QueueCount));



        await DataLayer.UpdateTargetDownloaded(e.PostId);

        //publish event to subscribers
        DownloadCompleted?.Invoke(this, e);

    }







    /// <inheritdoc/>
    protected virtual void OnDownloadFailed(DownloadFailedEventArgs e)
    {
        Log.LogError(e.Exception, $"Download failed: {e.Url}");
        DownloadFailed?.Invoke(this, e);
    }







    /// <inheritdoc/>
    protected virtual void OnQueueFinished()
    {
        Log.LogDebug("The download queue has been processed. Downloader exiting.");
        QueueFinished?.Invoke(this, EventArgs.Empty);
    }







    /// <summary>
    ///     Handles the download and verification for a single URL.
    /// </summary>
    /// <param name="url">The URL to process.</param>
    private async Task ProcessUrlAsync(string url)
    {
        var uri = new Uri(url);
        string fileName = Path.GetFileName(uri.AbsolutePath);
        string postid = Path.GetFileNameWithoutExtension(fileName).Split("-")[1];
        string destinationPath = Path.Combine(DownloaderOptions.Default.DownloadPath, $"{postid}.mp4");
        const int buffer = 81920;

        try
        {
            if (File.Exists(destinationPath))
            {
                Log.LogInformation("Skipping duplicate file: {Url}.", url);
                await DataLayer.UpdateTargetDownloaded(postid);
                return;
            }

            var context = new Context { ["url"] = url };

            // var response = await _retryPolicy.ExecuteAsync(ctx => _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None), context);
            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);
            response.EnsureSuccessStatusCode();

            long? expectedLength = response.Content.Headers.ContentLength;
            if (!expectedLength.HasValue)
            {
                throw new InvalidOperationException($"Missing Content-Length header for '{url}'.");
            }

            await using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, buffer, true))
            await using (var contentStream = await response.Content.ReadAsStreamAsync())
            {
                await contentStream.CopyToAsync(fileStream, buffer);

            }

            // Verify file length AFTER the stream has been fully copied and closed.
            // This ensures the file is not locked when checking its size.
            long actualLength = new FileInfo(destinationPath).Length;

            if (actualLength == expectedLength.Value)
            {
                OnDownloadCompleted(new DownloadCompletedEventArgs(url, postid, destinationPath, actualLength));
            }
            else
            {
                // If verification fails, delete the corrupted file.
                // This also helps clean up partially downloaded files that might cause issues later.
                try
                {
                    File.Delete(destinationPath);
                    Log.LogWarning("Deleted corrupted file {DestinationPath} due to size mismatch.", destinationPath);
                }
                catch (IOException ex)
                {
                    Log.LogError(ex, "Failed to delete corrupted file {DestinationPath}. Manual intervention may be required.", destinationPath);
                }
                throw new IOException($"Download verification failed for '{url}'. Expected {expectedLength.Value} bytes but received {actualLength} bytes.");
            }

        }
        catch (Exception)
        {

            File.Delete(destinationPath);

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

        Log.LogInformation($"Starting download process with max concurrency of {_maxConcurrency}...");

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







    /// <inheritdoc/>
    public async Task StopAllTasksAsync()
    {
        Log.LogInformation("Stopping all download tasks...");


        // Clear the URL queue to prevent new downloads from starting.
        while (_urlQueue.TryDequeue(out _))
        {
        }

        // Notify that the queue has been cleared.
        OnQueueFinished();

        Log.LogInformation("Remaining tasks have been saved to file. All download tasks have been stopped.");
    }

}