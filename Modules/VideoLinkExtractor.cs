// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers



// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using KC.Crawler;

using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;



// Added for IElementHandle, IJSHandle


namespace MediaRecycler.Modules;


public static class VideoLinkExtractor
{

    private static readonly ILogger ScraperLogger = Program.Logger;







    /// <summary>
    ///     Main entry point to process blog URLs from the frontier.
    ///     Dequeues URLs and orchestrates the parsing of each blog's archive.
    /// </summary>
    internal static async Task StartBulkParsingAsync(
                MiniFrontier frontier,
                IPage page,
                Scraping settings,
                DownloaderModule downloader)
    {
        // --- Parameter Validation ---
        ArgumentNullException.ThrowIfNull(frontier);
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(downloader);

        ScraperLogger.LogInformation("Beginning to process url's in the frontier...");


        // variable used to limit processing for debugging.
        var temp = 0;


        // --- Main Loop: Process URLs from Frontier ---
        while (frontier.Count() > 0)
        {
            if (frontier.TryDequeue(out var blogUrl) && blogUrl != null)
            {
                await ProcessSingleBlogAsync(blogUrl, ScraperLogger, page, settings, downloader);
            }
            else if (frontier.Count() > 0) // Check if TryDequeue failed but frontier still has items
            {
                // This case might indicate an issue with the frontier implementation
                ScraperLogger.LogWarning(
                            "TryDequeue returned false but frontier count is {Count}. There might be an issue with the frontier or null URLs.",
                            frontier.Count());
            }

            // If TryDequeue returns false and Count is 0, the loop condition handles termination.

            // Debugging hard limit
            if (temp == 2) { break; }

            temp++;
        }

        ScraperLogger.LogInformation("Blog archive parsing finished. Frontier is empty.");
    }







    /// <summary>
    ///     Processes a single blog URL by navigating to its archive page, extracting video links, and enqueuing them for
    ///     download.
    /// </summary>
    /// <param name="blogUrl"></param>
    /// <param name="scraperLogger"></param>
    /// <param name="page"></param>
    /// <param name="settings"></param>
    /// <param name="downloader"></param>
    /// <returns></returns>
    public static async Task ProcessSingleBlogAsync(
                Uri blogUrl,
                ILogger scraperLogger,
                IPage page,
                Scraping settings,
                DownloaderModule downloader)
    {
        var sBlogArchiveUrl = string.Empty;

        try
        {
            // Construct the specific archive URL
            sBlogArchiveUrl = blogUrl.ToString().TrimEnd('/');
            scraperLogger.LogInformation("Processing Blog URL: {BlogUrl}. Target Archive: {ArchiveUrl}", blogUrl,
                        sBlogArchiveUrl);

            // --- Navigate to the Blog Archive Page ---
            if (!await NavigateToPageAsync(page, sBlogArchiveUrl, scraperLogger,
                            $"blog archive page: {sBlogArchiveUrl}"))
            {
                return; // Skip this blog if initial navigation fails
            }

            // --- Parse Archive Pages and Collect Video Page Links ---
            var videoPageLinks =
                        await ParseBlogArchivePagesAsync(blogUrl, sBlogArchiveUrl, scraperLogger, page, settings);

            // --- Process Extracted Video Page Links ---
            await ProcessVideoPageLinksAsync(videoPageLinks, blogUrl, page, settings, downloader);

        }
        catch (Exception ex) // Catch unexpected errors during the processing of a single blog URL
        {
            scraperLogger.LogError(ex,
                        "An unhandled error occurred while processing blog URL {BlogUrl} (Archive: {ArchiveUrl}). Skipping to next URL.",
                        blogUrl, sBlogArchiveUrl);
        }
    }







