// Project Name: MediaRecycler
// Author:  Kyle Crowder [InvalidReference]
// **** Distributed under Open Source License ***
// ***   Do not remove file headers ***




using MediaRecycler.Modules;

using Microsoft.Extensions.Logging;

namespace MediaRecycler;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {

        // LoggerFactory implements IDisposable. Using a 'using' statement ensures it's
        // properly disposed of when the application exits, which in turn disposes
        // registered providers like FileLoggerProvider.
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            // Configure FileLoggerProvider to log Information level and above to "app.log"
            // Debug and Trace messages will not be written to this provider.
            builder.AddProvider(new FileLoggerProvider("app.log", LogLevel.Information));

        });
        var form = new Form1();
        ApplicationConfiguration.Initialize();
        Application.Run(form);

        loggerFactory.AddProvider(new ControlLoggerProvider(form.MainLogRichTextBox, LogLevel.Trace));

        loggerFactory.Dispose(); // is called here
    }
}
