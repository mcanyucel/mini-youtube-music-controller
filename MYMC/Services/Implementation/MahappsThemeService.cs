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

    public List<string> SupportedAccents { get; } =
    [
        "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", 
        "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna"
    ];
}