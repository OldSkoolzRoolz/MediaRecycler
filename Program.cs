// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using MediaRecycler.Modules;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;



namespace MediaRecycler;


internal static class Program
{

    public static ILogger Logger { get; private set; }







    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.SetCompatibleTextRenderingDefault(false);
        Application.EnableVisualStyles();
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile(
                                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                                true)
                    .AddEnvironmentVariables()
                    .Build();

        var services = new ServiceCollection();
        services.Configure<Scraping>(configuration.GetSection("Scraping"));
        services.Configure<HeadlessBrowserOptions>(configuration.GetSection(nameof(HeadlessBrowserOptions)));
        services.Configure<MiniFrontierSettings>(configuration.GetSection(nameof(MiniFrontierSettings)));
        services.Configure<DownloaderOptions>(configuration.GetSection(nameof(DownloaderOptions)));
        services.AddLogging(logBuilder =>
        {
            logBuilder.AddConsole();
            logBuilder.AddDebug();
            logBuilder.AddProvider(new FileLoggerProvider("logs/app.log", LogLevel.Trace));
        });
        services.AddTransient<MainForm>();

        /*     services.AddSingleton<ILoggerProvider>(serviceProvider =>
             {
                 var resolvedMainForm = serviceProvider.GetRequiredService<MainForm>();
                 return new ControlLoggerProvider(resolvedMainForm.MainLogRichTextBox, LogLevel.Trace);
             });
        */

        // Build the service provider
        using var serviceProvider = services.BuildServiceProvider();

        // Resolve and run the main form
        var mainFormInstance = serviceProvider.GetRequiredService<MainForm>();

        try
        {
            Logger = serviceProvider.GetRequiredService<ILogger<MainForm>>();
            Logger.LogInformation("Application Starting");
            Application.Run(mainFormInstance);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

}
