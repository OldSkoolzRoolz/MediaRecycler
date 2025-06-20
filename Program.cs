// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using System.Diagnostics;

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
        KillPreviousWebBrowserProcesses();


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




    private static void KillPreviousWebBrowserProcesses()
    {
        var matchingProcesses =
                    System.Diagnostics.Process.GetProcesses()
                                /*
                                2020-02-17
                                .Where(process => process.StartInfo.Arguments.Contains(UserDataDirPath(), StringComparison.InvariantCultureIgnoreCase))
                                */
                                .Where(ProcessIsWebBrowser)
                                .ToList();

        foreach (var process in matchingProcesses)
        {
            if (process.HasExited)
            {
                continue;
            }

            process.Kill();
        }
    }

    private static bool ProcessIsWebBrowser(System.Diagnostics.Process process)
    {
        try
        {
            return process.MainModule.FileName.Contains(".local-chromium");
        }
        catch
        {
            return false;
        }
    }



    //Terminate all running processes with the name of chrome.exe and iexplorer.exe
    private static void TerminateBrowserProcesses()
    {
        try
        {
            string[] processesToTerminate = new[] { "chrome", "msedge", "webview2" };

            foreach (string? processName in processesToTerminate)
            {
                var processes = Process.GetProcessesByName(processName);

                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        Logger?.LogInformation($"Terminated process: {process.ProcessName} (ID: {process.Id})");
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogWarning($"Failed to terminate process: {process.ProcessName} (ID: {process.Id}). Error: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError($"An error occurred while terminating browser processes: {ex.Message}");
        }
    }


}

//TODO: Create helper to centralize logging for all types implemented in the application

//TODO: Streamline error handling and logging across the application, ensuring consistent error messages and logging levels.

