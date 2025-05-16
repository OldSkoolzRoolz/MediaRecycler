// Project Name: MediaRecycler
// Author:  Kyle Crowder [InvalidReference]
// **** Distributed under Open Source License ***
// ***   Do not remove file headers ***




using Microsoft.Extensions.Logging;

public sealed class LoggingHookLoggerConfiguration
{
    public int EventId { get; set; }


    public string LogFilePath { get; set; } = string.Empty;
    public LogLevel MinLogLevel { get; set; } = LogLevel.Trace;
    public bool IncludeScopes { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
}
