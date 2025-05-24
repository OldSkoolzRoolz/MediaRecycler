// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"


// Required for Win32Exception

// For NullLogger




using System.ComponentModel;
using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MediaRecycler.Modules;
// Using a sub-namespace for utilities

/// <summary>
///     Provides utility methods for process management.
/// </summary>
public static class ProcessUtils
{
    /// <summary>
    ///     Finds and attempts to terminate all running processes with the specified name (case-insensitive).
    /// </summary>
    /// <param name="processName">The name of the process to kill (e.g., "chrome"). Do not include the extension.</param>
    /// <param name="logger">Optional logger instance for recording actions.</param>
    /// <returns>The number of processes successfully terminated.</returns>
    public static int KillProcessesByName(string processName, ILogger? logger = null)
    {
        logger ??= NullLogger.Instance; // Use NullLogger if none provided
        var killedCount = 0;

        if (string.IsNullOrWhiteSpace(processName))
        {
            logger.LogError("Process name cannot be empty or whitespace.");
            return 0;
        }

        logger.LogInformation("Attempting to find and kill processes named '{ProcessName}'...", processName);

        // Get all processes and filter manually for case-insensitivity and robustness
        var processesToKill = Process.GetProcesses()
            .Where(p => p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));

        foreach (var process in processesToKill)
        {
            try
            {
                logger.LogDebug("Attempting to kill process ID: {ProcessId}, Name: {ProcessName}", process.Id,
                    process.ProcessName);
                process.Kill(true); // Attempt to kill the entire process tree
                process.WaitForExit(5000); // Wait briefly for the process to exit
                logger.LogInformation("Successfully killed process ID: {ProcessId}", process.Id);
                killedCount++;
            }
            catch (Exception ex) when (ex is Win32Exception || ex is InvalidOperationException ||
                                       ex is NotSupportedException)
            {
                logger.LogWarning(ex,
                    "Failed to kill process ID: {ProcessId}. It might have already exited or access was denied.",
                    process.Id);
            }
            finally { process.Dispose(); } // Release process resources
        }

        logger.LogInformation("Finished killing processes. Terminated {KilledCount} processes named '{ProcessName}'.",
            killedCount, processName);
        return killedCount;
    }
}
