// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using System.Linq.Expressions;

using MediaRecycler.Modules;
using MediaRecycler.Modules.Loggers;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediaRecycler.Modules.Interfaces;



namespace MediaRecycler;


internal  class Program
{

    public static ILogger? Logger { get; private set; }







    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static async Task Main(string[] args)
    {

        Application.SetCompatibleTextRenderingDefault(false);
        Application.EnableVisualStyles();
        Application.SetHighDpiMode(HighDpiMode.SystemAware);


        var host = CreateHostBuilder(args).Build();

        var serviceProvider = host.Services;
        ILogger? logger = null;
try{
        //Ensure we don't have any left over browser processes running
        ProcessUtils.TerminateBrowserProcesses();

        //resolve logger
        logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Application Starting");

        //Start the host.. This runs background services.
        // We don't await host.RunAsync() because its a blocking call
        // and we need to run the Windforms message loop.
        // We will start it and then run the form...
        _ = host.StartAsync();

        //resolve and run the main form. The DI container will inject its dependencies
        var mainForm = serviceProvider.GetRequiredService<MainForm>();
        Application.Run(mainForm);
    }
    catch(Exception ex)
    {
        logger?.LogCritical(ex, "A fatal error occurred during application startup or execution.");
        MessageBox.Show(ex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Gracefully stop the host and dispose of services.
            logger?.LogInformation("Application shutting down.");
            await host.StopAsync();
            host.Dispose();
            Application.Exit();
        }

    }




  private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // Configure all your services here.
                ConfigureServices(services, hostContext.Configuration);
            });




       // services.AddDbContext<MRContext>(options => options.UseSqlServer(Properties.Settings.Default.ConnString));



     //   DependencyInjectionConfig.ConfigureServices(services);







      



     static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<MRContext>(options =>
            options.UseSqlServer(Properties.Settings.Default.ConnString));

        // Register application services from a separate configuration class
        DependencyInjectionConfig.ConfigureServices(services);

        // Register the main form
        services.AddTransient<MainForm>();

        // Configure logging
        services.AddLogging(logBuilder =>
        {
            logBuilder.ClearProviders(); // Clear default providers
            logBuilder.AddConfiguration(configuration.GetSection("Logging"));
            logBuilder.AddConsole();
            logBuilder.AddDebug();
            // Add other providers like a file logger here if needed.
            // logBuilder.AddProvider(new FileLoggerProvider("logs/app.log", LogLevel.Trace));
            logBuilder.SetMinimumLevel(LogLevel.Trace);

            // Special handling for the WinForms ControlLoggerProvider
            // We add it after the ServiceProvider is built because it needs an instance of a control.
            // This is a common pattern for UI logging in DI-enabled desktop apps.
            services.AddSingleton(sp =>
            {
                var mainForm = sp.GetRequiredService<MainForm>();
                var factory = sp.GetRequiredService<ILoggerFactory>();
                var provider = new ControlLoggerProvider(mainForm.MainLogRichTextBox, LogLevel.Trace);
                factory.AddProvider(provider);
                return provider; // Return it so it can be managed by the container if needed.
            });
        });
    }

}

//TODO: Create helper to centralize logging for all types implemented in the application

//TODO: Streamline error handling and logging across the application, ensuring consistent error messages and logging levels.




/// <summary>
/// 
/// </summary>
public class DependencyInjectionConfig
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // 1. Register IEventAggregator
        // Assuming you have a concrete implementation of IEventAggregator, e.g., EventAggregatorImpl
        services.AddSingleton<IEventAggregator, EventAggregator>(); // Replace with your actual implementation

        // 2. Register IWebAutomationService
        services.AddSingleton<IWebAutomationService, PuppeteerAutomationService>();

        // 3. Register IDownloaderModule
        services.AddSingleton<IUrlDownloader, UrlDownloader>();

        // 4. Register BlogScraper (it depends on the above)
        services.AddTransient<IBlogScraper, BlogScraper>();

        // 5. Add a logging provider (if not already added)
        services.AddLogging(builder =>
        {
            builder.AddConsole(); // Example: Use console logging.  Adjust as needed.
            // You can add other logging providers here, e.g., file logging, database logging, etc.
        });

        // Note:  If you need to access configuration (e.g., from appsettings.json),
        // you'll need to inject IConfiguration into this method and use it to
        // configure your services (e.g., for options).
    }
}

internal class EventAggregatorImpl
{
}