using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Flow.Converters;

/// <summary>
/// Converts a boolean to Visibility.
/// true  → Visible
/// false → Collapsed
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}
