using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using TDL.Configurator.Core;

using WpfButton = System.Windows.Controls.Button;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace TDL.Configurator.App.Pages;

public partial class ComedyPage : System.Windows.Controls.UserControl
{
    private const string SectionName = "Comedy";
    private const string UiTitle = "TDL Configurator";

    // Default значения (как в MCM)
    private const int DefaultFakeHeroDuration = 120;
    private const double DefaultFakeHeroActionInterval = 3.0;
    private const double DefaultFakeHeroDamageMult = 1.0;
    private const int DefaultFakeHeroPushForce = 5;
    private const int DefaultFakeHeroShoutChance = 30;
    private const int DefaultFakeHeroSpellChance = 30;

    private const int DefaultHorrorDuration = 120;
    private const int DefaultHorrorSpawn = 800;        // MCM: HorrorSpawnDist
    private const int DefaultHorrorTeleport = 600;     // MCM: HorrorTeleportDist
    private const int DefaultHorrorMaxDist = 3000;
    private const int DefaultHorrorHealth = 300;

    private const int DefaultArenaWaves = 3;
    private const int DefaultArenaPerWave = 3;
    private const double DefaultArenaInterval = 3.0;   // MCM: ArenaWaveInterval
    private const int DefaultArenaRadius = 800;        // MCM: ArenaSpawnRadius

    private const int DefaultEscortDuration = 120;

    public ComedyPage()
    {
        InitializeComponent();
        ApplyDefaultsToUi();
        StatusText.Text = "Готово (default).";
    }

