// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"



// Added for CancellationTokenRegistration




using Microsoft.Extensions.Logging;

using PuppeteerSharp;

using ErrorEventArgs = PuppeteerSharp.ErrorEventArgs;



namespace MediaRecycler.Modules;


/// <summary>
///     Base class for managing a single Puppeteer Page within a Browser Context.
///     Handles creation, disposal, and provides common page operations or helpers.
///     Inherits resource management from PuppetBrowserContextBase.
/// </summary>
public class PuppetPageBase : PuppetBrowserContextBase, IAsyncDisposable
{


    protected readonly ILogger _pageLogger;






    public PuppetPageBase(ILogger logger) : base(logger)
    {
        _pageLogger = logger;
    }






    // The managed Puppeteer Page instance. Null if not created or disposed.
    protected IPage? Page { get; private set; }






    /// <summary>
    ///     Internal helper to safely close and dispose a page instance.
    /// </summary>
    /// <param name="pageToDispose">The page instance to close and dispose.</param>
    private async Task CloseAndDisposePageInternalAsync(IPage? pageToDispose)
    {
        // Avoid operations on null or already closed/disposed pages.
        // Note: DisposeAsync generally handles closed pages gracefully, but explicit checks add clarity.
        if (pageToDispose == null || pageToDispose.IsClosed)
        {
            _pageLogger.LogDebug("CloseAndDisposePageInternalAsync called with null or already closed page.");

            // Attempt disposal anyway, as DisposeAsync should be idempotent or handle disposed state.
            if (pageToDispose != null)
            {
                try
                {
                    await pageToDispose.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _pageLogger.LogWarning(ex, "Exception during DisposeAsync on an already closed or potentially disposed page. Swallowing.");
                }
            }

            return;
        }

        try
        {
            _pageLogger.LogDebug("Closing page (Page.CloseAsync)...");

            // Although DisposeAsync often implies CloseAsync, calling it explicitly can sometimes
            // help ensure cleaner shutdown depending on the state Puppeteer leaves the page in.
            // It's generally safe to call both.
            await pageToDispose.CloseAsync();
            _pageLogger.LogDebug("Page closed. Disposing page (Page.DisposeAsync)...");
            await pageToDispose.DisposeAsync();
            _pageLogger.LogDebug("Page disposed successfully.");
        }
        catch (Exception ex)
        {
            // Log errors during cleanup, but don't let them stop the disposal process.
            _pageLogger.LogError(ex, "Error occurred during explicit closing or disposing of the Puppeteer page. Swallowing exception to ensure disposal continues.");

            // Attempt DisposeAsync again in case CloseAsync failed but Dispose might work, or vice-versa.
            // This is defensive; Puppeteer's DisposeAsync should ideally handle prior failures.
            try
            {
                await pageToDispose.DisposeAsync();
            }
            catch
            {
                /* Ignore secondary exception */
            }
        }
    }






    /// <summary>
    ///     Explicitly closes and disposes the currently managed page, if it exists and is open.
    ///     This is useful for scenarios where you want to release page resources before the entire
    ///     PuppetPageBase instance is disposed.
    /// </summary>
    public async Task ClosePageAsync()
    {
        if (Page != null && !Page.IsClosed)
        {
            _pageLogger.LogInformation("Closing and disposing page explicitly via ClosePageAsync().");
            await CloseAndDisposePageInternalAsync(Page);
            Page = null; // Set Page reference to null after disposal.
        }
        else
        {
            _pageLogger.LogDebug("ClosePageAsync called, but the page was already null or closed.");
        }
    }






