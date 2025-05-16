// Project Name: MediaRecycler
// Author:  Kyle Crowder [InvalidReference]
// **** Distributed under Open Source License ***
// ***   Do not remove file headers ***




using Microsoft.Extensions.Logging;

namespace MediaRecycler;

public sealed class LoggingHookLogger : ILogger
{
    private readonly string _categoryName;
    private readonly LogLevel _minLogLevel;







    public LoggingHookLogger(string categoryName, LogLevel minLogLevel)
    {
        _categoryName = categoryName;
        _minLogLevel = minLogLevel;
    }







    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _minLogLevel;
    }







    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }







    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        throw new NotImplementedException();
    }
}
