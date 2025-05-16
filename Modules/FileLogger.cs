// Project Name: MediaRecycler
// Author:  Kyle Crowder [InvalidReference]
// **** Distributed under Open Source License ***
// ***   Do not remove file headers ***




using Microsoft.Extensions.Logging;

namespace MediaRecycler.Modules;

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







    public IDisposable BeginScope<TState>(TState state)
    {
        return new FileLoggerScope(state!);
    }







    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _minLogLevel;
    }







    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        // Note: For high-volume logging, File.AppendAllText can be inefficient due to repeated
        // file open/close operations. Consider a buffered writer or a dedicated logging thread
        // with a queue for performance-critical scenarios.

        var messageBuilder = new System.Text.StringBuilder();
        // The formatter delegate is Func<TState, Exception?, string>, so it's designed to handle a null exception.
        // Pass the original exception (which can be null) directly.
        messageBuilder.Append($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_categoryName}: {formatter(state, exception)}");

        if (exception != null)
        {
            messageBuilder.AppendLine().Append(exception); // Appends exception details including stack trace
        }

        try
        {
            var logDirectory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            File.AppendAllText(_filePath, messageBuilder.ToString() + Environment.NewLine);
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
        private static readonly AsyncLocal<Stack<FileLoggerScope>> _currentScopes = new();







        public FileLoggerScope(object state)
        {
            State = state; // State can be null as per ILogger.BeginScope contract
            Parent = Current;
            if (_currentScopes.Value == null)
            {
                _currentScopes.Value = new Stack<FileLoggerScope>();
            }

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

        public static FileLoggerScope? Current
        {
            get
            {
                var stack = _currentScopes.Value;
                return stack != null && stack.Count > 0 ? stack.Peek() : null;
            }
        }







        public void Dispose()
        {
            var stack = _currentScopes.Value;
            if (stack != null && stack.Count > 0)
            {
                stack.Pop();
            }
        }
    }
}
