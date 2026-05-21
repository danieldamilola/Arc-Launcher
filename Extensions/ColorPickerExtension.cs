using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;

namespace Flow.Extensions;

/// <summary>
/// Parses a hex color from the query and displays a preview swatch
/// along with hex, RGB, and HSL breakdowns.
/// </summary>
public partial class ColorPickerExtension : UserControl, IExtension
{
    public string Id => "color";
    public string DisplayName => "Color Picker";

    /// <summary>
    /// Matches #RGB or #RRGGBB hex color notation.
    /// </summary>
    public object Trigger => new Regex(
        @"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$",
        RegexOptions.Compiled);

    public ColorPickerExtension()
    {
        InitializeComponent();
    }

    public UserControl CreateControl(string query)
    {
        var control = new ColorPickerExtension();
        control.ParseColor(query.Trim());
        return control;
    }

    private void ParseColor(string hex)
    {
        try
        {
            // Expand shorthand #RGB → #RRGGBB
            if (hex.Length == 4) // #RGB
            {
                hex = $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
            }

            if (!TryParseHexColor(hex, out var color))
            {
                ShowError("Invalid hex color");
                return;
            }

            var brush = new SolidColorBrush(color);
            ColorSwatch.Background = brush;

            HexLabel.Text = hex.ToUpperInvariant();
            RgbLabel.Text = $"R: {color.R}  G: {color.G}  B: {color.B}";

            var (h, s, l) = RgbToHsl(color.R, color.G, color.B);
            HslLabel.Text = $"H: {h:F0}°  S: {s:F0}%  L: {l:F0}%";
        }
        catch
        {
            ShowError("Invalid hex color");
        }
    }

    private static bool TryParseHexColor(string hex, out Color color)
    {
        color = default;

        if (string.IsNullOrEmpty(hex) || hex.Length != 7 || hex[0] != '#')
            return false;

        try
        {
            byte r = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);

            color = Color.FromRgb(r, g, b);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts RGB (0-255) to HSL.
    /// Returns H in degrees (0-360), S and L as percentages (0-100).
    /// </summary>
    private static (double H, double S, double L) RgbToHsl(byte r, byte g, byte b)
    {
        double dr = r / 255.0;
        double dg = g / 255.0;
        double db = b / 255.0;

        double max = Math.Max(dr, Math.Max(dg, db));
        double min = Math.Min(dr, Math.Min(dg, db));
        double delta = max - min;

        double h = 0;
        double s = 0;
        double l = (max + min) / 2.0;

        if (delta > 0.0001)
        {
            s = l > 0.5 ? delta / (2.0 - max - min) : delta / (max + min);

            if (Math.Abs(max - dr) < 0.0001)
            {
                h = ((dg - db) / delta) + (dg < db ? 6 : 0);
            }
            else if (Math.Abs(max - dg) < 0.0001)
            {
                h = ((db - dr) / delta) + 2;
            }
            else
            {
                h = ((dr - dg) / delta) + 4;
            }

            h *= 60;
        }

        return (Math.Round(h, 1), Math.Round(s * 100, 0), Math.Round(l * 100, 0));
    }

    private void ShowError(string message)
    {
        HexLabel.Text = "";
        RgbLabel.Text = "";
        HslLabel.Text = "";
        ErrorLabel.Text = message;
        ErrorLabel.Visibility = System.Windows.Visibility.Visible;
        ColorSwatch.Background = Brushes.Transparent;
    }
}