    /// <summary>
    ///     Creates a new Puppeteer Page within the browser context.
    ///     Ensures the underlying browser context is created first.
    ///     If a page already exists and is open, it will be closed and disposed before creating the new one.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the browser context has not been initialized.</exception>
    /// <exception cref="Exception">Propagates exceptions from PuppeteerSharp during page creation.</exception>
    public virtual async Task CreatePageAsync()
    {
        _pageLogger.LogInformation("Attempting to create a new page...");

        // Ensure the parent context is ready before creating a page within it.
        //  await base.CreateContextAsync();

        if (Context == null)
        {
            _pageLogger.LogError("Cannot create page because the browser context is null.");

            // This indicates a problem in the base class or initialization order.
            throw new InvalidOperationException("Browser context is not initialized. Cannot create page.");
        }

        // Ensure any previously managed page by this instance is cleaned up.
        if (Page != null && !Page.IsClosed)
        {
            _pageLogger.LogWarning("An existing page instance was found and is open. Closing and disposing it before creating a new one.");
            await CloseAndDisposePageInternalAsync(Page); // Use helper to avoid code duplication
        }

        // Ensure Page is null before attempting creation, even if CloseAndDisposePageInternalAsync failed somehow.
        Page = null;

        try
        {
            //A new page is created when a brower is initialized.
            //Verifing existance and using
            _pageLogger.LogDebug("Setting page Property...");

            if (Browser is not null)
            {





                //New page is inherently opened when a new browser is launched we will attempt to use it first.
                var pages = await Browser.PagesAsync();

                if (pages != null && pages.Length > 0)
                {
                    Page = pages[0]; //use the first page
                }
                else
                {
                    Page = await Context.NewPageAsync();
                    _pageLogger.LogDebug("Context.NewPageAsync() returned.");
                }

                // Page =    await Context.NewPageAsync();
                _pageLogger.LogInformation("New page created successfully.");
            }
        }
        catch (Exception ex)
        {
            _pageLogger.LogError(ex, "Failed to create a new page using Context.NewPageAsync().");

            // Ensure Page is null if creation failed partway through. Puppeteer might handle
            // internal cleanup, but our reference should be null.
            Page = null;
            throw; // Re-throw the exception so the caller knows creation failed.
        }
    }






    // --- IAsyncDisposable Implementation ---






    /// <summary>
    ///     Asynchronously disposes resources managed by this class (the Page)
    ///     and then calls the base class's disposal logic.
    /// </summary>
    protected new async Task DisposeAsyncCore()
    {
        _pageLogger.LogDebug("Disposing PuppetPageBase resources asynchronously...");

        if (Page != null)
        {
            _pageLogger.LogDebug("Page instance found. Attempting to close and dispose it.");

            // Use the internal helper for consistency.
            // We capture the reference in case Page is set to null concurrently (though unlikely here).
            var pageToDispose = Page;
            Page = null; // Set to null immediately to signal disposal has started.
            await CloseAndDisposePageInternalAsync(pageToDispose);
        }
        else
        {
            _pageLogger.LogDebug("No active page instance to dispose.");
        }

        _pageLogger.LogDebug("Calling base.DisposeAsyncCore() to dispose browser context resources...");

        // VERY IMPORTANT: Call the base implementation *after* disposing resources owned by this derived class.
        await base.DisposeAsyncCore();
        _pageLogger.LogDebug("PuppetPageBase asynchronous disposal complete.");
    }






