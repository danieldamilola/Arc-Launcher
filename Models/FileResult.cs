namespace Flow.Models;

/// <summary>
/// Represents a file discovered during a file system search.
/// </summary>
public class FileResult
{
    /// <summary>
    /// The file name including its extension.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The absolute path to the file.
    /// </summary>
    public string FullPath { get; set; } = "";

    /// <summary>
    /// The file extension including the leading dot (e.g. ".txt").
    /// </summary>
    public string Extension { get; set; } = "";

    /// <summary>
    /// A human-readable string representing the file's last modification time.
    /// </summary>
    public string LastModified { get; set; } = "";

    /// <summary>
    /// The size of the file in bytes.
    /// </summary>
    public long Size { get; set; }
}
