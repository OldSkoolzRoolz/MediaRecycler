// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




namespace MediaRecycler.Modules;


/// <summary>
///     Configuration options for the MiniFrontier.
/// </summary>
public class MiniFrontierSettings
{

    /// <summary>
    ///     The default time to wait between requests to the same host
    ///     if no specific delay (e.g., from robots.txt) is applied.
    ///     Defaults to 1 second.
    /// </summary>
    public TimeSpan DefaultPolitenessDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    ///     Optional: Maximum number of URLs to keep in a single host's queue.
    ///     If null, the queue size is effectively unlimited (bound by memory).
    ///     Defaults to null (unlimited).
    /// </summary>
    public int? MaxQueueDepthPerHost { get; init; }

    /// <summary>
    ///     Optional: Maximum number of URLs to track in the 'seen' set.
    ///     Helps prevent unbounded memory growth for very long crawls in the standalone version.
    ///     If null, the seen set size is effectively unlimited (bound by memory).
    ///     For distributed systems, this responsibility often moves to an external store.
    ///     Defaults to null (unlimited).
    /// </summary>
    /// <remarks>
    ///     Note: Implementing eviction for the ConcurrentDictionary based seen set
    ///     would require additional logic beyond just this setting. This setting
    ///     provides the configuration point for such logic if implemented.
    /// </remarks>
    public long? MaxSeenUrls { get; init; }

    // Future settings could include:
    // - Supported Schemes (List<string>)
    // - UserAgent string
    // - Max total URLs
    // - Specific delays per host (Dictionary<string, TimeSpan>)

}
