// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using MediaRecycler.Modules;

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
        _ = Application.SetHighDpiMode(HighDpiMode.SystemAware);
        var configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", false, true).AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true).AddEnvironmentVariables().Build();

        var services = new ServiceCollection();

        _ = services.AddLogging(logBuilder =>
        {
            _ = logBuilder.AddConsole();
            _ = logBuilder.AddDebug();
            _ = logBuilder.AddProvider(new FileLoggerProvider("logs/app.log", LogLevel.Trace));
        });
        _ = services.AddTransient<MainForm>();

        /*
             services.AddLogging(serviceProvider =>
             {
                 var resolvedMainForm = serviceProvider.GetRequiredService<MainForm>();
                 return new ControlLoggerProvider(resolvedMainForm.MainLogRichTextBox, LogLevel.Trace));
             });
        */

        // Build the service provider
        using var serviceProvider = services.BuildServiceProvider();


        // Resolve and run the main form
        var mainFormInstance = serviceProvider.GetRequiredService<MainForm>();

        var factory = serviceProvider.GetRequiredService<ILoggerFactory>();

        factory.AddProvider(new ControlLoggerProvider(mainFormInstance.MainLogRichTextBox, LogLevel.Trace));

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
