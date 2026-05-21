namespace Flow.Models;

/// <summary>
/// A unified search result that can represent either an application or a file.
/// Used as the common item type for the main search list in the UI.
/// </summary>
public class SearchResult
{
    /// <summary>
    /// Unique identifier for this result, derived from the type and path.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// The kind of result: "app" or "file".
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// The primary display name shown in the results list.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Secondary text displayed below the name (e.g. executable path or folder).
    /// </summary>
    public string Subtitle { get; set; } = "";

    /// <summary>
    /// Path or resource identifier for the icon to display alongside the result.
    /// </summary>
    public string Icon { get; set; } = "";

    /// <summary>
    /// Combined relevance score from fuzzy matching and frequency ranking.
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// The frequency-based component of the relevance score.
    /// </summary>
    public double FrequencyScore { get; set; }

    // ---- App-specific fields ----

    /// <summary>
    /// For app results, the path to the target executable.
    /// </summary>
    public string? ExecutablePath { get; set; }

    /// <summary>
    /// For app results, the path to the .lnk shortcut file.
    /// </summary>
    public string? ShortcutPath { get; set; }

    // ---- File-specific fields ----

    /// <summary>
    /// For file results, the absolute path to the file.
    /// </summary>
    public string? FullPath { get; set; }

    /// <summary>
    /// For file results, the file extension including the leading dot.
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// For file results, a human-readable last-modified timestamp.
    /// </summary>
    public string? LastModified { get; set; }

    /// <summary>
    /// For file results, the file size in bytes.
    /// </summary>
    public long Size { get; set; }
}
