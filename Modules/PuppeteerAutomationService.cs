// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using Microsoft.Extensions.Logging;

using PuppeteerSharp;



namespace MediaRecycler.Modules;


/// <summary>
///     Provides high level abstraction of puppeteer sharp internal functions
///     Purpose is to provide a clean interface for web automation tasks and to encapsulate
///     many of the boilerplate code required to interact with PuppeteerSharp. like waiting for selectors.
/// </summary>
public interface IWebAutomationService
{



    Task ClickElementAsync(string defaultPaginationSelector);






    /// <summary>
    ///     Attempts to log in to the current site asynchronously.
    /// </summary>
    /// <remarks>
    ///     This method navigates to the login page, fills in the credentials retrieved from
    ///     environment variables, and submits the login form. It also includes a delay to
    ///     ensure the login process completes before proceeding.
    /// </remarks>
    /// <returns>
    ///     A <see cref="Task" /> that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if required elements (e.g., email or password input fields) are not found on the page.
    /// </exception>
    Task DoSiteLoginAsync();






    Task<string[]> ExtractImageLinksFromPageAsync(string selector);
    Task<string[]> ExtractSourceUrlFromElementAsync(string selector);



    Task<string> GetElementTextAsync(string selector);






    /// <summary>
    ///     Retrieves a collection of nodes from the current page that match the specified CSS selector.
    /// </summary>
    /// <param name="selector">
    ///     The CSS selector used to identify the nodes to retrieve. This should be a valid CSS selector string.
    /// </param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> representing the asynchronous operation,
    ///     with a result containing an array of <see cref="IElementHandle" /> objects representing the matching nodes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="selector" /> is <c>null</c> or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the operation fails to retrieve the nodes due to an invalid selector or other page-related issues.
    /// </exception>
    /// <exception cref="Exception">
    ///     Thrown if an unexpected error occurs during the operation.
    /// </exception>
    Task<IElementHandle[]> GetNodeCollectionFromPageAsync(string selector);






    Task<string> GetPageContentsAsync(string url);






    /// <summary>
    ///     Asynchronously retrieves the title of the current web page.
    /// </summary>
    /// <remarks>
    ///     This method interacts with the browser to fetch the title of the currently loaded page.
    ///     It is useful for verifying the page content or ensuring navigation to the correct page.
    /// </remarks>
    /// <returns>
    ///     A <see cref="string" /> representing the title of the current page.
    /// </returns>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs while attempting to retrieve the page title.
    /// </exception>
    Task<string> GetPageTitleAsync();






    /// <summary>
    ///     Navigates the browser to the specified URL asynchronously.
    /// </summary>
    /// <param name="url">
    ///     The URL to navigate to. This should be a valid and well-formed URL string.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> that represents the asynchronous operation.
    /// </returns>
    Task GoToAsync(string url);






    /// <summary>
    ///     Determines whether an element specified by the given CSS selector is visible on the current page.
    /// </summary>
    /// <param name="selector">
    ///     The CSS selector used to identify the element. This should be a valid CSS selector string.
    /// </param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> representing the asynchronous operation,
    ///     with a result of <c>true</c> if the element is visible; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="selector" /> is <c>null</c> or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the operation fails to determine the visibility of the element due to an invalid selector
    ///     or other page-related issues.
    /// </exception>
    /// <exception cref="Exception">
    ///     Thrown if an unexpected error occurs during the operation.
    /// </exception>
    Task<bool> IsElementVisibleAsync(string selector);






    /// <summary>
    ///     Navigates to the next page in the current web automation context asynchronously.
    /// </summary>
    /// <remarks>
    ///     This method is typically used in scenarios where pagination is involved,
    ///     and the next page needs to be loaded for further processing or data extraction.
    ///     The implementation of this method may vary depending on the specific pagination mechanism of the website.
    /// </remarks>
    /// <returns>
    ///     A <see cref="Task" /> that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs while attempting to navigate to the next page.
    /// </exception>
    Task NavigateToNextPageAsync();






    Task<IElementHandle?> QuerySelectorAsync(string selector);






    /// <summary>
    ///     Waits asynchronously for a specific selector to appear on the page.
    /// </summary>
    /// <param name="waitForSelector">
    ///     The CSS selector to wait for. This should be a valid selector string that identifies the desired element.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> that represents the asynchronous operation. The task completes when the selector appears on
    ///     the page.
    /// </returns>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs while waiting for the selector, such as a timeout or invalid selector.
    /// </exception>
    Task WaitForSelectorAsync(string waitForSelector);

}


// --- 3. Concrete Implementation with Error Handling ---


/// <summary>
///     Implements the IWebAutomationService using PuppeteerSharp.
///     It encapsulates all low-level browser interactions and includes
///     error handling and retry logic for individual actions.
/// </summary>
public class PuppeteerAutomationService : IWebAutomationService
{

    private readonly IEventAggregator? _aggregator;

    private readonly IPage _page;






    public PuppeteerAutomationService(IPage page, IEventAggregator aggregator)
    {
        _page = page ?? throw new ArgumentNullException(nameof(page));
        _aggregator = aggregator;

    }






