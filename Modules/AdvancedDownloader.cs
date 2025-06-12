// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"



// To run this code, you need to add the Polly NuGet package to your project:
// dotnet add package Polly




using System.Collections.Concurrent;
using System.Net.Http.Headers;

using Polly;
using Polly.Retry;



namespace MediaRecycler.Modules;


/// <summary>
///     Provides data for the DownloadCompleted event.
/// </summary>
public class DownloadCompletedEventArgs : EventArgs
{



    public DownloadCompletedEventArgs(string url, string filePath, long fileSizeBytes)
    {
        Url = url;
        FilePath = filePath;
        FileSizeBytes = fileSizeBytes;
    }






    /// <summary>
    ///     The URL of the file that was successfully downloaded.
    /// </summary>
    public string Url { get; }

    /// <summary>
    ///     The path to the downloaded file on the local disk.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    ///     The size of the downloaded file in bytes.
    /// </summary>
    public long FileSizeBytes { get; }

}


/// <summary>
///     Provides data for the DownloadFailed event.
/// </summary>
public class DownloadFailedEventArgs : EventArgs
{



    public DownloadFailedEventArgs(string url, Exception exception)
    {
        Url = url;
        Exception = exception;
    }






    /// <summary>
    ///     The URL of the file that failed to download.
    /// </summary>
    public string Url { get; }

    /// <summary>
    ///     The exception that caused the download to fail.
    /// </summary>
    public Exception Exception { get; }

}


/// <summary>
///     Manages a queue of URLs to download asynchronously with a specified level of concurrency.
///     Uses Polly for transient error handling and retries. Verifies download integrity by file size.
/// </summary>
public class UrlDownloader : IDisposable
{

    #region Constructor

    /// <summary>
    ///     Initializes a new instance of the UrlDownloader.
    /// </summary>
    /// <param name="maxConcurrency">The maximum number of concurrent downloads. Must be greater than 0.</param>
    /// <param name="downloadDirectory">The directory where files will be saved. It will be created if it doesn't exist.</param>
    public UrlDownloader(int maxConcurrency, string downloadDirectory)
    {
        if (maxConcurrency <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "Concurrency must be greater than zero.");
        }

        _maxConcurrency = maxConcurrency;
        _downloadDirectory = downloadDirectory ?? throw new ArgumentNullException(nameof(downloadDirectory));

        // It's a best practice to reuse HttpClient instances.
        _httpClient = new HttpClient();
        ConfigureHttpClient(_httpClient);

        // Define a retry policy using Polly.
        // This policy will retry up to 3 times on HttpRequestException (network errors) 
        // or on HTTP 5xx server errors. It will use an exponential backoff strategy.
        _retryPolicy = Policy.Handle<HttpRequestException>().OrResult<HttpResponseMessage>(response => (int)response.StatusCode >= 500).WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine($"[RETRY] Attempt {retryCount} failed for URL {context["url"]}. Delaying for {timespan.TotalSeconds}s. Reason: {outcome.Exception?.Message ?? outcome.Result.ReasonPhrase}");
        });
    }

    #endregion






    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        _httpClient.Dispose();
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






    #region Fields and Properties

    private readonly HttpClient _httpClient;
    private readonly ConcurrentQueue<string> _urlQueue = new();
    private readonly int _maxConcurrency;
    private readonly string _downloadDirectory;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    #endregion

    #region Events

    /// <summary>
    ///     Fired when a file has been successfully downloaded and verified.
    /// </summary>
    public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;

    /// <summary>
    ///     Fired when a file fails to download after all retry attempts.
    /// </summary>
    public event EventHandler<DownloadFailedEventArgs> DownloadFailed;

    /// <summary>
    ///     Fired when the queue has been fully processed and all download tasks have completed.
    /// </summary>
    public event EventHandler QueueFinished;

    #endregion

    #region Public Methods

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
    ///     Adds a collection of URLs to the download queue.
    /// </summary>
    /// <param name="urls">The collection of URLs to queue.</param>
    public void QueueUrls(IEnumerable<string> urls)
    {
        foreach (var url in urls)
        {
            QueueUrl(url);
        }
    }






    /// <summary>
    ///     Starts the download process. This method returns a Task that completes when all URLs
    ///     in the queue have been processed.
    /// </summary>
    /// <returns>A Task that represents the entire download operation.</returns>
    public async Task StartDownloadsAsync()
    {
        // Ensure the download directory exists.
        _ = Directory.CreateDirectory(_downloadDirectory);

        Console.WriteLine($"Starting download process with max concurrency of {_maxConcurrency}...");

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

    #endregion

    #region Private Worker and Processing Logic

    /// <summary>
    ///     The core worker method. Each worker runs this method, continuously dequeuing
    ///     and processing URLs until the queue is empty.
    /// </summary>
    private async Task DownloadWorkerAsync()
    {
        while (_urlQueue.TryDequeue(out var url))
        {
            try
            {
                await ProcessUrlAsync(url);
            }
            catch (Exception ex)
            {
                // Catch any unexpected errors during processing a single URL.
                OnDownloadFailed(new DownloadFailedEventArgs(url, ex));
            }
        }
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
        HttpResponseMessage response = await _retryPolicy.ExecuteAsync(ctx => _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None), context);

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

    #endregion

    #region Event Raisers

    protected virtual void OnDownloadCompleted(DownloadCompletedEventArgs e)
    {
        DownloadCompleted?.Invoke(this, e);
    }






    protected virtual void OnDownloadFailed(DownloadFailedEventArgs e)
    {
        DownloadFailed?.Invoke(this, e);
    }






    protected virtual void OnQueueFinished()
    {
        QueueFinished?.Invoke(this, EventArgs.Empty);
    }

    #endregion

}
