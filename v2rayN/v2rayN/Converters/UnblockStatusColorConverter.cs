using System.Windows.Media;

namespace v2rayN.Converters;

public class UnblockStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var status = value.ToString().ToInt();

        return status switch
        {
            1 => new SolidColorBrush(Color.FromRgb(0x66, 0xBB, 0x6A)),
            2 => new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50)),
            3 => new SolidColorBrush(Color.FromRgb(0xFF, 0xB7, 0x4D)),
            4 => new SolidColorBrush(Color.FromRgb(0xBD, 0xBD, 0xBD)),
            5 => new SolidColorBrush(Color.FromRgb(0xB0, 0xBE, 0xC5)),
            _ => new SolidColorBrush(Color.FromRgb(0xB0, 0xBE, 0xC5))
        };
    }

    public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return null;
    }
}
