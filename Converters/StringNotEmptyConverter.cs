using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Flow.Converters;

/// <summary>
/// Converts a string to Visibility. True (non-null, non-empty) → Visible, else → Collapsed.
/// </summary>
public class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
