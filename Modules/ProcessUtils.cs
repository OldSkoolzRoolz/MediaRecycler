// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers



using System.ComponentModel;
using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;



namespace MediaRecycler.Modules;


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
        int killedCount = 0;

        if (string.IsNullOrWhiteSpace(processName))
        {
            logger.LogError("Process name cannot be empty or whitespace.");
            return 0;
        }

        logger.LogInformation("Attempting to find and kill processes named '{ProcessName}'...", processName);

        // Get all processes and filter manually for case-insensitivity and robustness
        var processesToKill = Process.GetProcesses().Where(p => p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));

        foreach (var process in processesToKill)
        {
            try
            {
                logger.LogDebug("Attempting to kill process ID: {ProcessId}, Name: {ProcessName}", process.Id, process.ProcessName);
                process.Kill(true); // Attempt to kill the entire process tree
                _ = process.WaitForExit(5000); // Wait briefly for the process to exit
                logger.LogInformation("Successfully killed process ID: {ProcessId}", process.Id);
                killedCount++;
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or NotSupportedException)
            {
                logger.LogWarning(ex, "Failed to kill process ID: {ProcessId}. It might have already exited or access was denied.", process.Id);
            }
            finally { process.Dispose(); } // Release process resources
        }

        logger.LogInformation("Finished killing processes. Terminated {KilledCount} processes named '{ProcessName}'.", killedCount, processName);
        return killedCount;
    }











    public static void KillPreviousWebBrowserProcesses()
    {
        var matchingProcesses =
                    System.Diagnostics.Process.GetProcesses()
                                /*
                                2020-02-17
                                .Where(process => process.StartInfo.Arguments.Contains(UserDataDirPath(), StringComparison.InvariantCultureIgnoreCase))
                                */
                                .Where(ProcessIsWebBrowser)
                                .ToList();

        foreach (var process in matchingProcesses)
        {
            if (process.HasExited)
            {
                continue;
            }

            process.Kill();
        }
    }

    public static bool ProcessIsWebBrowser(System.Diagnostics.Process process)
    {
        try
        {
            return process.MainModule.FileName.Contains(".local-chromium");
        }
        catch
        {
            return false;
        }
    }



    //Terminate all running processes with the name of chrome.exe and iexplorer.exe
    public static void TerminateBrowserProcesses()
    {
        try
        {
            string[] processesToTerminate = new[] { "chrome", "msedge", "webview2" };

            foreach (string? processName in processesToTerminate)
            {
                var processes = Process.GetProcessesByName(processName);

                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        Program.Logger?.LogInformation($"Terminated process: {process.ProcessName} (ID: {process.Id})");
                    }
                    catch (Exception ex)
                    {
                        Program.Logger?.LogWarning($"Failed to terminate process: {process.ProcessName} (ID: {process.Id}). Error: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Program.Logger?.LogError($"An error occurred while terminating browser processes: {ex.Message}");
        }
    }








}
