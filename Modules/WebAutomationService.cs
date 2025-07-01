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

    private readonly IEventAggregator? _aggregator;
    private readonly ILogger _logger;

    private readonly int DefaultTimeout = 90000; // Default timeout for operations in milliseconds






    /// <summary>
    ///     Initializes a new instance of the <see cref="WebAutomationService" /> class.
    /// </summary>
    /// <param name="logger">
    ///     The logger instance used for logging messages and errors.
    /// </param>
    /// <param name="aggregator">
    ///     The event aggregator used for publishing and subscribing to events.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="logger" /> or <paramref name="aggregator" /> is <c>null</c>.
    /// </exception>
    public WebAutomationService()
    {


        //   _puppeteerManager = new PuppeteerManager(aggregator);

    }






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






    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or
    ///     resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public new async ValueTask DisposeAsync()
    {
        if (Page != null) await Page.DisposeAsync();



        if (Browser != null) await Browser.DisposeAsync();

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

            _aggregator?.Publish(new StatusMessage("Login to site was successful"));
        }
        catch (Exception e)
        {

            Log.LogError(e, "Error during site login process");
        }

    }






    /// <inheritdoc />
    public Task<string[]> ExtractImageLinksFromPageAsync(string selector)
    {
        throw new NotImplementedException();
    }






    /// <inheritdoc />
    public Task<string[]> ExtractSourceUrlFromElementAsync(string selector)
    {
        throw new NotImplementedException();
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
                _aggregator?.Publish(new StatusMessage($"Element with selector '{selector}' not found."));
                return null;
            }

            return await element.GetPropertyAsync("textContent").Result.JsonValueAsync<string>();
        }
        catch (WaitTaskTimeoutException)
        {
            _aggregator?.Publish(new StatusMessage($"Timeout waiting for selector '{selector}'."));
            return null;
        }
        catch (Exception ex)
        {
            _aggregator?.Publish(new StatusMessage($"Error getting text from selector '{selector}': {ex.Message}"));
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
            _aggregator?.Publish(new StatusMessage($"Error getting node collection for selector '{selector}': {ex.Message}"));
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
            _aggregator?.Publish(new StatusMessage($"Error getting page title: {ex.Message}"));
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
        if (Page is null)
        {
            Log.LogCritical("Unable to use browser Page object. Object is null.");
            throw new InvalidOperationException("Browser Page object is null.");
        }

        try
        {
            _aggregator?.Publish(new StatusMessage($"Navigating to {url}..."));
            Log.LogInformation($"Navigating to page {url}...");

            var response = await Page.GoToAsync(url, DefaultTimeout, [WaitUntilNavigation.DOMContentLoaded, WaitUntilNavigation.Networkidle0]).ConfigureAwait(false);

            if (!response.Ok) _aggregator?.Publish(new StatusMessage($"Navigation failed with status: {response.Status}"));
            _aggregator?.Publish(new StatusMessage("Navigation successful."));

        }
        catch (Exception ex)
        {
            _aggregator?.Publish(new StatusMessage($"Error navigating to {url}: {ex.Message}"));

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
            _aggregator?.Publish(new StatusMessage($"Error checking visibility of element '{selector}': {ex.Message}"));
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
            _aggregator?.Publish(new StatusMessage($"Error in QuerySelectorAsync: {ex.Message}"));
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