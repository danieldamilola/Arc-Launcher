using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Models;
using Flow.Services;

namespace Flow.ViewModels;

/// <summary>
/// ViewModel for the settings panel. Wraps a working copy of <see cref="FlowConfig"/>
/// so edits can be committed or discarded.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private readonly MainViewModel _mainVm;

    /// <summary>Snapshot of the config before editing, for cancel/revert.</summary>
    private FlowConfig _originalConfig;

    // ─── Editable config properties ──────────────────────────────────

    [ObservableProperty]
    private string _theme = "dark";

    [ObservableProperty]
    private string _preset = "flow-dark";

    [ObservableProperty]
    private string _accentColor = "#007AFF";

    [ObservableProperty]
    private double _opacity = 1.0;

    [ObservableProperty]
    private string _borderRadius = "rounded";

    [ObservableProperty]
    private string _fontSize = "comfortable";

    [ObservableProperty]
    private string _shortcut = "Alt+Space";

    [ObservableProperty]
    private bool _showDetailPanel = true;

    [ObservableProperty]
    private string _groqApiKey = "";

    // ─── Dropdown options (read-only) ────────────────────────────────

    public static List<string> ThemeOptions { get; } = new() { "dark", "light", "system" };
    public static List<string> PresetOptions { get; } = new() { "flow-dark", "flow-light", "dracula", "nord", "catppuccin" };
    public static List<string> FontSizeOptions { get; } = new() { "compact", "comfortable", "spacious" };
    public static List<string> BorderRadiusOptions { get; } = new() { "sharp", "rounded", "square" };

    // ─── Constructor ─────────────────────────────────────────────────

    public SettingsViewModel(ConfigService configService, MainViewModel mainVm)
    {
        _configService = configService;
        _mainVm = mainVm;
        _originalConfig = mainVm.Config.Clone();
        LoadFromConfig(mainVm.Config);
    }

    /// <summary>Populates properties from a <see cref="FlowConfig"/> instance.</summary>
    private void LoadFromConfig(FlowConfig config)
    {
        Theme = config.Theme;
        Preset = config.Preset;
        AccentColor = config.AccentColor;
        Opacity = config.Opacity;
        BorderRadius = config.BorderRadius;
        FontSize = config.FontSize;
        Shortcut = config.Shortcut;
        ShowDetailPanel = config.ShowDetailPanel;
        GroqApiKey = config.GroqApiKey;
    }

    /// <summary>Builds a <see cref="FlowConfig"/> from the current property values.</summary>
    private FlowConfig BuildConfig()
    {
        return new FlowConfig
        {
            Theme = Theme,
            Preset = Preset,
            AccentColor = AccentColor,
            Opacity = Opacity,
            BorderRadius = BorderRadius,
            FontSize = FontSize,
            Shortcut = Shortcut,
            ShowDetailPanel = ShowDetailPanel,
            GroqApiKey = GroqApiKey,
        };
    }

    // ─── Commands ────────────────────────────────────────────────────

    /// <summary>Saves settings, persists to disk, and updates the main ViewModel.</summary>
    [RelayCommand]
    private async Task Save()
    {
        var newConfig = BuildConfig();
        await _configService.SaveAsync(newConfig);
        _mainVm.Config = newConfig;
        _originalConfig = newConfig.Clone();
    }

    /// <summary>Reverts all changes back to the original values.</summary>
    [RelayCommand]
    private void Cancel()
    {
        LoadFromConfig(_originalConfig);
    }

    /// <summary>Resets all settings to factory defaults.</summary>
    [RelayCommand]
    private void Reset()
    {
        var defaults = new FlowConfig();
        LoadFromConfig(defaults);
    }

    // ─── Converters ─────────────────────────────────────────────────

    /// <summary>
    /// Converts a hex colour string (e.g. "#007AFF") to a <see cref="Color"/>.
    /// Used by the SettingsPanel XAML to display the accent-colour preview circle.
    /// </summary>
    public static readonly IValueConverter ColorStringConverter = new ColorStringToColorConverter();

    private sealed class ColorStringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrEmpty(hex))
            {
                try
                {
                    return (Color)ColorConverter.ConvertFromString(hex);
                }
                catch
                {
                    return Colors.Transparent;
                }
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
                return color.ToString();
            return "#007AFF";
        }
    }
}
