using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Flow.Services;

/// <summary>
/// Extracts and caches application icons from .exe / .lnk paths as WPF BitmapSources.
/// Uses SHGetFileInfo (Shell32) — no System.Drawing dependency required.
/// </summary>
public static class IconService
{
    private static readonly ConcurrentDictionary<string, BitmapSource?> _cache
        = new(StringComparer.OrdinalIgnoreCase);

    // SHGetFileInfo flags
    private const uint SHGFI_ICON       = 0x000000100;
    private const uint SHGFI_LARGEICON  = 0x000000000; // 32x32
    private const uint SHGFI_SMALLICON  = 0x000000001; // 16x16
    private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int    iIcon;
        public uint   dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
        ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    /// <summary>
    /// Returns a cached BitmapSource for the given path, or null on failure.
    /// Safe to call from any thread.
    /// </summary>
    public static BitmapSource? GetIcon(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        return _cache.GetOrAdd(path, static p =>
        {
            try { return ExtractFromPath(p); }
            catch { return null; }
        });
    }

    /// <summary>Pre-warms the icon cache for a batch of paths in the background.</summary>
    public static Task PreloadAsync(IEnumerable<string> paths)
        => Task.Run(() => { foreach (var p in paths) GetIcon(p); });

    private static BitmapSource? ExtractFromPath(string path)
    {
        var info = new SHFILEINFO();
        var result = SHGetFileInfo(path, 0, ref info,
            (uint)Marshal.SizeOf(info),
            SHGFI_ICON | SHGFI_LARGEICON);

        if (result == IntPtr.Zero || info.hIcon == IntPtr.Zero)
            return null;

        try
        {
            var bs = Imaging.CreateBitmapSourceFromHIcon(
                info.hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(32, 32));
            bs.Freeze();
            return bs;
        }
        finally
        {
            DestroyIcon(info.hIcon);
        }
    }
}
