using System.Diagnostics;

namespace Flow.Services;

public class AppDiscoveryService
{
    private readonly List<Models.SearchResult> _cachedApps = new();
    public IReadOnlyList<Models.SearchResult> InstalledApps => _cachedApps.AsReadOnly();

    public async Task LoadAppsAsync()
    {
        _cachedApps.Clear();

        string[] startMenuPaths = new[]
        {
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs"),
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs"),
        };

        var tasks = startMenuPaths.Select(ScanDirectoryAsync);
        var results = await Task.WhenAll(tasks);

        foreach (var batch in results)
            _cachedApps.AddRange(batch);

        _cachedApps.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
    }

    private static Task<List<Models.SearchResult>> ScanDirectoryAsync(string directory)
    {
        return Task.Run(() =>
        {
            var results = new List<Models.SearchResult>();

            if (!Directory.Exists(directory))
                return results;

            foreach (var lnk in Directory.EnumerateFiles(directory, "*.lnk", SearchOption.AllDirectories))
            {
                try
                {
                    var targetPath = ResolveShortcutTarget(lnk);

                    results.Add(new Models.SearchResult
                    {
                        Id = $"app:{lnk}",
                        Type = "app",
                        Name = System.IO.Path.GetFileNameWithoutExtension(lnk),
                        Subtitle = targetPath ?? lnk,
                        Icon = lnk,
                        ExecutablePath = targetPath,
                        ShortcutPath = lnk,
                    });
                }
                catch
                {
                    // Skip shortcuts that cannot be resolved
                }
            }

            return results;
        });
    }

    /// <summary>Resolves a .lnk file to its target path using Shell32 COM.</summary>
    private static string? ResolveShortcutTarget(string lnkPath)
    {
        try
        {
            // Use Shell32 IShellLink to resolve .lnk target
            Type? shellLinkType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellLinkType == null) return null;

            dynamic? shell = Activator.CreateInstance(shellLinkType);
            if (shell == null) return null;

            dynamic? shortcut = shell.CreateShortcut(lnkPath);
            if (shortcut == null) return null;

            string target = shortcut.TargetPath ?? "";

            System.Runtime.InteropServices.Marshal.ReleaseComObject(shortcut);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(shell);

            if (string.IsNullOrEmpty(target) || !File.Exists(target))
                return null;

            return target;
        }
        catch
        {
            return null;
        }
    }

    public static void LaunchApp(string executablePath)
    {
        try
        {
            var psi = new ProcessStartInfo(executablePath)
            {
                UseShellExecute = true,
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to launch app: {ex.Message}");
        }
    }
}
