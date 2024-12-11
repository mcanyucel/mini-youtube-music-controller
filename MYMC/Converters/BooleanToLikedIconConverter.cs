using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.IconPacks;

namespace MYMC.Converters;

public class BooleanToLikedIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? PackIconMaterialKind.ThumbUp : PackIconMaterialKind.ThumbUpOutline;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("This converter cannot convert back");
    }
}