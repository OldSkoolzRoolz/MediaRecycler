// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace MediaRecycler.Modules;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
    private readonly LogLevel _minLogLevel;







    public FileLoggerProvider(string filePath, LogLevel minLogLevel)
    {
        _filePath = filePath;
        _minLogLevel = minLogLevel;
    }







    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _filePath, _minLogLevel));
    }







    public void Dispose()
    {
        // No resources to dispose currently.
    }
}
