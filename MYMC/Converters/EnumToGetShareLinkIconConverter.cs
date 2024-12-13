using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.IconPacks;
using MYMC.Models;

namespace MYMC.Converters;

public class EnumToGetShareLinkIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GetShareLinkStatus status)
        {
            return status switch
            {
                GetShareLinkStatus.Ready => PackIconBootstrapIconsKind.Share,
                GetShareLinkStatus.Loading => PackIconBootstrapIconsKind.HourglassSplit,
                GetShareLinkStatus.Error => PackIconBootstrapIconsKind.X,
                GetShareLinkStatus.Success => PackIconBootstrapIconsKind.Check,
                _ => null
            };
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("Cannot convert back");
    }
}