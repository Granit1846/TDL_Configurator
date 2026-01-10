using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TDL.Configurator.Core;

public sealed class AppSettings
{
    public string GamePath { get; set; } = "";

    // UI
    public AppTheme Theme { get; set; } = AppTheme.Light;
    public AppLanguage Language { get; set; } = AppLanguage.Ru;

    public static string SettingsFilePath
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TDL.Configurator");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "settings.json");
        }
    }

    public static AppSettings Load()
    {
        try
        {
            var path = SettingsFilePath;
            if (!File.Exists(path))
                return new AppSettings();

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this, SerializerOptions);
        File.WriteAllText(SettingsFilePath, json);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
