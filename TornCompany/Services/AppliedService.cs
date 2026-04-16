using System.IO;
using System.Text.Json;

namespace TornCompany.Services;

public sealed class AppliedService
{
    private static readonly string FilePath = Path.Combine(
        AppContext.BaseDirectory, "applied.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private HashSet<string> _appliedNames = new(StringComparer.OrdinalIgnoreCase);

    public void Load()
    {
        if (!File.Exists(FilePath))
            return;

        var json = File.ReadAllText(FilePath);
        var names = JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
        if (names is not null)
            _appliedNames = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsApplied(string companyName) =>
        _appliedNames.Contains(companyName);

    public void SetApplied(string companyName, bool applied)
    {
        if (applied)
            _appliedNames.Add(companyName);
        else
            _appliedNames.Remove(companyName);

        Save();
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_appliedNames.OrderBy(n => n).ToList(), JsonOptions);
        File.WriteAllText(FilePath, json);
    }
}