    /// <summary>
    ///     Clicks on an HTML element specified by the given CSS selector and waits for the page navigation to complete.
    /// </summary>
    /// <param name="paginationSelector">
    ///     The CSS selector of the element to be clicked. This selector should uniquely identify the target element on the
    ///     page.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task completes when the click action is performed
    ///     and the page navigation triggered by the click is finished.
    /// </returns>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs while attempting to click the element or during the navigation process.
    /// </exception>
    public async Task ClickElementAsync(string paginationSelector)
    {
        try
        {
            _aggregator?.Publish($"Clicking element with selector '{paginationSelector}'...");
            Program.Logger.LogDebug($"Clicking element with selector '{paginationSelector}'...");
            await _page.ClickAsync(paginationSelector).ConfigureAwait(false);
            _ = await _page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded } }).ConfigureAwait(false);


        }
        catch (Exception ex)
        {
            _aggregator?.Publish($"Error clicking element '{paginationSelector}': {ex.Message}");
            throw;
        }
    }






    /// <summary>
    ///     Attempts to log in to the current page using credentials from environment variables.
    /// </summary>
    public async Task DoSiteLoginAsync()
    {
        _ = await _page.GoToAsync("https://www.bdsmlr.com/login").ConfigureAwait(false);
        await Task.Delay(5000);

        var element1Handle = await _page.QuerySelectorAsync("input#email").ConfigureAwait(false);

        if (element1Handle == null)
        {
            return;
        }

        var email = Environment.GetEnvironmentVariable("SCRAPER_EMAIL");
        await element1Handle.TypeAsync(email).ConfigureAwait(false);

        var element2Handle = await _page.QuerySelectorAsync("input#password").ConfigureAwait(false);

        if (element2Handle == null)
        {
            return;
        }

        var password = Environment.GetEnvironmentVariable("SCRAPER_PASSWORD");
        await element2Handle.TypeAsync(password).ConfigureAwait(false);

        await _page.ClickAsync("button[type=submit]").ConfigureAwait(false);
        await Task.Delay(5000).ConfigureAwait(false);
        _aggregator?.Publish("Login to site was successful");

    }






    public Task<string[]> ExtractImageLinksFromPageAsync(string selector)
    {
        throw new NotImplementedException();
    }






    public Task<string[]> ExtractSourceUrlFromElementAsync(string selector)
    {
        throw new NotImplementedException();
    }






    public async Task<string> GetElementTextAsync(string selector)
    {
        try
        {
            _ = await _page.WaitForSelectorAsync(selector, new WaitForSelectorOptions { Timeout = 5000 }).ConfigureAwait(false);
            var element = await _page.QuerySelectorAsync(selector).ConfigureAwait(false);

            if (element == null)
            {
                _aggregator?.Publish($"Element with selector '{selector}' not found.");
                return null!;
            }

            return await element.GetPropertyAsync("textContent").Result.JsonValueAsync<string>();
        }
        catch (WaitTaskTimeoutException)
        {
            _aggregator?.Publish($"Timeout waiting for selector '{selector}'.");
            return null!;
        }
        catch (Exception ex)
        {
            _aggregator?.Publish($"Error getting text from selector '{selector}': {ex.Message}");
            throw;
        }
    }






    public async Task<IElementHandle[]> GetNodeCollectionFromPageAsync(string selector)
    {
        try
        {
            var elements = await _page.QuerySelectorAllAsync(selector).ConfigureAwait(false);
            return elements;
        }
        catch (Exception ex)
        {
            _aggregator?.Publish($"Error getting node collection for selector '{selector}': {ex.Message}");
            return [];
        }
    }






    public async Task<string> GetPageContentsAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentNullException(url, "url cannot be null");
        }

        try
        {

            _ = await _page.GoToAsync(url, new NavigationOptions
            {
                        WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded }, Timeout = 20000 // 20 seconds
            }).ConfigureAwait(false);
            return await _page.GetContentAsync().ConfigureAwait(false);

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
            return await _page.GetTitleAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _aggregator?.Publish($"Error getting page title: {ex.Message}");
            throw;
        }
    }






    public async Task GoToAsync(string url)
    {
        try
        {
            _aggregator?.Publish($"Navigating to {url}...");
            Program.Logger.LogInformation($"Navigating to page {url}...");

            var response = await _page.GoToAsync(url, WaitUntilNavigation.DOMContentLoaded).ConfigureAwait(false);

            if (response.Ok)
            {
                _aggregator?.Publish("Navigation successful.");
                return;
            }

            _aggregator?.Publish($"Navigation failed with status: {response.Status}");
        }
        catch (Exception ex)
        {
            _aggregator?.Publish($"Error navigating to {url}: {ex.Message}");

        }

        await Task.Delay(RetryDelayMilliseconds).ConfigureAwait(false);

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
            var element = await _page.QuerySelectorAsync(selector).ConfigureAwait(false);

            if (element == null)
            {
                return false;
            }

            var boundingBox = await element.BoundingBoxAsync();
            return boundingBox != null && boundingBox.Width > 0 && boundingBox.Height > 0;
        }
        catch (Exception ex)
        {
            _aggregator?.Publish($"Error checking visibility of element '{selector}': {ex.Message}");
            return false;
        }
    }






    public Task NavigateToNextPageAsync()
    {
        throw new NotImplementedException();
    }






    public async Task<IElementHandle?> QuerySelectorAsync(string selector)
    {
        if (string.IsNullOrWhiteSpace(selector))
        {
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));
        }

        try
        {
            return await _page.QuerySelectorAsync(selector).ConfigureAwait(false) ?? throw new InvalidOperationException($"No element found for selector: {selector}");
        }
        catch (Exception ex)
        {
            _aggregator?.Publish(new { Message = $"Error in QuerySelectorAsync: {ex.Message}", Exception = ex });
            throw;
        }
    }






    /// <summary>
    ///     Automatically waits for a selector to appear on the page.
    /// </summary>
    /// <param name="waitForSelector"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task WaitForSelectorAsync(string waitForSelector) =>
                _page.WaitForSelectorAsync(waitForSelector, new WaitForSelectorOptions { Timeout = 30000 });






    private const int MaxRetries = 3;
    private const int RetryDelayMilliseconds = 1000;






    public Task<string[]> ExtractVideoLinksFromPageAsync(string selector)
    {
        throw new NotImplementedException();
    }

}
