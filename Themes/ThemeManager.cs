using System.Windows;
using System.Windows.Media;

namespace Flow.Themes;

/// <summary>
/// Manages application theme switching by replacing the merged ResourceDictionary
/// containing theme color brushes.
/// </summary>
public static class ThemeManager
{
    private const string ThemesBasePath = "/Themes/";

    private static readonly Dictionary<string, string> ThemePathMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Dark"]        = $"{ThemesBasePath}DarkTheme.xaml",
        ["Light"]       = $"{ThemesBasePath}LightTheme.xaml",
        ["Dracula"]     = $"{ThemesBasePath}DraculaTheme.xaml",
        ["Nord"]        = $"{ThemesBasePath}NordTheme.xaml",
        ["Catppuccin"]  = $"{ThemesBasePath}CatppuccinTheme.xaml",
    };

    private static int _themeDictionaryIndex = -1;

    /// <summary>
    /// Applies the specified theme preset and optional custom accent color.
    /// </summary>
    /// <param name="presetName">Theme preset name: Dark, Light, Dracula, Nord, Catppuccin.</param>
    /// <param name="accentColor">Optional hex accent color (e.g., "#007AFF").</param>
    public static void ApplyTheme(string presetName, string? accentColor = null)
    {
        var appResources = Application.Current.Resources;
        var mergedDicts = appResources.MergedDictionaries;

        // Find and cache the index of our theme dictionary
        if (_themeDictionaryIndex < 0)
        {
            for (int i = 0; i < mergedDicts.Count; i++)
            {
                var source = mergedDicts[i].Source?.ToString();
                if (source != null && source.Contains("/Themes/", StringComparison.OrdinalIgnoreCase))
                {
                    _themeDictionaryIndex = i;
                    break;
                }
            }
        }

        // Resolve the theme file path
        if (!ThemePathMap.TryGetValue(presetName, out var themePath))
        {
            // Fall back to Dark if the preset isn't recognized
            themePath = ThemePathMap["Dark"];
        }

        var newThemeDict = new ResourceDictionary
        {
            Source = new Uri(themePath, UriKind.Relative)
        };

        if (_themeDictionaryIndex >= 0 && _themeDictionaryIndex < mergedDicts.Count)
        {
            mergedDicts[_themeDictionaryIndex] = newThemeDict;
        }
        else
        {
            // First time or not found — add to the end
            mergedDicts.Add(newThemeDict);
            _themeDictionaryIndex = mergedDicts.Count - 1;
        }

        // Override the accent color if provided
        if (!string.IsNullOrWhiteSpace(accentColor))
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(accentColor);
                appResources["Accent"] = new SolidColorBrush(color);
            }
            catch
            {
                // Ignore invalid accent color strings
            }
        }
    }

    /// <summary>
    /// Returns the list of available theme preset names.
    /// </summary>
    public static IReadOnlyList<string> AvailableThemes => ThemePathMap.Keys.ToList().AsReadOnly();
}
