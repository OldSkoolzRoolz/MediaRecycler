// Project Name: MediaRecycler
// File Name: WebAutomationService.cs
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using MediaRecycler.Logging;
using MediaRecycler.Modules.Interfaces;
using MediaRecycler.Modules.Loggers;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;
using PuppeteerSharp.Helpers;



namespace MediaRecycler.Modules;


/// <summary>
///     Implements the IWebAutomationService using PuppeteerSharp.
///     It encapsulates common puppeteersharp methods low-level browser interactions and includes
///     error handling and retry logic for individual actions.
/// </summary>
public class WebAutomationService : PuppeteerManager, IWebAutomationService, IAsyncDisposable
{


    private readonly int DefaultTimeout = 90000; // Default timeout for operations in milliseconds









    /// <summary>
    ///     Clicks on an HTML element specified by the given CSS selector and waits for the page navigation to complete.
    /// </summary>
    /// <param name="paginationSelector">
    ///     The CSS selector of the element to be clicked. This selector should uniquely identify the target element on the
    ///     page.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task completes when the click action is performed
    ///     and the page navigation triggered by the click is finished.
    /// </returns>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs while attempting to click the element or during the navigation process.
    /// </exception>
    public async Task ClickElementAsync(string paginationSelector, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paginationSelector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(paginationSelector));

