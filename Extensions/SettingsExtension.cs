using System.Windows;
using System.Windows.Controls;
using Flow.Models;
using Flow.Services;

namespace Flow.Extensions;

/// <summary>
/// Settings panel triggered by typing "settings".
/// Allows configuring theme, accent color, font size, opacity, shortcut, and API key.
/// </summary>
public partial class SettingsExtension : UserControl, IExtension
{
    public string Id => "settings";
    public string DisplayName => "Settings";
    public object Trigger => "settings";

    private readonly ConfigService _configService = new();
    private FlowConfig _config = new();

    public SettingsExtension()
    {
        InitializeComponent();
        OpacitySlider.ValueChanged += OnOpacityChanged;
    }

    public UserControl CreateControl(string query)
    {
        var control = new SettingsExtension();
        _ = control.LoadConfigAsync();
        return control;
    }

    private async Task LoadConfigAsync()
    {
        _config = await _configService.LoadAsync();

        // Apply config to UI
        ThemeCombo.SelectedIndex = _config.Theme switch
        {
            "light" => 1,
            "system" => 2,
            _ => 0
        };

        AccentColorBox.Text = _config.AccentColor;

        FontSizeCombo.SelectedIndex = _config.FontSize switch
        {
            "compact" => 0,
            "large" => 2,
            _ => 1
        };

        OpacitySlider.Value = _config.Opacity;
        OpacityLabel.Text = $"{_config.Opacity * 100:F0}%";
        ShortcutLabel.Text = _config.Shortcut;
        ApiKeyBox.Text = string.IsNullOrWhiteSpace(_config.GroqApiKey) ? "" : "••••••••"; // Mask existing key
    }

    private void OnOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        OpacityLabel.Text = $"{e.NewValue * 100:F0}%";
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        // Read values from UI
        _config.Theme = ThemeCombo.SelectedIndex switch
        {
            1 => "light",
            2 => "system",
            _ => "dark"
        };

        _config.AccentColor = AccentColorBox.Text.Trim();
        _config.FontSize = FontSizeCombo.SelectedIndex switch
        {
            0 => "compact",
            2 => "large",
            _ => "comfortable"
        };

        _config.Opacity = OpacitySlider.Value;
        _config.Shortcut = ShortcutLabel.Text;

        var apiKey = ApiKeyBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            Environment.SetEnvironmentVariable("FLOW_GROQ_API_KEY", apiKey);
        }

        await _configService.SaveAsync(_config);

        // Show saved confirmation
        SavedLabel.Visibility = Visibility.Visible;
        await Task.Delay(2000);
        SavedLabel.Visibility = Visibility.Collapsed;
    }
}