    private static void ShowInfo(string message)
    {
        System.Windows.MessageBox.Show(
            message,
            UiTitle,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private string GamePath => AppSettings.Load().GamePath.Trim();
    private string PluginsFolder => Path.Combine(GamePath, "Data", "SKSE", "Plugins");
    private string IniPath => Path.Combine(PluginsFolder, "TDL_StreamPlugin.ini");

    private bool EnsureGamePath()
    {
        if (string.IsNullOrWhiteSpace(GamePath) || !Directory.Exists(GamePath))
        {
            System.Windows.MessageBox.Show(
                "Сначала укажи путь к игре в Настройках (корень Skyrim Special Edition).",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath()) return;

        if (!File.Exists(IniPath))
        {
            ApplyDefaultsToUi();
            StatusText.Text = "INI не найден (default).";
            System.Windows.MessageBox.Show(
                "INI не найден. Применены значения по умолчанию.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var map = ReadSection(IniPath, SectionName);
        if (map.Count == 0)
        {
            ApplyDefaultsToUi();
            StatusText.Text = "Секция [Comedy] не найдена (default).";
            System.Windows.MessageBox.Show(
                "Секция [Comedy] не найдена. Применены значения по умолчанию.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        FakeHeroDurationBox.Text = GetOr(map, "FakeHeroDuration", DefaultFakeHeroDuration.ToString(CultureInfo.InvariantCulture));
        FakeHeroActionIntervalBox.Text = GetOr(map, "FakeHeroActionInterval", DefaultFakeHeroActionInterval.ToString("0.##", CultureInfo.InvariantCulture));
        FakeHeroDamageMultBox.Text = GetOr(map, "FakeHeroDamageMult", DefaultFakeHeroDamageMult.ToString("0.##", CultureInfo.InvariantCulture));
        FakeHeroPushForceBox.Text = GetOr(map, "FakeHeroPushForce", DefaultFakeHeroPushForce.ToString(CultureInfo.InvariantCulture));
        FakeHeroShoutChanceBox.Text = GetOr(map, "FakeHeroShoutChance", DefaultFakeHeroShoutChance.ToString(CultureInfo.InvariantCulture));
        FakeHeroSpellChanceBox.Text = GetOr(map, "FakeHeroSpellChance", DefaultFakeHeroSpellChance.ToString(CultureInfo.InvariantCulture));

        HorrorDurationBox.Text = GetOr(map, "HorrorDuration", DefaultHorrorDuration.ToString(CultureInfo.InvariantCulture));
        HorrorSpawnBox.Text = GetOr(map, "HorrorSpawn", DefaultHorrorSpawn.ToString(CultureInfo.InvariantCulture));
        HorrorTeleportBox.Text = GetOr(map, "HorrorTeleport", DefaultHorrorTeleport.ToString(CultureInfo.InvariantCulture));
        HorrorMaxDistBox.Text = GetOr(map, "HorrorMaxDist", DefaultHorrorMaxDist.ToString(CultureInfo.InvariantCulture));
        HorrorHealthBox.Text = GetOr(map, "HorrorHealth", DefaultHorrorHealth.ToString(CultureInfo.InvariantCulture));

        ArenaWavesBox.Text = GetOr(map, "ArenaWaves", DefaultArenaWaves.ToString(CultureInfo.InvariantCulture));
        ArenaPerWaveBox.Text = GetOr(map, "ArenaPerWave", DefaultArenaPerWave.ToString(CultureInfo.InvariantCulture));
        ArenaIntervalBox.Text = GetOr(map, "ArenaInterval", DefaultArenaInterval.ToString("0.##", CultureInfo.InvariantCulture));
        ArenaRadiusBox.Text = GetOr(map, "ArenaRadius", DefaultArenaRadius.ToString(CultureInfo.InvariantCulture));

        EscortDurationBox.Text = GetOr(map, "EscortDuration", DefaultEscortDuration.ToString(CultureInfo.InvariantCulture));

        StatusText.Text = $"Загружено: {DateTime.Now:HH:mm:ss}";
        ShowInfo("Успешно загружено.");
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath()) return;

        // FakeHero
        if (!TryGetInt(FakeHeroDurationBox, "FakeHeroDuration", 10, 600, out var fakeHeroDuration)) return;
        if (!TryGetDouble(FakeHeroActionIntervalBox, "FakeHeroActionInterval", 0.5, 10.0, out var fakeHeroActionInterval)) return;
        if (!TryGetDouble(FakeHeroDamageMultBox, "FakeHeroDamageMult", 0.2, 5.0, out var fakeHeroDamageMult)) return;
        if (!TryGetInt(FakeHeroPushForceBox, "FakeHeroPushForce", 0, 50, out var fakeHeroPushForce)) return;
        if (!TryGetInt(FakeHeroShoutChanceBox, "FakeHeroShoutChance", 0, 100, out var fakeHeroShoutChance)) return;
        if (!TryGetInt(FakeHeroSpellChanceBox, "FakeHeroSpellChance", 0, 100, out var fakeHeroSpellChance)) return;

        // Horror
        if (!TryGetInt(HorrorDurationBox, "HorrorDuration", 10, 600, out var horrorDuration)) return;
        if (!TryGetInt(HorrorSpawnBox, "HorrorSpawn", 200, 3000, out var horrorSpawn)) return;
        if (!TryGetInt(HorrorTeleportBox, "HorrorTeleport", 200, 2000, out var horrorTeleport)) return;
        if (!TryGetInt(HorrorMaxDistBox, "HorrorMaxDist", 1000, 6000, out var horrorMaxDist)) return;
        if (!TryGetInt(HorrorHealthBox, "HorrorHealth", 50, 5000, out var horrorHealth)) return;

        // Arena
        if (!TryGetInt(ArenaWavesBox, "ArenaWaves", 1, 10, out var arenaWaves)) return;
        if (!TryGetInt(ArenaPerWaveBox, "ArenaPerWave", 1, 20, out var arenaPerWave)) return;
        if (!TryGetDouble(ArenaIntervalBox, "ArenaInterval", 0.5, 10.0, out var arenaInterval)) return;
        if (!TryGetInt(ArenaRadiusBox, "ArenaRadius", 200, 3000, out var arenaRadius)) return;

        // Escort
        if (!TryGetInt(EscortDurationBox, "EscortDuration", 30, 600, out var escortDuration)) return;

        Directory.CreateDirectory(PluginsFolder);

        var lines = File.Exists(IniPath)
            ? File.ReadAllLines(IniPath, Encoding.UTF8).ToList()
            : new List<string>();

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FakeHeroDuration"] = fakeHeroDuration.ToString(CultureInfo.InvariantCulture),
            ["FakeHeroActionInterval"] = fakeHeroActionInterval.ToString("0.##", CultureInfo.InvariantCulture),
            ["FakeHeroDamageMult"] = fakeHeroDamageMult.ToString("0.##", CultureInfo.InvariantCulture),
            ["FakeHeroPushForce"] = fakeHeroPushForce.ToString(CultureInfo.InvariantCulture),
            ["FakeHeroShoutChance"] = fakeHeroShoutChance.ToString(CultureInfo.InvariantCulture),
            ["FakeHeroSpellChance"] = fakeHeroSpellChance.ToString(CultureInfo.InvariantCulture),

            ["HorrorDuration"] = horrorDuration.ToString(CultureInfo.InvariantCulture),
            ["HorrorSpawn"] = horrorSpawn.ToString(CultureInfo.InvariantCulture),
            ["HorrorTeleport"] = horrorTeleport.ToString(CultureInfo.InvariantCulture),
            ["HorrorMaxDist"] = horrorMaxDist.ToString(CultureInfo.InvariantCulture),
            ["HorrorHealth"] = horrorHealth.ToString(CultureInfo.InvariantCulture),

            ["ArenaWaves"] = arenaWaves.ToString(CultureInfo.InvariantCulture),
            ["ArenaPerWave"] = arenaPerWave.ToString(CultureInfo.InvariantCulture),
            ["ArenaInterval"] = arenaInterval.ToString("0.##", CultureInfo.InvariantCulture),
            ["ArenaRadius"] = arenaRadius.ToString(CultureInfo.InvariantCulture),

            ["EscortDuration"] = escortDuration.ToString(CultureInfo.InvariantCulture),
        };

        UpsertSection(lines, SectionName, values);

        File.WriteAllLines(IniPath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        StatusText.Text = $"Сохранено: {DateTime.Now:HH:mm:ss}";
        ShowInfo("Успешно сохранено");
    }

    private void DefaultsAll_Click(object sender, RoutedEventArgs e)
    {
        ApplyDefaultsToUi();
        StatusText.Text = "Готово (default).";
        ShowInfo("Сброшено на значения по умолчанию.");
    }

    private void DefaultRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not WpfButton btn) return;

        var key = btn.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(key)) return;

        SetDefaultForKey(key);
        StatusText.Text = $"Default: {key}";
    }

    private void ApplyDefaultsToUi()
    {
        FakeHeroDurationBox.Text = DefaultFakeHeroDuration.ToString(CultureInfo.InvariantCulture);
        FakeHeroActionIntervalBox.Text = DefaultFakeHeroActionInterval.ToString("0.##", CultureInfo.InvariantCulture);
        FakeHeroDamageMultBox.Text = DefaultFakeHeroDamageMult.ToString("0.##", CultureInfo.InvariantCulture);
        FakeHeroPushForceBox.Text = DefaultFakeHeroPushForce.ToString(CultureInfo.InvariantCulture);
        FakeHeroShoutChanceBox.Text = DefaultFakeHeroShoutChance.ToString(CultureInfo.InvariantCulture);
        FakeHeroSpellChanceBox.Text = DefaultFakeHeroSpellChance.ToString(CultureInfo.InvariantCulture);

        HorrorDurationBox.Text = DefaultHorrorDuration.ToString(CultureInfo.InvariantCulture);
        HorrorSpawnBox.Text = DefaultHorrorSpawn.ToString(CultureInfo.InvariantCulture);
        HorrorTeleportBox.Text = DefaultHorrorTeleport.ToString(CultureInfo.InvariantCulture);
        HorrorMaxDistBox.Text = DefaultHorrorMaxDist.ToString(CultureInfo.InvariantCulture);
        HorrorHealthBox.Text = DefaultHorrorHealth.ToString(CultureInfo.InvariantCulture);

        ArenaWavesBox.Text = DefaultArenaWaves.ToString(CultureInfo.InvariantCulture);
        ArenaPerWaveBox.Text = DefaultArenaPerWave.ToString(CultureInfo.InvariantCulture);
        ArenaIntervalBox.Text = DefaultArenaInterval.ToString("0.##", CultureInfo.InvariantCulture);
        ArenaRadiusBox.Text = DefaultArenaRadius.ToString(CultureInfo.InvariantCulture);

        EscortDurationBox.Text = DefaultEscortDuration.ToString(CultureInfo.InvariantCulture);
    }

    private void SetDefaultForKey(string key)
    {
        switch (key)
        {
            case "FakeHeroDuration": FakeHeroDurationBox.Text = DefaultFakeHeroDuration.ToString(CultureInfo.InvariantCulture); break;
            case "FakeHeroActionInterval": FakeHeroActionIntervalBox.Text = DefaultFakeHeroActionInterval.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "FakeHeroDamageMult": FakeHeroDamageMultBox.Text = DefaultFakeHeroDamageMult.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "FakeHeroPushForce": FakeHeroPushForceBox.Text = DefaultFakeHeroPushForce.ToString(CultureInfo.InvariantCulture); break;
            case "FakeHeroShoutChance": FakeHeroShoutChanceBox.Text = DefaultFakeHeroShoutChance.ToString(CultureInfo.InvariantCulture); break;
            case "FakeHeroSpellChance": FakeHeroSpellChanceBox.Text = DefaultFakeHeroSpellChance.ToString(CultureInfo.InvariantCulture); break;

            case "HorrorDuration": HorrorDurationBox.Text = DefaultHorrorDuration.ToString(CultureInfo.InvariantCulture); break;
            case "HorrorSpawn": HorrorSpawnBox.Text = DefaultHorrorSpawn.ToString(CultureInfo.InvariantCulture); break;
            case "HorrorTeleport": HorrorTeleportBox.Text = DefaultHorrorTeleport.ToString(CultureInfo.InvariantCulture); break;
            case "HorrorMaxDist": HorrorMaxDistBox.Text = DefaultHorrorMaxDist.ToString(CultureInfo.InvariantCulture); break;
            case "HorrorHealth": HorrorHealthBox.Text = DefaultHorrorHealth.ToString(CultureInfo.InvariantCulture); break;

            case "ArenaWaves": ArenaWavesBox.Text = DefaultArenaWaves.ToString(CultureInfo.InvariantCulture); break;
            case "ArenaPerWave": ArenaPerWaveBox.Text = DefaultArenaPerWave.ToString(CultureInfo.InvariantCulture); break;
            case "ArenaInterval": ArenaIntervalBox.Text = DefaultArenaInterval.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "ArenaRadius": ArenaRadiusBox.Text = DefaultArenaRadius.ToString(CultureInfo.InvariantCulture); break;

            case "EscortDuration": EscortDurationBox.Text = DefaultEscortDuration.ToString(CultureInfo.InvariantCulture); break;
        }
    }

    private static string GetOr(Dictionary<string, string> map, string key, string fallback)
        => map.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : fallback;

    // ---------------- INI helpers ----------------
    private static Dictionary<string, string> ReadSection(string path, string sectionName)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var lines = File.ReadAllLines(path, Encoding.UTF8);

        var inSection = false;

        foreach (var raw in lines)
        {
            var line = (raw ?? "").Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith(";") || line.StartsWith("#")) continue;

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                inSection = string.Equals(line.Trim('[', ']'), sectionName, StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inSection) continue;

            var eq = line.IndexOf('=');
            if (eq <= 0) continue;

            var k = line.Substring(0, eq).Trim();
            var v = line.Substring(eq + 1).Trim();
            if (k.Length == 0) continue;

            result[k] = v;
        }

        return result;
    }

    private static void UpsertSection(List<string> lines, string sectionName, Dictionary<string, string> values)
    {
        var header = $"[{sectionName}]";

        var start = -1;
        for (var i = 0; i < lines.Count; i++)
        {
            if (string.Equals((lines[i] ?? "").Trim(), header, StringComparison.OrdinalIgnoreCase))
            {
                start = i;
                break;
            }
        }

        var newSection = new List<string> { header };
        foreach (var kv in values)
            newSection.Add($"{kv.Key}={kv.Value}");
        newSection.Add("");

        if (start < 0)
        {
            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                lines.Add("");
            lines.AddRange(newSection);
            return;
        }

        var end = start + 1;
        while (end < lines.Count)
        {
            var t = (lines[end] ?? "").Trim();
            if (t.StartsWith("[") && t.EndsWith("]"))
                break;
            end++;
        }

        lines.RemoveRange(start, end - start);
        lines.InsertRange(start, newSection);
    }

    // ---------------- Parse helpers ----------------
    private static bool TryGetInt(WpfTextBox box, string name, int min, int max, out int value)
    {
        value = 0;
        var text = (box.Text ?? "").Trim();

        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) &&
            !int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
        {
            System.Windows.MessageBox.Show($"{name} должен быть целым числом.", "Comedy",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show($"{name}: значение должно быть в диапазоне {min}..{max}.", "Comedy",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private static bool TryGetDouble(WpfTextBox box, string name, double min, double max, out double value)
    {
        value = 0;
        var text = (box.Text ?? "").Trim();

        if (!TryParseDoubleFlexible(text, out value))
        {
            System.Windows.MessageBox.Show($"{name} должен быть числом (пример: 1.50).", "Comedy",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show($"{name}: значение должно быть в диапазоне {min}..{max}.", "Comedy",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private static bool TryParseDoubleFlexible(string s, out double value)
    {
        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return true;

        if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
            return true;

        var swapped = s.Contains(',') ? s.Replace(',', '.') : s.Replace('.', ',');
        if (double.TryParse(swapped, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return true;

        value = 0;
        return false;
    }
}
