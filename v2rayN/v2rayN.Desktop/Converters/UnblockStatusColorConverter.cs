using Avalonia.Data.Converters;
using Avalonia.Media;

namespace v2rayN.Desktop.Converters;

public class UnblockStatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value.ToString().ToInt();

        return status switch
        {
            1 => new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32)),
            2 => new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28)),
            3 => new SolidColorBrush(Color.FromRgb(0xEF, 0x6C, 0x00)),
            4 => new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75)),
            5 => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)),
            _ => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}
