using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace StarResonanceDpsAnalysis.WPF.Helpers;

/// <summary>
/// Helper class for checking and requesting administrator privileges
/// </summary>
public static class ElevationHelper
{
    /// <summary>
    /// Check if the current process is running with administrator privileges
    /// </summary>
    public static bool IsRunningAsAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Restart the application with administrator privileges
    /// </summary>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>True if restart was initiated, false otherwise</returns>
    public static bool RestartAsAdministrator(ILogger? logger = null)
    {
        if (IsRunningAsAdministrator())
        {
            logger?.LogWarning("Already running as administrator");
            return false;
        }

        try
        {
            var processInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName,
                Verb = "runas" // This triggers UAC elevation
            };

            // Pass command line arguments to the new process
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                processInfo.Arguments = string.Join(" ", args[1..]);
            }

            logger?.LogInformation("Attempting to restart as administrator");
            Process.Start(processInfo);

            // Shutdown current instance
            Application.Current?.Shutdown();
            return true;
        }
        catch (Exception ex)
        {
            // User clicked "No" on UAC prompt or other error
            logger?.LogWarning(ex, "Failed to restart as administrator - user may have cancelled");
            return false;
        }
    }

    /// <summary>
    /// Show a dialog asking user if they want to restart with admin privileges
    /// </summary>
    /// <param name="message">Custom message to display</param>
    /// <param name="logger">Optional logger</param>
    /// <returns>True if user accepted and restart was initiated</returns>
    public static bool PromptAndRestartAsAdministrator(string? message = null, ILogger? logger = null)
    {
        if (IsRunningAsAdministrator())
        {
            return false;
        }

        var defaultMessage = "This operation requires administrator privileges.\n\n" +
                           "Would you like to restart the application as administrator?";
        
        var result = MessageBox.Show(
            message ?? defaultMessage,
            "Administrator Privileges Required",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            return RestartAsAdministrator(logger);
        }

        return false;
    }

    /// <summary>
    /// Get a display string indicating the current privilege level
    /// </summary>
    public static string GetPrivilegeLevelText()
    {
        return IsRunningAsAdministrator() ? "Administrator" : "Standard User";
    }
}
