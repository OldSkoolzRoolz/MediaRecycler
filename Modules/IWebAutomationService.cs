// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using PuppeteerSharp;



namespace MediaRecycler.Modules;


public interface IWebAutomationService
{

    Task InitializeAsync();







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
    Task ClickElementAsync(string paginationSelector);







    /// <summary>
    ///     Attempts to log in to the current page using credentials from environment variables.
    /// </summary>
    Task DoSiteLoginAsync();







    Task<string[]> ExtractImageLinksFromPageAsync(string selector);

    Task<string[]> ExtractSourceUrlFromElementAsync(string selector);

    Task<string?> GetElementTextAsync(string selector);

    Task<IElementHandle[]> GetNodeCollectionFromPageAsync(string selector);







    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<string> GetPageContentsAsync();







    Task<string> GetPageContentsAsync(string url);







    /// <summary>
    ///     Asynchronously retrieves the title of the current page.
    /// </summary>
    /// <returns>A <see cref="string" /> representing the title of the current page.</returns>
    Task<string> GetPageTitleAsync();







    Task GoToAsync(string url);







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
    Task<bool> IsElementVisibleAsync(string selector);







    Task NavigateToNextPageAsync();







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
    Task<IElementHandle?> QuerySelectorAsync(string selector);







    /// <summary>
    ///     Automatically waits for a selector to appear on the page.
    /// </summary>
    /// <param name="waitForSelector"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    Task WaitForSelectorAsync(string waitForSelector);







    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or
    /// resetting unmanaged resources asynchronously.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    ValueTask DisposeAsync();

}