using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Flow.Services;

namespace Flow.Converters;

/// <summary>
/// Converts a file/shortcut path string into a BitmapSource icon.
/// Returns null if the icon cannot be extracted (XAML will hide the Image element).
/// </summary>
[ValueConversion(typeof(string), typeof(BitmapSource))]
public sealed class PathToIconConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrEmpty(path))
            return null;

        return IconService.GetIcon(path);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
