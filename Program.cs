// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using MediaRecycler.Modules;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



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
        _ = Application.SetHighDpiMode(HighDpiMode.SystemAware);
        var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile(
                                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                                true)
                    .AddEnvironmentVariables()
                    .Build();

        var services = new ServiceCollection();

        // services.Configure<Scraping>(configuration.GetSection("Scraping"));
        _ = services.Configure<HeadlessBrowserOptions>(configuration.GetSection(nameof(HeadlessBrowserOptions)));
        _ = services.Configure<MiniFrontierSettings>(configuration.GetSection(nameof(MiniFrontierSettings)));
        _ = services.Configure<DownloaderOptions>(configuration.GetSection(nameof(DownloaderOptions)));
        _ = services.AddLogging(logBuilder =>
        {
            _ = logBuilder.AddConsole();
            _ = logBuilder.AddDebug();
            _ = logBuilder.AddProvider(new FileLoggerProvider("logs/app.log", LogLevel.Trace));
        });
        _ = services.AddTransient<MainForm>();
        _ = services.AddSingleton(provider => Scraping.Default);
        _ = services.AddSingleton<IOptionsMonitor<Scraping>>(provider =>
                    new OptionsMonitorStub<Scraping>(Scraping.Default));


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
            _ = MessageBox.Show(ex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

}
