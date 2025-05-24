// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using MediaRecycler.Modules;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace MediaRecycler;

internal static class Program
{

    public static IServiceProvider serviceProvider { get; private set; } = null!;

    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static async Task Main()
    {
        var b2 = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var builder = Host.CreateApplicationBuilder();
        
           builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var config = builder.Build();
        


        
        builder.Services.AddTransient<MainForm>();
        builder.Services.Configure<LauncherSettings>(builder.Configuration.GetSection(nameof(LauncherSettings)));
        builder.Services.Configure<LauncherSettings>(builder.Configuration.GetSection(nameof(LauncherSettings)));
        builder.Services.Configure<ScraperSettings>(builder.Configuration.GetSection(nameof(ScraperSettings)));
        builder.Services.Configure<DownloaderSettings>(builder.Configuration.GetSection(nameof(DownloaderSettings)));
      //  builder.Services.Configure<MiniFrontierSettings>(builder.Configuration.GetSection(nameof(MiniFrontierSettings)));
        builder.Services.AddLogging(logbuilder =>
        {
            logbuilder.AddConsole();
            logbuilder.AddDebug();
            logbuilder.AddProvider(new ControlLoggerProvider(new RichTextBox(), LogLevel.Trace));
            logbuilder.AddProvider(new FileLoggerProvider("app.log", LogLevel.Trace));
        });
        
        var host = builder.Build();
        using var scope = host.Services.CreateScope();
        
        var mainForm = scope.ServiceProvider.GetRequiredService<MainForm>();

        ApplicationConfiguration.Initialize();
        Application.Run(mainForm);



    }
}
