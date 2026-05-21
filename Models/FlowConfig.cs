namespace Flow.Models;

/// <summary>
/// Application configuration settings persisted to disk.
/// All properties have sensible defaults so a fresh install works without a config file.
/// </summary>
public class FlowConfig
{
    /// <summary>
    /// Colour theme: "dark" or "light".
    /// </summary>
    public string Theme { get; set; } = "dark";

    /// <summary>
    /// Preset style name applied to the window chrome (e.g. "flow-dark").
    /// </summary>
    public string Preset { get; set; } = "flow-dark";

    /// <summary>
    /// Hex accent colour used for highlights and interactive elements.
    /// </summary>
    public string AccentColor { get; set; } = "#007AFF";

    /// <summary>
    /// Window background opacity (0.0 = fully transparent, 1.0 = fully opaque).
    /// </summary>
    public double Opacity { get; set; } = 0.95;

    /// <summary>
    /// Corner style for the window: "rounded" or "square".
    /// </summary>
    public string BorderRadius { get; set; } = "rounded";

    /// <summary>
    /// Font size preset: "compact", "comfortable", or "spacious".
    /// </summary>
    public string FontSize { get; set; } = "comfortable";

    /// <summary>
    /// Global hotkey combination used to show/hide the launcher (e.g. "Alt+Space").
    /// </summary>
    public string Shortcut { get; set; } = "Alt+Space";

    /// <summary>
    /// Whether the detail/preview panel is visible by default.
    /// </summary>
    public bool ShowDetailPanel { get; set; } = true;

    /// <summary>
    /// API key for Groq-based AI features. Leave empty to disable AI.
    /// </summary>
    public string GroqApiKey { get; set; } = "";

    /// <summary>
    /// Maximum number of file results returned per search.
    /// </summary>
    public int MaxFileResults { get; set; } = 20;

    /// <summary>
    /// Maximum number of application results returned per search.
    /// </summary>
    public int MaxAppResults { get; set; } = 10;

    /// <summary>
    /// Creates a deep copy of this configuration.
    /// </summary>
    public FlowConfig Clone()
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
            MaxFileResults = MaxFileResults,
            MaxAppResults = MaxAppResults,
        };
    }
}
