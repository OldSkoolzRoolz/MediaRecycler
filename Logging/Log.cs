// Project Name: MediaRecycler
// File Name: Log.cs
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using System.Diagnostics;

using Microsoft.Extensions.Logging;



namespace MediaRecycler.Logging;


/// <summary>
///     Provides logging functionality for the application, including methods for logging messages
///     at various levels and formatting log output. This class serves as a centralized logging utility
///     to standardize and simplify logging operations throughout the application.
///     Formatter does handle named or indexed parameter values but does not handle literal braces.
/// </summary>
public static class Log
{

    private static readonly object _lock = new(); // Ensures thread safety






// Helper method to check for named placeholders
//TODO: DOES NOT take into account for literal braces
    private static bool ContainsNamedPlaceholders(string message)
    {
        return message.Contains("{") && message.Contains("}") && !message.Contains("{0}");
    }






// Helper method to count numerical placeholders
    private static int CountNumericalPlaceholders(string message)
    {
        int count = 0;
        for (int i = 0; i < message.Length - 1; i++)
            if (message[i] == '{' && char.IsDigit(message[i + 1]))
                count++;

        return count;
    }






    /// <summary>
    ///     Formats a log message by combining the log level, message, exception details, and arguments into a single string.
    /// </summary>
    /// <param name="level">The severity level of the log message.</param>
    /// <param name="message">The log message template, which may contain placeholders for arguments.</param>
    /// <param name="exception">An optional exception to include in the log message.</param>
    /// <param name="args">An array of arguments to replace placeholders in the message template.</param>
    /// <returns>A formatted string containing the timestamp, log level, message, and exception details.</returns>
    /// <remarks>
    ///     This method ensures thread safety using a lock and handles both named and numerical placeholders in the message
    ///     template.
    ///     If formatting fails, an error message is included in the output.
    /// </remarks>
    private static string FormatMessage(LogLevel level, string? message, Exception? exception, params object?[] args)
    {
        lock (_lock)
        {
            string timestamp = DateTime.UtcNow.ToString("o");
            string levelString = level.ToString().ToUpper();

            // Safely format the message
            string formattedMessage;

            if (!string.IsNullOrEmpty(message) && args.Length > 0)
                try
                {
                    // Check if the message contains named placeholders (e.g., {name})
                    if (ContainsNamedPlaceholders(message))
                    {
                        // Convert args to a dictionary if named placeholders are detected
                        var namedArgs = args.OfType<KeyValuePair<string, object>>().ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? "null");
                        formattedMessage = FormatWithNamedPlaceholders(message, namedArgs);
                    }
                    else
                    {
                        // Convert all arguments to strings for numerical placeholders
                        string[] stringArgs = args.Select(arg => arg?.ToString() ?? "null").ToArray();

                        // Validate placeholders and argument count
                        int placeholderCount = CountNumericalPlaceholders(message);
                        if (placeholderCount != stringArgs.Length)
                            throw new FormatException($"Placeholder count ({placeholderCount}) does not match argument count ({stringArgs.Length}).");
                        formattedMessage = string.Format(message, stringArgs);
                    }
                }
                catch (FormatException ex)
                {
                    formattedMessage = $"[Formatting Error: {ex.Message}] {message}";
                }
            else
                formattedMessage = message ?? string.Empty;

            string exceptionDetails = exception?.ToString() ?? string.Empty;
            return $"{timestamp} [{levelString}] {formattedMessage} {exceptionDetails}".Trim();
        }
    }






// Helper method to format strings with named placeholders
    private static string FormatWithNamedPlaceholders(string message, Dictionary<string, string> namedArgs)
    {
        foreach (var kvp in namedArgs) message = message.Replace($"{{{kvp.Key}}}", kvp.Value);
        return message;
    }






    public static void LogCritical(string message, params object?[] args)
    {


        LogMessage(LogLevel.Critical, null, message, args);
    }






    public static void LogCritical(Exception exception, string? message, params object?[] args)
    {
        LogMessage(LogLevel.Critical, exception, message, args);
    }






    public static void LogDebug(string message, params object[] args)
    {
        LogMessage(LogLevel.Debug, null, message, args);
    }






    public static void LogDebug(Exception? exception, string? message, params object?[] args)
    {
        LogMessage(LogLevel.Debug, exception, message, args);
    }






    public static void LogError(string message, params object?[] args)
    {
        LogMessage(LogLevel.Error, null, message, args);
    }






    public static void LogError(Exception exception, string? message, params object?[] args)
    {
        LogMessage(LogLevel.Error, exception, message, args);
    }






    public static LogLevel LoggingLevel { get; set; } = LogLevel.Information;






    public static void LogInformation(string message, params object?[] args)
    {
        LogMessage(LogLevel.Information, null, message, args);

    }






    public static void LogInformation(Exception? exception, string? message, params object?[] args)
    {
        LogMessage(LogLevel.Information, exception, message, args);
    }






    private static void LogMessage(LogLevel level, Exception? exception, string? message, params object?[] args)
    {
        if (!ShouldLog(level)) return;

        try
        {
            string formattedMessage = FormatMessage(level, message, exception, args);
            WriteLog(formattedMessage);
        }
        catch (Exception ex)
        {
            // Handle unexpected errors in logging itself
            Debug.WriteLine($"Logging failed: {ex.Message}");
        }
    }






    public static void LogWarning(string message, params object?[] args)
    {
        LogMessage(LogLevel.Warning, null, message, args);
    }






    public static void LogWarning(Exception? exception, string? message, params object?[] args)
    {
        LogMessage(LogLevel.Warning, exception, message, args);
    }






    private static bool ShouldLog(LogLevel level)
    {
        return level >= LoggingLevel;
    }






    private static void WriteLog(string formattedMessage)
    {
        lock (_lock)
        {
            WriteMessage?.Invoke(formattedMessage);
        }
    }






    public static Action<string>? WriteMessage;

}