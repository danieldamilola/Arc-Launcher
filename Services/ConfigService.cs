using System.Text.Json;
using Flow.Models;

namespace Flow.Services;

public class ConfigService
{
    private readonly string _configPath;

    public ConfigService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dir = Path.Combine(appData, "Flow");
        Directory.CreateDirectory(dir);
        _configPath = Path.Combine(dir, "flow.config.json");
    }

    public FlowConfig Load()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<FlowConfig>(json) ?? new FlowConfig();
            }
        }
        catch { }

        var defaults = new FlowConfig();
        Save(defaults);
        return defaults;
    }

    public void Save(FlowConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
        catch { }
    }

    public async Task<FlowConfig> LoadAsync()
    {
        return await Task.Run(Load);
    }

    public async Task SaveAsync(FlowConfig config)
    {
        await Task.Run(() => Save(config));
    }
}
