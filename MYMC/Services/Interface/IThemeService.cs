namespace MYMC.Services.Interface;

public interface IThemeService
{
    void SetThemeAndAccent(string theme, string accent);
    
    const string LightThemeName = "Light";
    const string DarkThemeName = "Dark";
    const string DefaultAccentName = "Blue";
}