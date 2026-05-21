using System.Diagnostics;

namespace Flow.Services;

/// <summary>
/// Handles launching applications, opening files with their default handler,
/// and revealing items in File Explorer.
/// </summary>
public static class LauncherService
{
    /// <summary>
    /// Launches an application from its .lnk shortcut or .exe path.
    /// </summary>
    /// <param name="path">The path to a .lnk or .exe file.</param>
    /// <returns>True if the process started successfully; otherwise false.</returns>
    public static bool LaunchApp(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            };

            Process.Start(psi);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Flow] Failed to launch app '{path}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Opens a file with its default associated application using shell execution.
    /// </summary>
    /// <param name="path">The absolute path to the file to open.</param>
    /// <returns>True if the file was opened successfully; otherwise false.</returns>
    public static bool OpenFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return false;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            };

            Process.Start(psi);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Flow] Failed to open file '{path}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Opens File Explorer with the specified file or folder selected.
    /// </summary>
    /// <param name="path">The absolute path to the file or folder to highlight in Explorer.</param>
    /// <returns>True if Explorer launched successfully; otherwise false.</returns>
    public static bool OpenContainingFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            // If the path is a directory, open it directly; otherwise select the file
            if (Directory.Exists(path))
            {
                Process.Start("explorer.exe", $"\"{path}\"");
            }
            else
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Flow] Failed to open containing folder for '{path}': {ex.Message}");
            return false;
        }
    }
}
