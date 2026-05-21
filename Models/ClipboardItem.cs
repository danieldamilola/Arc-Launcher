namespace Flow.Models;

/// <summary>
/// Represents a single clipboard history entry.
/// </summary>
public class ClipboardItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..12];
    public string Content { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.Now;

    /// <summary>
    /// Truncated content for display (max 120 characters).
    /// </summary>
    public string TruncatedContent =>
        Content.Length <= 120
            ? Content.Replace("\r", "").Replace("\n", " ↵ ")
            : Content[..120].Replace("\r", "").Replace("\n", " ↵ ") + "…";

    /// <summary>
    /// Human-friendly timestamp string.
    /// </summary>
    public string TimeAgo => Timestamp switch
    {
        _ when Timestamp > DateTime.Now.AddMinutes(-1) => "Just now",
        _ when Timestamp > DateTime.Now.AddHours(-1) => $"{(int)(DateTime.Now - Timestamp).TotalMinutes}m ago",
        _ when Timestamp > DateTime.Now.AddDays(-1) => $"{(int)(DateTime.Now - Timestamp).TotalHours}h ago",
        _ => Timestamp.ToString("MMM dd, HH:mm")
    };

    /// <summary>
    /// First line or first 60 chars, used as a title preview.
    /// </summary>
    public string Preview
    {
        get
        {
            var firstLine = Content.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? Content;
            return firstLine.Length <= 60 ? firstLine : firstLine[..60] + "…";
        }
    }
}
