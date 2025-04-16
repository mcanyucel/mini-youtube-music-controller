namespace MYMC.Services.Interface;

public interface IThemeService
{
    void SetThemeAndAccent(string theme, string accent);
    List<string> SupportedAccents { get; }
    
    const string LightThemeName = "Light";
    const string DarkThemeName = "Dark";
    const string DefaultAccentName = "Blue";
    

}