using System.Windows;
using Flow.Models;

namespace Flow.Services;

/// <summary>
/// Maintains an in-memory clipboard text history.
/// Never persists data to disk. Deduplicates consecutive identical entries.
/// </summary>
public static class ClipboardService
{
    private static readonly List<ClipboardItem> History = [];
    private const int MaxItems = 20;

    /// <summary>
    /// Returns a snapshot of the current clipboard history, newest first.
    /// </summary>
    public static IReadOnlyList<ClipboardItem> GetHistory()
    {
        lock (History)
        {
            return History.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Adds a text item to the clipboard history.
    /// Consecutive duplicates are silently ignored.
    /// When the history exceeds <see cref="MaxItems"/> entries, the oldest item is removed.
    /// </summary>
    /// <param name="content">The text content that was copied.</param>
    public static void AddItem(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        lock (History)
        {
            // Deduplicate consecutive identical entries
            if (History.Count > 0 &&
                string.Equals(History[0].Content, content, StringComparison.Ordinal))
            {
                return;
            }

            History.Insert(0, new ClipboardItem
            {
                Content = content,
                Timestamp = DateTime.Now
            });

            // Trim to max size
            while (History.Count > MaxItems)
            {
                History.RemoveAt(History.Count - 1);
            }
        }
    }

    /// <summary>
    /// Copies the given text to the system clipboard.
    /// </summary>
    public static void CopyToClipboard(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        try
        {
            Clipboard.SetText(text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to copy to clipboard: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads the current text content from the system clipboard.
    /// </summary>
    public static string? GetClipboardText()
    {
        try
        {
            if (Clipboard.ContainsText())
                return Clipboard.GetText();
        }
        catch
        {
            // Clipboard may be locked by another process
        }
        return null;
    }
}
