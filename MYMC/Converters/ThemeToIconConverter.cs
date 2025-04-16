using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.IconPacks;
using MYMC.Services.Interface;

namespace MYMC.Converters;

public class ThemeToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string themeName)
        {
            return themeName switch
            {
                IThemeService.DarkThemeName => PackIconForkAwesomeKind.MoonOutline,
                IThemeService.LightThemeName => PackIconForkAwesomeKind.SunOutline,
                _ => PackIconForkAwesomeKind.Question
            };
        }
        return PackIconForkAwesomeKind.Exclamation;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("ConvertBack is not supported.");
    }
}