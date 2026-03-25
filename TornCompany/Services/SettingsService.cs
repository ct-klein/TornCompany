using System.IO;
using System.Text.Json;

namespace TornCompany.Services;

public sealed class SettingsService
{
    private static readonly string SettingsPath = Path.Combine(
        AppContext.BaseDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string ApiKey { get; set; } = string.Empty;

    public void Load()
    {
        if (!File.Exists(SettingsPath))
            return;

        var json = File.ReadAllText(SettingsPath);
        var settings = JsonSerializer.Deserialize<SettingsData>(json, JsonOptions);
        if (settings is not null)
        {
            ApiKey = settings.ApiKey ?? string.Empty;
        }
    }

    public void Save()
    {
        var settings = new SettingsData { ApiKey = ApiKey };
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsPath, json);
    }

    private sealed class SettingsData
    {
        public string? ApiKey { get; set; }
    }
}
