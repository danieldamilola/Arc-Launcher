namespace Flow.Models;

/// <summary>
/// Represents a discovered application with its display name, icon, and launch targets.
/// </summary>
public class AppInfo
{
    /// <summary>
    /// The display name of the application.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The file system path to the application's icon.
    /// </summary>
    public string IconPath { get; set; } = "";

    /// <summary>
    /// The full path to the target executable.
    /// </summary>
    public string ExecutablePath { get; set; } = "";

    /// <summary>
    /// The full path to the .lnk shortcut file that was parsed.
    /// </summary>
    public string ShortcutPath { get; set; } = "";
}
