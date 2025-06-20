// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers


//keeper

using PuppeteerSharp;



namespace MediaRecycler.Modules.Interfaces;


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



    Task<string?> GetElementTextAsync(string selector);







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





    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<string> GetPageContentsAsync();




    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
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



    ValueTask DisposeAsync();



}