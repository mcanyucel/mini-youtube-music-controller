using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.IconPacks;

namespace MYMC.Converters;

public class BooleanToShuffleIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool isShuffled)
        {
            return null;
        }

        return isShuffled ? PackIconMaterialKind.ShuffleVariant : PackIconMaterialKind.ShuffleDisabled;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("BooleanToShuffleIconConverter can only be used OneWay.");
    }
}