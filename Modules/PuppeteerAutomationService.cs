// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using MediaRecycler.Modules.Interfaces;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using PuppeteerSharp;



namespace MediaRecycler.Modules;




/// <summary>
///     Implements the IWebAutomationService using PuppeteerSharp.
///     It encapsulates common puppeteersharp methods low-level browser interactions and includes
///     error handling and retry logic for individual actions.
/// </summary>
public class PuppeteerAutomationService : IWebAutomationService, IAsyncDisposable
{

    private readonly IEventAggregator? _aggregator;

    //TODO: Remove this logger and use the one from Program.cs
    private readonly ILogger _logger = NullLogger.Instance;

    private readonly IPage _page;
    private readonly PuppeteerManager _puppeteerManager;







    public PuppeteerAutomationService( IEventAggregator aggregator)
    {
        _aggregator = aggregator;
        _puppeteerManager = new PuppeteerManager(aggregator);

        _puppeteerManager.InitializeAsync(HeadlessBrowserOptions.Default).GetAwaiter();
        
        
        _page = _puppeteerManager.Page ?? throw new InvalidOperationException("Page cannot be null. Ensure PuppeteerManager is initialized correctly.");
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
            await _page.ClickAsync(paginationSelector).ConfigureAwait(false);
            _ = await _page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded } }).ConfigureAwait(false);


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
        _ = await _page.GoToAsync("https://www.bdsmlr.com/login").ConfigureAwait(false);
        await Task.Delay(5000);

        var element1Handle = await _page.QuerySelectorAsync("input#email").ConfigureAwait(false);

        if (element1Handle == null) return;

        string? email = Environment.GetEnvironmentVariable("SCRAPER_EMAIL");
        await element1Handle.TypeAsync(email).ConfigureAwait(false);

        var element2Handle = await _page.QuerySelectorAsync("input#password").ConfigureAwait(false);

        if (element2Handle == null) return;

        string? password = Environment.GetEnvironmentVariable("SCRAPER_PASSWORD");
        await element2Handle.TypeAsync(password).ConfigureAwait(false);

        await _page.ClickAsync("button[type=submit]").ConfigureAwait(false);
        await Task.Delay(5000).ConfigureAwait(false);
        _aggregator?.Publish(new StatusMessage("Login to site was successful"));

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
                _aggregator?.Publish(new StatusMessage($"Element with selector '{selector}' not found."));
                return null!;
            }

            return await element.GetPropertyAsync("textContent").Result.JsonValueAsync<string>();
        }
        catch (WaitTaskTimeoutException)
        {
            _aggregator?.Publish(new StatusMessage($"Timeout waiting for selector '{selector}'."));
            return null!;
        }
        catch (Exception ex)
        {
            _aggregator?.Publish(new StatusMessage($"Error getting text from selector '{selector}': {ex.Message}"));
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
            _aggregator?.Publish(new StatusMessage($"Error getting node collection for selector '{selector}': {ex.Message}"));
            return [];
        }
    }







    public async Task<string> GetPageContentsAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(url, "url cannot be null");

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

            var response = await _page.GoToAsync(url, WaitUntilNavigation.DOMContentLoaded).ConfigureAwait(false);

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
            var element = await _page.QuerySelectorAsync(selector).ConfigureAwait(false);

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
            _logger.LogError("Selector cannot be null or empty. {selector}", nameof(selector));
            return null;
        }

        try
        {
            return await _page.QuerySelectorAsync(selector).ConfigureAwait(false);
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
                _page.WaitForSelectorAsync(waitForSelector, new WaitForSelectorOptions { Timeout = 30000 });







    public Task<string[]> ExtractVideoLinksFromPageAsync(string selector)
    {
        throw new NotImplementedException();
    }







    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or
    /// resetting unmanaged resources asynchronously.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await _page.DisposeAsync();
    }

}