        try
        {
            Log.LogInformation("Attempting to click element with selector: {Selector}", paginationSelector);

            if (Page != null)
            {
                var element = await Page.QuerySelectorAsync(paginationSelector).ConfigureAwait(false);

                if (element != null)
                {
                    await element.ClickAsync().ConfigureAwait(false);

                }
                else
                {

                    Log.LogWarning("Element with selector {Selector} not found.", paginationSelector);
                    return;
                }
            }

            Log.LogInformation("Successfully clicked element with selector: {Selector}", paginationSelector);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Error clicking element with selector: {Selector}", paginationSelector);
            throw;
        }
    }

























    public async Task<string> SafelyExtractTextAsync(string selector, int timeoutMs = 10000)
    {
        try
        {
            // Wait for element to be available
            await Page.WaitForSelectorAsync(selector, new WaitForSelectorOptions
            {
                        Timeout = timeoutMs,
                        Visible = true
            });

            // Double-check element exists before interaction
            var element = await Page.QuerySelectorAsync(selector);
            if (element == null)
            {
                throw new InvalidOperationException($"Element with selector '{selector}' not found");
            }

            return await element.EvaluateFunctionAsync<string>("el => el.textContent?.trim() || ''");
        }
        catch (WaitTaskTimeoutException ex)
        {
            Console.WriteLine($"Element '{selector}' not found within {timeoutMs}ms: {ex.Message}");
            return string.Empty; // Return safe default
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting text from '{selector}': {ex.Message}");
            return string.Empty;
        }
    }

















    public static async Task<T> RetryWithExponentialBackoffAsync<T>(
                Func<Task<T>> operation,
                int maxRetries = 3,
                TimeSpan? baseDelay = null,
                Func<Exception, bool>? shouldRetry = null)
    {
        var delay = baseDelay ?? TimeSpan.FromSeconds(1);
        shouldRetry ??= ex => ex is TimeoutException || ex is NavigationException;

        Exception lastException = null;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < maxRetries && shouldRetry(ex))
            {
                lastException = ex;
                var currentDelay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * Math.Pow(2, attempt));
                Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}. Retrying in {currentDelay.TotalSeconds}s...");
                await Task.Delay(currentDelay);
            }
        }

        throw new InvalidOperationException($"Operation failed after {maxRetries + 1} attempts", lastException);
    }







    /// <summary>
    /// Checks the health of the provided browser instance by verifying its responsiveness
    /// and ability to create new pages.
    /// </summary>
    /// <param name="browser">
    /// The browser instance to be checked. This should be an active instance of <see cref="IBrowser"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is <c>true</c> if the browser is healthy;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if an error occurs during the health check process.
    /// </exception>
    public async Task<bool> IsBrowserHealthyAsync(IBrowser browser)
    {
        try
        {
            var pages = await browser.PagesAsync();
            if (pages.Length == 0)
            {
                // Create a test page to verify browser is responsive
                await using var testPage = await browser.NewPageAsync();
                await testPage.GoToAsync("about:blank", new NavigationOptions { Timeout = 5000 });
                return true;
            }

            // Test if we can create a new page
            await using var newPage = await browser.NewPageAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Browser health check failed: {ex.Message}");
            return false;
        }
    }















































    /// <summary>
    /// Navigates to the specified URL with retry logic in case of navigation failures.
    /// </summary>
    /// <param name="page">
    /// The <see cref="IPage"/> instance representing the browser page where the navigation will occur.
    /// </param>
    /// <param name="url">
    /// The URL to navigate to. This should be a valid and reachable URL.
    /// </param>
    /// <param name="maxRetries">
    /// The maximum number of retry attempts in case of navigation failures. Defaults to 3.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is a boolean indicating whether the navigation
    /// was successful (<c>true</c>) or failed after exhausting all retries (<c>false</c>).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="page"/> or <paramref name="url"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown if an unexpected error occurs during navigation.
    /// </exception>
    public async Task<bool> NavigateWithRetryAsync( string url, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await Page.GoToAsync(url, new NavigationOptions
                {
                            Timeout = 30000, // 30 seconds
                            WaitUntil = [ WaitUntilNavigation.Networkidle0 ]
                });
                return true;
            }
            catch (TimeoutException ex)
            {
                Log.LogInformation($"Attempt {attempt}: Navigation timeout - {ex.Message}");
                if (attempt == maxRetries)
                {
                    Log.LogInformation("Max retries reached. Navigation failed.");
                    return false;
                }
                await Task.Delay(2000 * attempt); // Progressive delay
            }
        }
        return false;
    }
























    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or
    ///     resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public new async ValueTask DisposeAsync()
    {
 
        await base.DisposeAsync();
    }






    /// <summary>
    ///     Attempts to log in to the current page using credentials from environment variables.
    /// </summary>
    public async Task DoSiteLoginAsync()
    {
        _ = await Page.GoToAsync("https://www.bdsmlr.com/login", DefaultTimeout, [WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Networkidle0]);

        try
        {
            var element1Handle = await Page.QuerySelectorAsync("input#email").ConfigureAwait(false);

            if (element1Handle == null) return;

            string? email = Environment.GetEnvironmentVariable("SCRAPER_EMAIL");
            await element1Handle.TypeAsync(email).ConfigureAwait(false);

            var element2Handle = await Page.QuerySelectorAsync("input#password").ConfigureAwait(false);

            if (element2Handle == null) return;

            string? password = Environment.GetEnvironmentVariable("SCRAPER_PASSWORD");
            await element2Handle.TypeAsync(password).ConfigureAwait(false);

            await Page.ClickAsync("button[type=submit]").ConfigureAwait(false);

            //  await Page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = [ WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Networkidle0 ]});

           Log.LogInformation("Login to site was successful");
        }
        catch (Exception e)
        {

            Log.LogError(e, "Error during site login process");
        }

    }







    /// <inheritdoc />
    public async Task<string?> GetElementTextAsync(string selector)
    {
        try
        {
            _ = await Page.WaitForSelectorAsync(selector, new WaitForSelectorOptions { Timeout = 15000 }).ConfigureAwait(false);
            var element = await Page.QuerySelectorAsync(selector).WithTimeout(DefaultTimeout).ConfigureAwait(false);

            if (element == null)
            {
               Log.LogInformation($"Element with selector '{selector}' not found.");
                return null;
            }

            return await element.GetPropertyAsync("textContent").Result.JsonValueAsync<string>();
        }
        catch (WaitTaskTimeoutException)
        {
           Log.LogInformation($"Timeout waiting for selector '{selector}'.");
            return null;
        }
        catch (Exception ex)
        {
           Log.LogInformation($"Error getting text from selector '{selector}': {ex.Message}");
            return null;
        }
        finally
        {
            await DisposeAsync();
        }
    }






    /// <inheritdoc />
    public async Task<IElementHandle[]> GetNodeCollectionFromPageAsync(string selector)
    {
        try
        {
            var elements = await Page.QuerySelectorAllAsync(selector).WithTimeout(DefaultTimeout).ConfigureAwait(false);
            return elements;
        }
        catch (Exception ex)
        {
           Log.LogInformation($"Error getting node collection for selector '{selector}': {ex.Message}");
            return [];
        }
    }






    /// <summary>
    ///     Gets the contents of the current page the PuppeteerManager is attached to.
    ///     Method does not navigate to a new page, it simply retrieves the HTML content of the current page.
    /// </summary>
    /// <returns>String containing the HTML of the current page</returns>
    public async Task<string> GetPageContentsAsync()
    {
        return await Page.GetContentAsync().ConfigureAwait(false);
    }






    /// <inheritdoc />
    public async Task<string> GetPageContentsAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(url, "url cannot be null");

        try
        {

            _ = await Page.GoToAsync(url, new NavigationOptions
            {
                        WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded }, Timeout = 20000 // 20 seconds
            }).ConfigureAwait(false);
            return await Page.GetContentAsync().ConfigureAwait(false);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }






    /// <summary>
    ///     Asynchronously retrieves the title of the current page.
    /// </summary>
    /// <returns>A <see cref="string" /> representing the title of the current page.</returns>
    public async Task<string> GetPageTitleAsync()
    {
        try
        {
            return await Page.GetTitleAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
           Log.LogInformation($"Error getting page title: {ex.Message}");
            throw;
        }
    }






    /// <summary>
    ///     Asynchronously navigates to the specified URL using the Puppeteer page instance.
    /// </summary>
    /// <param name="url">The URL to navigate to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     Publishes status messages for navigation start, success, and failure.
    ///     Logs information messages for navigation start and result.
    ///     Handles exceptions and publishes error messages.
    /// </remarks>
    public async Task GoToAsync(string url)
    {
        if (Page == null)
        {
            Log.LogCritical("Unable to use browser Page object. Object is null.");
            throw new InvalidOperationException("Browser Page object is null.");
        }

        try
        {
           Log.LogInformation($"Navigating to {url}...");

            var response = await Page.GoToAsync(url, DefaultTimeout, [WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Networkidle0]).ConfigureAwait(false);

            if (!response.Ok){Log.LogWarning($"Navigation failed with status: {response.Status}");}
            else
            {
                Log.LogInformation("Navigation succesful.");
            }

        }
        catch (Exception ex)
        {
           Log.LogInformation($"Error navigating to {url}: {ex.Message}");

        }


    }






    /// <summary>
    ///     Determines whether the specified element, identified by its CSS selector, is visible on the page.
    /// </summary>
    /// <param name="selector">
    ///     The CSS selector of the element to check for visibility.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value:
    ///     <c>true</c> if the element is visible (i.e., it exists in the DOM and has a non-zero width and height);
    ///     otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     If an exception occurs during the visibility check, it is logged using the event aggregator,
    ///     and the method returns <c>false</c>.
    /// </remarks>
    public async Task<bool> IsElementVisibleAsync(string selector)
    {
        try
        {
            var element = await Page.QuerySelectorAsync(selector).WithTimeout(DefaultTimeout).ConfigureAwait(false);

            if (element == null) return false;

            var boundingBox = await element.BoundingBoxAsync();
            return boundingBox != null && boundingBox.Width > 0 && boundingBox.Height > 0;
        }
        catch (Exception ex)
        {
           Log.LogInformation($"Error checking visibility of element '{selector}': {ex.Message}");
            return false;
        }
    }






    /// <inheritdoc />
    public Task NavigateToNextPageAsync()
    {
        throw new NotImplementedException();
    }






    /// <summary>
    ///     Retrieves the first element that matches the specified CSS selector from the current page.
    /// </summary>
    /// <param name="selector">
    ///     A CSS selector string used to identify the desired element on the page.
    /// </param>
    /// <returns>
    ///     An <see cref="IElementHandle" /> representing the first matching element, or <c>null</c> if no matching element is
    ///     found.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when the <paramref name="selector" /> is <c>null</c>, empty, or consists only of whitespace.
    /// </exception>
    /// <remarks>
    ///     This method uses PuppeteerSharp's <c>QuerySelectorAsync</c> to locate the element.
    ///     If an error occurs during the operation, it logs the error and returns <c>null</c>.
    /// </remarks>
    public async Task<IElementHandle?> QuerySelectorAsync(string selector)
    {
        if (string.IsNullOrWhiteSpace(selector))
        {
            Log.LogError("Selector cannot be null or empty. {selector}", nameof(selector));
            return null;
        }

        try
        {
            return await Page.QuerySelectorAsync(selector).WithTimeout(DefaultTimeout).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
           Log.LogInformation($"Error in QuerySelectorAsync: {ex.Message}");
            return null;
        }
    }






    /// <summary>
    ///     Automatically waits for a selector to appear on the page.
    /// </summary>
    /// <param name="waitForSelector"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task WaitForSelectorAsync(string waitForSelector) =>
                Page.WaitForSelectorAsync(waitForSelector, new WaitForSelectorOptions { Timeout = 60000 });

}