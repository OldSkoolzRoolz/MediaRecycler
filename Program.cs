#region Header

// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers

#endregion



// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"



using MediaRecycler.Modules;
using MediaRecycler.Modules.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;



namespace MediaRecycler;


internal static class Program
{

    public static IServiceProvider serviceProvider { get; private set; } = null!;
    public static ILogger Logger { get; private set; } = null!;







    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.SetCompatibleTextRenderingDefault(false);

        var builder = Host.CreateApplicationBuilder();

        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                true)
            .AddEnvironmentVariables();

        builder.Services.Configure<Scraping>(builder.Configuration.GetSection(nameof(Scraping)));
        builder.Services.Configure<HeadlessBrowserOptions>(
            builder.Configuration.GetSection(nameof(HeadlessBrowserOptions)));
        builder.Services.Configure<MiniFrontierSettings>(
            builder.Configuration.GetSection(nameof(MiniFrontierSettings)));
        builder.Services.Configure<DownloaderOptions>(builder.Configuration.GetSection(nameof(DownloaderOptions)));


        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
        builder.Services.AddLogging(logBuilder =>
        {
            logBuilder.AddConsole();
            logBuilder.AddDebug();
            logBuilder.AddProvider(new FileLoggerProvider("app.log", LogLevel.Trace));
        });

        serviceProvider = builder.Services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();

        Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("MediaRecycler");

        MainForm mainform = new(
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<Scraping>>(),
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<HeadlessBrowserOptions>>(),
            scope.ServiceProvider.GetRequiredService<IOptionsMonitor<DownloaderOptions>>(),
            Logger);

        Logger.LogDebug("MainForm created successfully.");

        var factory = serviceProvider.GetRequiredService<ILoggerFactory>();
        factory.AddProvider(new ControlLoggerProvider(mainform.MainLogRichTextBox, LogLevel.Trace));

        Logger.LogDebug("ControlLoggerProvider added successfully.");

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();


        Application.Run(mainform);

    }

}
