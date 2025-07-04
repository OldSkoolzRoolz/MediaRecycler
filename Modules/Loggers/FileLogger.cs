// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using System.Text;

using Microsoft.Extensions.Logging;



namespace MediaRecycler.Modules.Loggers;


public class FileLogger : ILogger
{

    private readonly string _categoryName;
    private readonly string _filePath;
    private readonly LogLevel _minLogLevel;






    public FileLogger(string categoryName, string filePath, LogLevel minLogLevel)
    {
        _categoryName = categoryName;
        _filePath = filePath;
        _minLogLevel = minLogLevel;
    }






    IDisposable ILogger.BeginScope<TState>(TState state)
    {
        return new FileLoggerScope(state!);
    }






    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _minLogLevel;
    }






    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        // Note: For high-volume logging, File.AppendAllText can be inefficient due to repeated
        // file open/close operations. Consider a buffered writer or a dedicated logging thread
        // with a queue for performance-critical scenarios.

        StringBuilder messageBuilder = new();

        // The formatter delegate is Func<TState, Exception?, string>, so it's designed to handle a null exception.
        // Pass the original exception (which can be null) directly.
#pragma warning disable CS8604 // Possible null reference argument.
        _ = messageBuilder.Append($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_categoryName}: {formatter(state, exception)}");
#pragma warning restore CS8604 // Possible null reference argument.

        if (exception != null)
        {
            _ = messageBuilder.AppendLine().Append(exception); // Appends exception details including stack trace
        }

        try
        {
            string? logDirectory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                _ = Directory.CreateDirectory(logDirectory);
            }

            File.AppendAllText(_filePath, messageBuilder + Environment.NewLine);
        }
        catch (Exception ex)
        {
            // Fallback logging if the primary file logger fails.
            // This helps in diagnosing issues with the logger itself.
            System.Diagnostics.Debug.WriteLine($"Error writing to log file '{_filePath}': {ex}");
        }
    }






    private class FileLoggerScope : IDisposable
    {



        public FileLoggerScope(object state)
        {
            State = state; // State can be null as per ILogger.BeginScope contract
            Parent = Current;

            _currentScopes.Value ??= new Stack<FileLoggerScope>();

            _currentScopes.Value.Push(this);
        }






        public object State
        {
            get;
        }

        public FileLoggerScope? Parent
        {
            get;
        }






        public void Dispose()
        {
            var stack = _currentScopes.Value;

            if (stack != null && stack.Count > 0)
            {
                _ = stack.Pop();
            }
        }






        private static readonly AsyncLocal<Stack<FileLoggerScope>> _currentScopes = new();

        public static FileLoggerScope? Current
        {
            get
            {
                var stack = _currentScopes.Value;
                return stack != null && stack.Count > 0 ? stack.Peek() : null;
            }
        }

    }

}
