using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.IconPacks;

namespace MYMC.Converters;

public class IntegerToVolumeIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int volume)
        {
            return null;
        }

        return volume switch
        {
            0 => PackIconMaterialKind.VolumeVariantOff,
            <= 33 => PackIconMaterialKind.VolumeLow,
            <= 66 => PackIconMaterialKind.VolumeMedium,
            _ => PackIconMaterialKind.VolumeHigh
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("IntegerToVolumeIconConverter can only be used OneWay.");
    }
}