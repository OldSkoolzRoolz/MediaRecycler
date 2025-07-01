// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using MediaRecycler.Logging;
using MediaRecycler.Modules;
using MediaRecycler.Modules.Interfaces;
using MediaRecycler.Modules.Loggers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;



namespace MediaRecycler;


internal class Program
{

    public static ILogger? _logger = null;






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

        //   var mainForm = serviceProvider.GetRequiredService<MainForm>();
        //serviceProvider.GetRequiredService<MainForm>();

        try
        {
            //Ensure we don't have any left over browser processes running
            ProcessUtils.TerminateBrowserProcesses();

            //resolve logger
            _logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            Log.LogInformation("Application Starting");

            Log.WriteMessage += s => _logger.LogInformation(s);

            //Start the host.. This runs background services.
            // We don't await host.RunAsync() because its a blocking call
            // and we need to run the Windforms message loop.
            // We will start it and then run the form...
            _ = host.StartAsync();

            //resolve and run the main form. The DI container will inject its dependencies
            Application.Run(serviceProvider.GetRequiredService<MainForm>());
        }
        catch (Exception ex)
        {
            _logger?.LogCritical(ex, "A fatal error occurred during application startup or execution.");
            MessageBox.Show(ex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Gracefully stop the host and dispose of services.
            _logger?.LogInformation("Application shutting down.");
            await host.StopAsync();
            host.Dispose();
            Application.Exit();
        }

    }




    private static IHostBuilder CreateHostBuilder(string[] args) =>
              Host.CreateDefaultBuilder(args)
              .ConfigureServices((hostContext, services) =>
              {


                  // Register application services from a separate configuration class
                  DependencyInjectionConfig.ConfigureServices(services);

              });

}



/// <summary>
/// 
/// </summary>
public class DependencyInjectionConfig
{
    /// <summary>
    /// Configures the dependency injection container by registering application services and their implementations.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which services will be added.
    /// </param>
    public static void ConfigureServices(IServiceCollection services)
    {

        // 5. Add a logging provider (if not already added)
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Trace);

            // Example: Use console logging.  Adjust as needed.

            // You can add other logging providers here, e.g., file logging, database logging, etc.
        });

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

        // Add DbContext
        services.AddDbContextFactory<MRContext>(options =>
                    options.UseSqlServer(Properties.Settings.Default.ConnString));



        // Register the main form
        services.AddSingleton<MainForm>();


        // 1. Register IEventAggregator
        // Assuming you have a concrete implementation of IEventAggregator, e.g., EventAggregatorImpl
        services.AddSingleton<IEventAggregator, EventAggregator>(); // Replace with your actual implementation

        // 2. Register IWebAutomationService
        services.AddSingleton<IWebAutomationService, WebAutomationService>();
        // 3. Register IDownloaderModule
        services.AddSingleton<IUrlDownloader, UrlDownloader>();

        // 4. Register BlogScraper (it depends on the above)
        services.AddSingleton<IBlogScraper, BlogScraper>();



        // Note:  If you need to access configuration (e.g., from appsettings.json),
        // you'll need to inject IConfiguration into this method and use it to
        // configure your services (e.g., for options).
    }
}

