// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using MediaRecycler.Modules.Interfaces;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;
using PuppeteerSharp.Helpers;



namespace MediaRecycler.Modules;




/// <summary>
///     Implements the IWebAutomationService using PuppeteerSharp.
///     It encapsulates common puppeteersharp methods low-level browser interactions and includes
///     error handling and retry logic for individual actions.
/// </summary>
public class PuppeteerAutomationService :  IAsyncDisposable, IWebAutomationService
{

    private readonly IEventAggregator? _aggregator;

    private readonly PuppeteerManager _puppeteerManager;
    private readonly int DefaultTimeout = 90000; // Default timeout for operations in milliseconds







    public PuppeteerAutomationService(IEventAggregator aggregator)
    {
        _aggregator = aggregator;
        _puppeteerManager = new PuppeteerManager(aggregator);

    }







    public async Task InitializeAsync()
    {
        await _puppeteerManager.InitAsync();

        if (_puppeteerManager.Page is null)
        {
            Program.Logger.LogError("Failed to initialize Puppeteer page.");
            throw new InvalidOperationException("Page initialization failed.");
        }
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
            _aggregator?.Publish(new StatusMessage($"Clicking element with selector '{paginationSelector}'..."));
            Program.Logger.LogDebug($"Clicking element with selector '{paginationSelector}'...");
            await _puppeteerManager.Page.ClickAsync(paginationSelector).ConfigureAwait(false);
            _ = await _puppeteerManager.Page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded } })
                        .WithTimeout(DefaultTimeout).ConfigureAwait(false);


        }
        catch (Exception ex)
        {
            _aggregator?.Publish(new StatusMessage($"Error clicking element '{paginationSelector}': {ex.Message}"));
            throw;
        }
    }







    /// <summary>
    ///     Attempts to log in to the current page using credentials from environment variables.
    /// </summary>
    public async Task DoSiteLoginAsync()
    {
        _ = await _puppeteerManager.Page.GoToAsync("https://www.bdsmlr.com/login").ConfigureAwait(false);
        await Task.Delay(5000);

        try
        {
            var element1Handle = await _puppeteerManager.Page.QuerySelectorAsync("input#email").ConfigureAwait(false);

            if (element1Handle == null)
            {
                return;
            }

            string? email = Environment.GetEnvironmentVariable("SCRAPER_EMAIL");
            await element1Handle.TypeAsync(email).ConfigureAwait(false);

            var element2Handle = await _puppeteerManager.Page.QuerySelectorAsync("input#password").ConfigureAwait(false);

            if (element2Handle == null)
            {
                return;
            }

            string? password = Environment.GetEnvironmentVariable("SCRAPER_PASSWORD");
            await element2Handle.TypeAsync(password).ConfigureAwait(false);

            await _puppeteerManager.Page.ClickAsync("button[type=submit]").ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            _aggregator?.Publish(new StatusMessage("Login to site was successful"));
        }
        catch (Exception e)
        {

            Program.Logger.LogError(e, "Error during site login process");
        }

    }







    public Task<string[]> ExtractImageLinksFromPageAsync(string selector)
    {
        throw new NotImplementedException();
    }







    public Task<string[]> ExtractSourceUrlFromElementAsync(string selector)
    {
        throw new NotImplementedException();
    }







    public async Task<string?> GetElementTextAsync(string selector)
    {
        try
        {
            _ = await _puppeteerManager.Page.WaitForSelectorAsync(selector, new WaitForSelectorOptions { Timeout = 15000 }).ConfigureAwait(false);
            var element = await _puppeteerManager.Page.QuerySelectorAsync(selector).WithTimeout(DefaultTimeout).ConfigureAwait(false);

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
            await _puppeteerManager.DisposeAsync();
        }
    }







    public async Task<IElementHandle[]> GetNodeCollectionFromPageAsync(string selector)
    {
        try
        {
            var elements = await _puppeteerManager.Page.QuerySelectorAllAsync(selector).WithTimeout(DefaultTimeout).ConfigureAwait(false);
            return elements;
        }
        catch (Exception ex)
        {
            _aggregator?.Publish(new StatusMessage($"Error getting node collection for selector '{selector}': {ex.Message}"));
            return [];
        }
    }






    /// <summary>
    /// Gets the contents of the current page the PuppeteerManager is attached to.
    /// Method does not navigate to a new page, it simply retrieves the HTML content of the current page.
    /// </summary>
    /// <returns>String containing the HTML of the current page</returns>
    public async Task<string> GetPageContentsAsync()
    {
        return await _puppeteerManager.Page.GetContentAsync().ConfigureAwait(false);
    }




    public async Task<string> GetPageContentsAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentNullException(url, "url cannot be null");
        }

        try
        {

            _ = await _puppeteerManager.Page.GoToAsync(url, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded }, Timeout = 20000 // 20 seconds
            }).ConfigureAwait(false);
            return await _puppeteerManager.Page.GetContentAsync().ConfigureAwait(false);

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
            return await _puppeteerManager.Page.GetTitleAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _aggregator?.Publish(new StatusMessage($"Error getting page title: {ex.Message}"));
            throw;
        }
    }







    public async Task GoToAsync(string url)
    {
        try
        {
            _aggregator?.Publish(new StatusMessage($"Navigating to {url}..."));
            Program.Logger.LogInformation($"Navigating to page {url}...");

            var response = await _puppeteerManager.Page.GoToAsync(url, WaitUntilNavigation.DOMContentLoaded).WithTimeout(DefaultTimeout).ConfigureAwait(false);

            if (response.Ok)
            {
                _aggregator?.Publish(new StatusMessage("Navigation successful."));
                return;
            }

            _aggregator?.Publish(new StatusMessage($"Navigation failed with status: {response.Status}"));
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
            var element = await _puppeteerManager.Page.QuerySelectorAsync(selector).WithTimeout(DefaultTimeout).ConfigureAwait(false);

            if (element == null)
            {
                return false;
            }

            var boundingBox = await element.BoundingBoxAsync();
            return boundingBox != null && boundingBox.Width > 0 && boundingBox.Height > 0;
        }
        catch (Exception ex)
        {
            _aggregator?.Publish(new StatusMessage($"Error checking visibility of element '{selector}': {ex.Message}"));
            return false;
        }
    }







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
            Program.Logger.LogError("Selector cannot be null or empty. {selector}", nameof(selector));
            return null;
        }

        try
        {
            return await _puppeteerManager.Page.QuerySelectorAsync(selector).WithTimeout(DefaultTimeout).ConfigureAwait(false);
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
                _puppeteerManager.Page.WaitForSelectorAsync(waitForSelector, new WaitForSelectorOptions { Timeout = 60000 });









    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or
    /// resetting unmanaged resources asynchronously.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_puppeteerManager?.Page != null)
        {
            await _puppeteerManager.Page.DisposeAsync();
        }
    }

}