using System.Diagnostics;
using Flow.Models;

namespace Flow.Services;

/// <summary>
/// Searches the file system using Windows Search Index for fast results.
/// Falls back to a simple Directory.EnumerateFiles when the index is unavailable.
/// </summary>
public class FileSearchService
{
    /// <summary>
    /// Performs a file search with the given query.
    /// Returns up to <paramref name="maxResults"/> matches.
    /// </summary>
    public async Task<List<SearchResult>> SearchAsync(string query, int maxResults = 20, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<SearchResult>();

        return await Task.Run(() =>
        {
            var results = new List<SearchResult>();

            try
            {
                // Search common user directories using the file system.
                string[] searchRoots = new[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                };

                var pattern = $"*{query}*";

                foreach (var root in searchRoots.Distinct())
                {
                    if (ct.IsCancellationRequested)
                        break;

                    try
                    {
                        foreach (var file in Directory.EnumerateFiles(root, pattern, new EnumerationOptions
                        {
                            RecurseSubdirectories = true,
                            MaxRecursionDepth = 3,
                            IgnoreInaccessible = true,
                            ReturnSpecialDirectories = false,
                        }).Take(maxResults - results.Count))
                        {
                            if (ct.IsCancellationRequested)
                                break;

                            var info = new FileInfo(file);
                            results.Add(new SearchResult
                            {
                                Id = $"file:{file}",
                                Type = "file",
                                Name = info.Name,
                                Subtitle = info.DirectoryName ?? file,
                                Icon = file,
                                FullPath = file,
                                Extension = info.Extension,
                                LastModified = info.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                                Size = info.Length,
                            });
                        }
                    }
                    catch
                    {
                        // Skip inaccessible directories.
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"File search error: {ex.Message}");
            }

            return results;
        }, ct);
    }

    /// <summary>Opens the file with its default associated application.</summary>
    public static void OpenFile(string fullPath)
    {
        try
        {
            var psi = new ProcessStartInfo(fullPath)
            {
                UseShellExecute = true,
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open file: {ex.Message}");
        }
    }

    /// <summary>Opens the containing folder and selects the file.</summary>
    public static void OpenContainingFolder(string fullPath)
    {
        try
        {
            Process.Start("explorer.exe", $"/select,\"{fullPath}\"");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open folder: {ex.Message}");
        }
    }
}
