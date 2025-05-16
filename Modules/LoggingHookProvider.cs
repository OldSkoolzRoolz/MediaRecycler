// Project Name: MediaRecycler
// Author:  Kyle Crowder [InvalidReference]
// **** Distributed under Open Source License ***
// ***   Do not remove file headers ***




using System.Collections.Concurrent;
using System.Runtime.Versioning;

using MediaRecycler;

using Microsoft.Extensions.Logging;

namespace KC.DropIns.Logging;

/// <summary>
///     LoggingHook is a module that implements the ILoggerProvider interface and
///     is designed to seamlessly integrate with the logging system to
///     capture all messages sent through the ILogger system.
///     There is no need to call CreateLogger() as it is handled internally.
///     The output is sent to the configured object such as a UI control.
///     TODO: expose configuration interface for public consumption.
/// </summary>
[UnsupportedOSPlatform("browser")]
[ProviderAlias("LoggingHook")]
public sealed class LoggingHookProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, LoggingHookLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);


    public readonly IDisposable? _onChangeToken;
    private readonly LoggingHookLoggerConfiguration _currentConfig;
    private bool _disposedValue;







    public LoggingHookProvider(LoggingHookLoggerConfiguration config)
    {
        _currentConfig = config;
    }







    // This class is intended as a hook into the logging system.
    // If it's not meant to provide a specific logging implementation yet,
    // returning NullLogger.Instance is a safe way to make it a no-op provider.
    // Otherwise, replace this with the actual logger creation logic.
    public ILogger CreateLogger(string categoryName)
    {
        // Example of a no-op implementation:
        //  return NullLogger.Instance;

        //  Debugger.Log("LoggingHook.CreateLogger needs to be implemented to return a functional ILogger.");
        return _loggers.GetOrAdd(categoryName, name => new LoggingHookLogger(name, LogLevel.Trace));


    }







    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~LoggingHook()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }


    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~LoggingHook()
    // {
    //     Dispose(disposing: false);
    // }







    public void Dispose()
    {
        // TODO release managed resources here
        _loggers.Clear();
        _onChangeToken?.Dispose();
        GC.SuppressFinalize(this);
    }







    private LoggingHookLoggerConfiguration GetCurrentConfig()
    {
        return _currentConfig;
    }
}
