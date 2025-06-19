// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;



namespace MediaRecycler.Modules;


// Inherits from the refactored base class
public class PuppetBrowserContextBase : PuppetBrowserBase
{

    private readonly ILogger _logger;






    public PuppetBrowserContextBase(ILogger logger) : base(logger)
    {

        _logger = logger;
        
    }






    // Use private set for better encapsulation
    protected IBrowserContext? Context { get; private set; }






    // Fix for CS8774: Ensure 'Context' is assigned a non-null value before exiting the method.
    [MemberNotNull(nameof(Context))]
    protected Task CreateContextAsync()
    {
        Context = Browser != null ? Browser.DefaultContext ?? throw new InvalidOperationException("Browser.DefaultContext is null.") : throw new InvalidOperationException("Browser is null.");

        return Task.CompletedTask;
    }






    protected virtual async Task DisposeAsyncCore()
    {
        _logger.LogDebug("Disposing Context asynchronously...");

        if (Context is { IsClosed: false })
        {
            try
            {
                await Context.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing the Puppeteer context.");

                // Swallow or handle exception during disposal as needed
            }
            finally
            {
                Context = null!; // Set to null after disposal attempt
            }
        }

        _logger.LogDebug("Calling base DisposeAsyncCore...");
        await DisposeAsync();

        _logger.LogDebug("Context asynchronous disposal complete.");

    }

}
