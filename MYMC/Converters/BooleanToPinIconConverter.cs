using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.IconPacks;

namespace MYMC.Converters;

public class BooleanToPinIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? PackIconMaterialKind.Pin : PackIconMaterialKind.PinOff;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("BooleanToPinIconConverter can only be used OneWay.");
    }
}