    /// <summary>
    ///     Navigates the Puppeteer page to a specified URL with error handling and logging.
    /// </summary>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    private static async Task<bool> NavigateToPageAsync(
                IPage page,
                string url,
                ILogger logger,
                string pageDescription)
    {
        try
        {
            //page.Request -= Page_Request_BlockImages;



            // ---> BEST LOCATION START <---
            // Ensure interception is enabled (idempotent call after the first time)
            // await page.SetRequestInterceptionAsync(true);

            // Check if the handler is already attached to avoid duplicates if this method
            // could somehow be called multiple times without page recreation.
            // (A more robust way might involve storing handler state, but this is simpler)
            // NOTE: PuppeteerSharp might handle duplicate event handler additions gracefully,
            // but explicit checks can prevent potential issues or performance overhead.
            // For simplicity in this common pattern, often the handler is just added.
            // If performance is critical or complex handler management is needed,
            // track the handler state more explicitly.

            // Remove existing handlers first (safer if method might be re-entered for same page)
            // This assumes only ONE type of request handler is managed by this function.
            //  page.Request -= Page_Request_BlockImages; // Use a named handler
            // Add the handler
            //page.Request += Page_Request_BlockImages; // Use a named handler
            // ---> BEST LOCATION END <---






            logger.LogDebug("Navigating to {PageDescription}: {Url}", pageDescription, url);
            await page.GoToAsync(url,
                        new NavigationOptions
                                    { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 }, Timeout = 60000 });
            logger.LogDebug("Successfully navigated to {PageDescription}: {Url}", pageDescription, url);
            return true;
        }
        catch (TargetClosedException tce)
        {
            logger.LogDebug(399, tce, "PuppeteerSharpFailure");

            //swallow
            return false;
        }
        catch (Exception navEx)
        {

            logger.LogError(navEx, "Failed to navigate to {PageDescription}: {Url}. Skipping.", pageDescription, url);
            return false;
        }
    }







    // Define the handler separately for clarity and reusability
    private static void Page_Request_BlockImages(
                object? sender,
                RequestEventArgs e)
    {
        if (e.Request.ResourceType == ResourceType.Image)
        {
            e.Request.AbortAsync().Wait();
        }
        else
        {
            e.Request.ContinueAsync().Wait();
        }





    }







    /// <summary>
    ///     Parses through the paginated archive of a blog, extracting links to individual video pages.
    /// </summary>
    /// <returns>A HashSet containing all unique video page links found.</returns>
    internal static async Task<HashSet<string>> ParseBlogArchivePagesAsync(
                Uri blogUrl,
                string currentArchiveUrl,
                ILogger scraperLogger,
                IPage page,
                Scraping settings)
    {
        HashSet<string> allVideoPageLinks = new();
        var pageNum = 1;
        var nextPageAvailable = true;
        var currentPageUrl = currentArchiveUrl; // Track the current URL for logging

        ArgumentNullException.ThrowIfNull(blogUrl);
        ArgumentNullException.ThrowIfNull(scraperLogger);
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(settings);


        scraperLogger.LogDebug("Starting archive pagination for {BlogUrl}", blogUrl);

        while (nextPageAvailable)
        {
            scraperLogger.LogDebug("--- Processing Archive Page {PageNum} for {BlogUrl} (URL: {CurrentPageUrl}) ---",
                        pageNum, blogUrl, currentPageUrl);

            // Extract links from the current page
            await ExtractVideoPageLinksFromCurrentPageAsync(page, settings, blogUrl, pageNum,
                        allVideoPageLinks);

            // Handle pagination to the next page
            var paginationResult = await HandleArchivePaginationAsync(page, settings, blogUrl, pageNum);

            nextPageAvailable = paginationResult.Success;
            currentPageUrl = page.Url; // Update current URL after potential navigation

            if (nextPageAvailable)
            {
                pageNum++;
            }

            // Temporary page limit HACK - Consider removing or making configurable
            if (pageNum >= 10)
            {
                scraperLogger.LogWarning("Reached page limit (10) for blog {BlogUrl}. Stopping pagination.", blogUrl);
                nextPageAvailable = false;
            }
        }

        scraperLogger.LogInformation(
                    "Finished archive pagination for {BlogUrl}. Found {LinkCount} unique video page links.", blogUrl,
                    allVideoPageLinks.Count);
        return allVideoPageLinks;
    }







    /// <summary>
    ///     Extracts video page links from the *currently loaded* archive page and adds them to the provided set.
    /// </summary>
    private static async Task ExtractVideoPageLinksFromCurrentPageAsync(
                IPage page,
                Scraping settings,
                Uri blogUrl,
                int pageNum,
                HashSet<string> videoPageLinks)
    {
        IElementHandle[]? nodes = null;

        try
        {
            nodes = await page.QuerySelectorAllAsync(settings.TargetElementSelector);

            if (nodes == null || nodes.Length == 0)
            {
                ScraperLogger.LogWarning(
                            "No archive link nodes found using selector '{Selector}' on page {PageNum} for blog {BlogUrl}",
                            settings.TargetElementSelector, pageNum, blogUrl);
                return;
            }

            ScraperLogger.LogDebug("Found {NodeCount} potential video page links on page {PageNum}.", nodes.Length,
                        pageNum);

            foreach (var nodeHandle in nodes)
            {
                IJSHandle? hrefHandle = null;

                try
                {
                    hrefHandle = await nodeHandle.GetPropertyAsync(settings.TargetPropertySelector);
                    var link = await hrefHandle?.JsonValueAsync<string>()!;

                    if (!string.IsNullOrWhiteSpace(link))
                    {
                        // Optional: Resolve relative URLs if necessary: new Uri(blogUrl, link).ToString()
                        if (videoPageLinks.Add(link)) // Add returns true if the item was new
                        {
                            ScraperLogger.LogTrace("Extracted unique video page link: {Link}", link);
                        }
                        else
                        {
                            ScraperLogger.LogTrace("Duplicate video page link found (already extracted): {Link}", link);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ScraperLogger.LogWarning(ex,
                                "Error extracting href from an element on archive page {PageNum} of {BlogUrl}. Continuing.",
                                pageNum, blogUrl);
                }
                finally
                {
                    if (hrefHandle != null)
                    {
                        await hrefHandle.DisposeAsync();
                    }

                    // nodeHandle disposal is usually handled by Puppeteer when nodes array is processed/disposed.
                    // Explicit disposal here can be added if needed for very long-running loops or complex scenarios.
                    // await nodeHandle.DisposeAsync();
                }
            }
        }
        catch (Exception ex)
        {
            ScraperLogger.LogError(ex, "Error querying/processing archive links on page {PageNum} for {BlogUrl}",
                        pageNum, blogUrl);

            // Decide whether to throw, stop pagination, or just log and continue
        }
        finally
        {
            // Dispose the array of handles
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    await node.DisposeAsync();
                }
            }
        }
    }







    /// <summary>
    ///     Finds the 'next' pagination link, clicks it, and waits for navigation.
    /// </summary>
    /// <returns>A record indicating success (true if navigation occurred, false otherwise).</returns>
    private static async Task<(bool Success, string? NextPageUrl)> HandleArchivePaginationAsync(
                IPage page,
                Scraping settings,
                Uri blogUrl,
                int currentPageNum)
    {
        IElementHandle? nextAnchorHandle = null;
        string? nextPageUrl = null;

        try
        {
            // TODO: Selector needs refinement if it's selecting non-next links.
            // Maybe look for specific text or 'rel=next' attribute if available.
            // Example: "ul.pagination li:not(.disabled) a[rel='next']" or similar
            nextAnchorHandle =
                        await page.QuerySelectorAsync(settings
                                    .PaginationSelector); // Assuming this selects the *correct* next link

            if (nextAnchorHandle != null)
            {

                ScraperLogger.LogDebug(
                            "Found next page link (Page {CurrentPageNum} -> ?). Clicking and waiting for navigation. Target URL (approx): {NextPageUrl}",
                            currentPageNum, nextPageUrl ?? "N/A");

                // Use Task.WhenAll for potentially faster execution if network latency is high
                await Task.WhenAll(
                            nextAnchorHandle.ClickAsync(),
                            page.WaitForNavigationAsync(new NavigationOptions
                            {
                                        WaitUntil = new[] { WaitUntilNavigation.Networkidle2 },
                                        Timeout = settings.DefaultTimeout
                            })
                            );

                ScraperLogger.LogDebug("Navigation to page {NextPageNum} complete. New URL: {ActualNewUrl}",
                            currentPageNum + 1, page.Url);
                return (Success: true, NextPageUrl: page.Url);
            }
            else
            {
                ScraperLogger.LogInformation(
                            "No next page link found using selector '{Selector}'. Pagination finished for {BlogUrl} after page {CurrentPageNum}.",
                            settings.PaginationSelector, blogUrl, currentPageNum);
                return (Success: false, NextPageUrl: null);
            }
        }
        catch (Exception ex)
        {
            ScraperLogger.LogError(ex,
                        "Error occurred during pagination handling (finding/clicking next link) for {BlogUrl} after page {CurrentPageNum}. Stopping pagination.",
                        blogUrl, currentPageNum);
            return (Success: false, NextPageUrl: null); // Stop pagination on error
        }
        finally
        {
            if (nextAnchorHandle != null)
            {
                await nextAnchorHandle.DisposeAsync();
            }
        }
    }







    /// <summary>
    ///     Iterates through collected video page links, navigates to each, extracts the video source URL, and enqueues it.
    /// </summary>
    private static async Task ProcessVideoPageLinksAsync(
                HashSet<string> videoPageLinks,
                Uri blogUrl,
                IPage page,
                Scraping settings,
                DownloaderModule downloader)
    {
        if (!videoPageLinks.Any())
        {
            ScraperLogger.LogInformation("No video page links were extracted for blog {BlogUrl}. Nothing to process.",
                        blogUrl);
            return;
        }

        ScraperLogger.LogInformation("Found {Count} video page links for blog {BlogUrl}. Processing each...",
                    videoPageLinks.Count, blogUrl);
        var processedCount = 0;
        var failedCount = 0;

        foreach (var link in videoPageLinks)
        {
            try
            {




                // Navigate to the individual video page
                if (!await NavigateToPageAsync(page, link, ScraperLogger, $"video page: {link}"))
                {
                    failedCount++;
                    continue; // Skip if navigation fails
                }

                // Extract the direct video source URL
                var videoSrcUrl = await ExtractVideoSourceUrlFromPageAsync(page, settings, link);

                // Enqueue the video source URL if found
                if (!string.IsNullOrWhiteSpace(videoSrcUrl))
                {
                    ScraperLogger.LogInformation(
                                "Successfully extracted video source URL: {VideoSrcUrl} from page {VideoPageLink}",
                                videoSrcUrl,
                                link);

                    if (downloader.EnqueueUrl(new Uri(videoSrcUrl))) // Assuming videoSrcUrl is absolute
                    {
                        ScraperLogger.LogTrace("Video URL enqueued successfully: {VideoSrcUrl}", videoSrcUrl);
                        processedCount++;
                    }
                    else
                    {
                        ScraperLogger.LogWarning(
                                    "Failed to enqueue video URL (already present or downloader stopping?): {VideoSrcUrl}",
                                    videoSrcUrl);

                        // Consider if this should count as a failure or just a skip
                    }
                }
                else
                {
                    failedCount++;

                    // Warning already logged by ExtractVideoSourceUrlFromPageAsync
                }
            }
            catch (Exception ex)
            {
                failedCount++;
                ScraperLogger.LogError(ex, "Error processing video page link: {VideoPageLink}", link);

                // Continue to the next video link even if one fails
            }
        } // End foreach

        ScraperLogger.LogInformation(
                    "Finished processing video links for blog {BlogUrl}. Successfully processed: {ProcessedCount}, Failed/Skipped: {FailedCount}",
                    blogUrl, processedCount, failedCount);
    }







    /// <summary>
    ///     Extracts the direct video source URL (e.g., from a video tag's src) from the *currently loaded* page.
    /// </summary>
    /// <returns>The video source URL string, or null if not found or an error occurs.</returns>
    private static async Task<string?> ExtractVideoSourceUrlFromPageAsync(
                IPage page,
                Scraping settings,
                string videoPageLink)
    {
        IElementHandle? videoElementHandle = null;
        IJSHandle? srcHandle = null;

        try
        {
            videoElementHandle = await page.QuerySelectorAsync(settings.TargetElementSelector);

            if (videoElementHandle == null)
            {
                ScraperLogger.LogWarning(
                            "Unable to find video element using selector '{Selector}' on page: {VideoPageLink}",
                            settings.TargetElementSelector, videoPageLink);
                return null;
            }

            srcHandle = await videoElementHandle.GetPropertyAsync(settings.TargetPropertySelector);

            if (srcHandle == null)
            {
                ScraperLogger.LogWarning(
                            "Could not get 'src' property from the video element found by selector '{Selector}' on page: {VideoPageLink}",
                            settings.TargetPropertySelector, videoPageLink);
                return null;
            }

            var videoSrcUrl = await srcHandle.JsonValueAsync<string>();

            if (string.IsNullOrWhiteSpace(videoSrcUrl))
            {
                ScraperLogger.LogWarning(
                            "Found video element but 'src' attribute is empty or null on page: {VideoPageLink}",
                            videoPageLink);
                return null;
            }

            // Optional: Add validation if needed (e.g., check if it's an expected domain or format)

            return videoSrcUrl;
        }
        catch (Exception ex)
        {
            ScraperLogger.LogError(ex, "Error extracting video source URL from page: {VideoPageLink}", videoPageLink);
            return null;
        }
        finally
        {
            // Safely dispose handles
            if (srcHandle != null)
            {
                await srcHandle.DisposeAsync();
            }

            if (videoElementHandle != null)
            {
                await videoElementHandle.DisposeAsync();
            }
        }
    }







    public static async Task StartSingleArchiveAsync(
                string startUrl,
                IPage page,
                Scraping scraperSettings,
                DownloaderModule downloader)
    {


        //await ExtractVideoPageLinksFromCurrentPageAsync(scraperLogger,page,scraperSettings,scraperSettings.StartingWebPage)

        await Task.CompletedTask;

    }

}
