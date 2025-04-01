using CreativeFileBrowser;
using System.Diagnostics;
using System.Text.Json;

namespace CreativeFileBrowser;

public static class SettingsManager
{
    private static readonly string FilePath = "appsettings.json";

    public static AppSettings Load()
    {
        if (!File.Exists(FilePath)) return new AppSettings();

        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch { /* log or ignore */ }
    }

}