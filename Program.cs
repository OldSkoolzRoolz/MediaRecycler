// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using MediaRecycler.Modules;
using MediaRecycler.Modules.Loggers;

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
        ProcessUtils.TerminateBrowserProcesses();

        Application.SetCompatibleTextRenderingDefault(false);
        Application.EnableVisualStyles();
        _ = Application.SetHighDpiMode(HighDpiMode.SystemAware);
        var configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddEnvironmentVariables().Build();

        var services = new ServiceCollection();

        _ = services.AddLogging(logBuilder =>
        {
            _ = logBuilder.AddConsole();
            _ = logBuilder.AddDebug();
            _ = logBuilder.AddProvider(new FileLoggerProvider("logs/app.log", LogLevel.Trace));
            logBuilder.SetMinimumLevel(LogLevel.Trace);
        });
        _ = services.AddTransient<MainForm>();

        /*
             services.AddLogging(serviceProvider =>
             {
                 var resolvedMainForm = serviceProvider.GetRequiredService<MainForm>();
                 return new ControlLoggerProvider(resolvedMainForm.MainLogRichTextBox, LogLevel.Trace));
             });
        */


        Properties.Settings.Default.MyDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        Properties.Settings.Default.Save();





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

//TODO: Create helper to centralize logging for all types implemented in the application

//TODO: Streamline error handling and logging across the application, ensuring consistent error messages and logging levels.

