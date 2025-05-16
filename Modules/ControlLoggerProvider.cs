using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace MediaRecycler.Modules
{
    public class ControlLoggerProvider : ILoggerProvider
    {
        private readonly Control _control;
        private readonly LogLevel _minLogLevel;
        private readonly ConcurrentDictionary<string, ControlLogger> _loggers = new();

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
        private readonly Control _control;
        private readonly LogLevel _minLogLevel;
        private readonly string _categoryName;

        public ControlLogger(Control control, LogLevel minLogLevel, string categoryName)
        {
            _control = control;
            _minLogLevel = minLogLevel;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            if (formatter == null) return;

            string message = $"{DateTime.Now:HH:mm:ss} [{logLevel}] {_categoryName}: {formatter(state, exception)}";
            if (exception != null)
                message += Environment.NewLine + exception;

            if (_control.InvokeRequired)
            {
                _control.Invoke(new Action(() => AppendText(message)));
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
    }
}
