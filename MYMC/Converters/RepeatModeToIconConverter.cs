using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.IconPacks;
using MYMC.Models;

namespace MYMC.Converters;

public class RepeatModeToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not RepeatMode mode)
        {
            return PackIconMaterialKind.RepeatOff;
        }

        return mode switch
        {
            RepeatMode.Off => PackIconMaterialKind.RepeatOff,
            RepeatMode.One => PackIconMaterialKind.RepeatOnce,
            RepeatMode.All => PackIconMaterialKind.Repeat,
            _ => PackIconMaterialKind.RepeatOff
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("RepeatModeToIconConverter can only be used OneWay.");
    }
}