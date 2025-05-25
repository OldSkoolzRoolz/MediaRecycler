// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;



namespace MediaRecycler.Modules;


public class ControlLoggerProvider : ILoggerProvider
{

    private readonly Control _control;
    private readonly ConcurrentDictionary<string, ControlLogger> _loggers = new();
    private readonly LogLevel _minLogLevel;






    public ControlLoggerProvider(Control control, LogLevel minLogLevel = LogLevel.Information)
    {
        _control = control;
        _minLogLevel = minLogLevel;
    }






    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new ControlLogger(_control, _minLogLevel, name));
    }






    public void Dispose() { }

}


public class ControlLogger : ILogger
{

    private readonly string _categoryName;
    private readonly Control _control;
    private readonly LogLevel _minLogLevel;






    public ControlLogger(Control control, LogLevel minLogLevel, string categoryName)
    {
        _control = control;
        _minLogLevel = minLogLevel;
        _categoryName = categoryName;
    }






    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }






    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _minLogLevel;
    }






    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (formatter == null)
        {
            return;
        }

        string? message = $"{DateTime.Now:HH:mm:ss} [{logLevel}] {_categoryName}: {formatter(state, exception)}";

        if (exception != null)
        {
            message += Environment.NewLine + exception;
        }

        if (_control.InvokeRequired)
        {
            _control.Invoke(() => AppendText(message));
        }
        else
        {
            AppendText(message);
        }
    }






    private void AppendText(string message)
    {
        if (_control is RichTextBox rtb)
        {
            rtb.AppendText(message + Environment.NewLine);
        }
        else if (_control is TextBox tb)
        {
            tb.AppendText(message + Environment.NewLine);
        }
    }






    private class NullScope : IDisposable
    {

        public static readonly NullScope Instance = new();

        private NullScope() { }

        public void Dispose() { }

    }

}
