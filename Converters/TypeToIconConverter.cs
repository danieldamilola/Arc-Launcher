using System.Globalization;
using System.Windows.Data;

namespace Flow.Converters;

/// <summary>
/// Converts a result type string ("app" or "file") to an icon glyph or path data.
/// Returns Segoe Fluent Icons / MDL2 glyphs for common types.
/// </summary>
[ValueConversion(typeof(string), typeof(string))]
public class TypeToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string type)
            return "";

        return type.ToLowerInvariant() switch
        {
            "app"      => "Application",
            "file"     => "File",
            "folder"   => "Folder",
            "setting"  => "Setting",
            "url"      => "Web",
            "script"   => "Script",
            "image"    => "Image",
            "music"    => "Music",
            "video"    => "Video",
            "archive"  => "Archive",
            _          => "",
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("TypeToIconConverter does not support ConvertBack.");
    }
}
