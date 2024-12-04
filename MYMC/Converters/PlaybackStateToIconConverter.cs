using System.Globalization;
using System.Windows.Data;

namespace MYMC.Converters;

public sealed class PlaybackStateToIconConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Any(q => q == System.Windows.DependencyProperty.UnsetValue))
        {
            return MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
        }

        var isBusy = (bool)values[0];
        var isPlaying = (bool)values[1];
        if (isBusy)
            return MahApps.Metro.IconPacks.PackIconMaterialKind.TimerSand;
        return isPlaying ? MahApps.Metro.IconPacks.PackIconMaterialKind.Pause : MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("PlaybackStateButtonConverter can only be used OneWay.");
    }
}