    /// <summary>
    ///     Asynchronously waits for the Page's 'Error' event to occur.
    ///     Handles cancellation requests and potential race conditions with page closure.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>
    ///     A Task that completes with the ErrorEventArgs when the event occurs,
    ///     completes as Canceled if the cancellationToken is triggered,
    ///     or completes with an InvalidOperationException if the page is not valid or closes during the wait setup.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the Page is null, closed, or closes unexpectedly during setup.</exception>
    protected Task<ErrorEventArgs> WaitForError(CancellationToken cancellationToken = default)
    {
        // Capture the current page reference to work with, mitigating race conditions
        // if `this.Page` is set to null by disposal on another thread.
        var currentPage = Page;

        // --- Initial State Check ---
        if (currentPage == null || currentPage.IsClosed)
        {
            _pageLogger.LogWarning("WaitForError called but Page is null or already closed.");

            // Return a faulted task immediately for invalid state.
            return Task.FromException<ErrorEventArgs>(new InvalidOperationException("Cannot wait for error: Page is not initialized or has been closed."));
        }

        TaskCompletionSource<ErrorEventArgs> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationTokenRegistration tokenRegistration = default;


        // --- Event Handler ---
        // Define the handler that will complete the TaskCompletionSource.
        void ErrorEventHandler(object? sender, ErrorEventArgs e)
        {
            _pageLogger.LogDebug("Page.Error event received.");

            // **CRITICAL:** Unsubscribe *before* completing the task to prevent potential
            // re-entrancy or handling duplicate events if they fire rapidly.
            // Use the captured 'currentPage' and re-check its state for safety.
            var pageNow = Page; // Re-check the main Page field as well

            if (pageNow != null && !pageNow.IsClosed)
            {
                pageNow.Error -= ErrorEventHandler;
            }
            else if (currentPage != null && !currentPage.IsClosed) // Fallback to captured if main is null now
            {
                currentPage.Error -= ErrorEventHandler;
            }

            tokenRegistration.Dispose(); // Clean up the cancellation registration.
            _ = tcs.TrySetResult(e); // Use TrySetResult for safety against race conditions (e.g., with cancellation).
        }


        // --- Cancellation Handling Setup ---
        if (cancellationToken.CanBeCanceled)
        {
            tokenRegistration = cancellationToken.Register(() =>
            {
                _pageLogger.LogInformation("Cancellation requested while waiting for page error.");

                // Unsubscribe on cancellation to prevent the handler from being called later
                // and to avoid resource leaks. Re-check state before unsubscribing.
                var pageNow = Page;

                if (pageNow != null && !pageNow.IsClosed)
                {
                    pageNow.Error -= ErrorEventHandler;
                }
                else if (currentPage != null && !currentPage.IsClosed)
                {
                    currentPage.Error -= ErrorEventHandler;
                }

                // Complete the task as canceled. Use TrySetCanceled.
                _ = tcs.TrySetCanceled(cancellationToken);
            });
        }

        // --- Pre-Subscription State Check ---
        // Check the page status and cancellation *again* right before subscribing
        // to minimize the window for race conditions.
        if (currentPage.IsClosed || cancellationToken.IsCancellationRequested)
        {
            tokenRegistration.Dispose(); // Clean up registration if we don't subscribe.

            if (currentPage.IsClosed)
            {
                _pageLogger.LogWarning("Page closed after initial check but before subscribing to the Error event.");
                return Task.FromException<ErrorEventArgs>(new InvalidOperationException("Page closed while attempting to wait for error event subscription."));
            }

            // Must be cancellation requested
            _pageLogger.LogDebug("Cancellation requested after initial check but before subscribing to the Error event.");
            return Task.FromCanceled<ErrorEventArgs>(cancellationToken);
        }

        // --- Subscribe to the Event ---
        // Subscribe *after* setting up cancellation and performing the pre-subscription check.
        currentPage.Error += ErrorEventHandler;
        _pageLogger.LogDebug("Subscribed to Page.Error event.");

        // --- Post-Subscription State Check ---
        // Final check for immediate closure or cancellation *after* subscription.
        // This handles the unlikely edge case where the state changed between the `+=` operation and this check.
        if (currentPage.IsClosed || cancellationToken.IsCancellationRequested)
        {
            // Attempt to unsubscribe immediately if the state changed right after subscribing.
            // Re-check state before unsubscribing.
            var pageNow = Page;

            if (pageNow != null && !pageNow.IsClosed)
            {
                pageNow.Error -= ErrorEventHandler;
            }
            else if (!currentPage.IsClosed)
            {
                currentPage.Error -= ErrorEventHandler;
            }

            tokenRegistration.Dispose(); // Clean up registration.

            if (currentPage.IsClosed)
            {
                _pageLogger.LogWarning("Page closed immediately after subscribing to the Error event.");

                // Complete the task exceptionally, consistent with other closure cases.
                _ = tcs.TrySetException(new InvalidOperationException("Page closed immediately after waiting for error event subscription."));
            }
            else
            {
                // Must be cancellation
                _pageLogger.LogDebug("Cancellation requested immediately after subscribing to the Error event.");
                _ = tcs.TrySetCanceled(cancellationToken);
            }
        }

        // --- End Race Condition Handling ---

        _pageLogger.LogDebug("Waiting for Page.Error event or cancellation...");
        return tcs.Task;
    }

}
