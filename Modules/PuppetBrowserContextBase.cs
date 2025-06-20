// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;



namespace MediaRecycler.Modules;


// Inherits from the refactored base class
public class PuppetBrowserContextBase : PuppetBrowserBase
{







    public PuppetBrowserContextBase() : base()
    {



    }






    // Use private set for better encapsulation
    protected IBrowserContext? Context { get; private set; }






    // Fix for CS8774: Ensure 'Context' is assigned a non-null value before exiting the method.
    [MemberNotNull(nameof(Context))]
    protected Task CreateContextTaskAsync()
    {
        Context = Browser != null ? Browser.DefaultContext ?? throw new InvalidOperationException("Browser.DefaultContext is null.") : throw new InvalidOperationException("Browser is null.");

        return Task.CompletedTask;
    }






    protected virtual async Task DisposeAsyncCore()
    {
        Program.Logger.LogDebug("Disposing Context asynchronously...");

        if (Context is { IsClosed: false })
        {
            try
            {
                await Context.CloseAsync();
            }
            catch (Exception ex)
            {
                Program.Logger.LogError(ex, "Error disposing the Puppeteer context.");

                // Swallow or handle exception during disposal as needed
            }
            finally
            {
                Context = null!; // Set to null after disposal attempt
            }
        }

        Program.Logger.LogDebug("Calling base DisposeAsyncCore...");
        await DisposeAsync();

        Program.Logger.LogDebug("Context asynchronous disposal complete.");

    }

}
