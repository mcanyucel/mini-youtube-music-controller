using System.IO;
using System.Text.Json;

namespace MYMC.Settings;

public class UserSettings
{
    public bool IsTopMost { get; set; }
    public bool IsCompactMode { get; set; }
    public string Theme { get; set; } = "Light";
    public string Accent { get; set; } = "Blue";
    private static string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MYMC", "settings.json");
    public static UserSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                return JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(SettingsPath))
                       ?? new UserSettings();
            }
        }
        catch
        {
            // ignored
        }

        return new UserSettings();
    }
    
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath) ?? throw new InvalidOperationException());
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this));
        }
        catch
        {
            // ignored
        }
    }
}