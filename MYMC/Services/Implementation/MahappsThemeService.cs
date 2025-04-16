using System.Windows;
using ControlzEx.Theming;
using MYMC.Services.Interface;

namespace MYMC.Services.Implementation;

public class MahappsThemeService : IThemeService
{
    public void SetThemeAndAccent(string theme, string accent)
    {
        var styleName = $"{theme}.{accent}";
        ThemeManager.Current.ChangeTheme(Application.Current, styleName);
    }
}