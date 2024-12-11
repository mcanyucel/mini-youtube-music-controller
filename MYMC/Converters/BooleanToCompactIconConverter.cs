using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.IconPacks;

namespace MYMC.Converters;

public class BooleanToCompactIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? PackIconMaterialKind.ViewCompact : PackIconMaterialKind.ViewGallery;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("BooleanToCompactIconConverter can only be used OneWay.");
    }
}