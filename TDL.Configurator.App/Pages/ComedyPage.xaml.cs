// Auto-generated patch: autoload INI + Save+Apply (SYSTEM_RELOAD_CONFIG)
// Source of defaults/ranges: TDL_AllRanges.txt

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using TDL.Configurator.Core;

using WpfButton = System.Windows.Controls.Button;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace TDL.Configurator.App.Pages;

public partial class ComedyPage : System.Windows.Controls.UserControl
{
    private const string IniRelativePath = @"Data\SKSE\Plugins\TDL_StreamPlugin.ini";
    private const string ToolsRelativePath = @"Data\TDL\Tools\tdl_send.exe";

    private const string SectionName = "Comedy";
    private const string UiTitle = "TDL Configurator";

    // TDL_AllRanges.txt → COMEDY
    private const int DefaultFakeHeroDuration = 120;           // 10..600
    private const double DefaultFakeHeroActionInterval = 3.0;  // 0.5..10.0
    private const double DefaultFakeHeroDamageMult = 1.0;      // 0.2..5.0
    private const int DefaultFakeHeroPushForce = 5;            // 0..50
    private const int DefaultFakeHeroShoutChance = 30;         // 0..100
    private const int DefaultFakeHeroSpellChance = 30;         // 0..100

    private const int DefaultHorrorDuration = 120;             // 10..600
    private const int DefaultHorrorSpawn = 800;                // 200..3000
    private const int DefaultHorrorTeleport = 600;             // 200..2000
    private const int DefaultHorrorMaxDist = 3000;             // 1000..6000
    private const int DefaultHorrorHealth = 300;               // 50..5000

    private const int DefaultArenaWaves = 3;                   // 1..10
    private const int DefaultArenaPerWave = 3;                // 1..20
    private const double DefaultArenaInterval = 3.0;           // 0.5..10.0
    private const int DefaultArenaRadius = 800;                // 200..3000

    private const int DefaultEscortDuration = 120;             // 30..600

    public ComedyPage()
    {
        InitializeComponent();
        ApplyDefaultsToUi();
        AutoLoadFromIniSilent();
    }

    private static string SafeNow() => DateTime.Now.ToString("HH:mm:ss");

    private bool TryGetGamePath(out string gamePath)
    {
        gamePath = "";
        try
        {
            var s = AppSettings.Load();
            gamePath = (s?.GamePath ?? "").Trim();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                "Не удалось прочитать settings.json.\n" + ex.Message,
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
        {
            System.Windows.MessageBox.Show(
                "Путь к игре не задан или неверный.\nОткрой настройки и укажи папку Skyrim Special Edition.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private bool TryGetIniPath(out string iniPath)
    {
        iniPath = "";
        if (!TryGetGamePath(out var gamePath))
            return false;

        iniPath = Path.Combine(gamePath, IniRelativePath);
        return true;
    }

    private void AutoLoadFromIniSilent()
    {
        ApplyDefaultsToUi();

        if (!TryGetIniPath(out var iniPath))
        {
            StatusText.Text = "Путь к игре не задан (default).";
            return;
        }

        if (!File.Exists(iniPath))
        {
            StatusText.Text = "INI не найден (default).";
            return;
        }

        var map = ReadSection(iniPath, SectionName);

        FakeHeroDurationBox.Text = GetOr(map, "FakeHeroDuration", FakeHeroDurationBox.Text);
        FakeHeroActionIntervalBox.Text = GetOr(map, "FakeHeroActionInterval", FakeHeroActionIntervalBox.Text);
        FakeHeroDamageMultBox.Text = GetOr(map, "FakeHeroDamageMult", FakeHeroDamageMultBox.Text);
        FakeHeroPushForceBox.Text = GetOr(map, "FakeHeroPushForce", FakeHeroPushForceBox.Text);
        FakeHeroShoutChanceBox.Text = GetOr(map, "FakeHeroShoutChance", FakeHeroShoutChanceBox.Text);
        FakeHeroSpellChanceBox.Text = GetOr(map, "FakeHeroSpellChance", FakeHeroSpellChanceBox.Text);

        HorrorDurationBox.Text = GetOr(map, "HorrorDuration", HorrorDurationBox.Text);
        HorrorSpawnBox.Text = GetOr(map, "HorrorSpawn", HorrorSpawnBox.Text);
        HorrorTeleportBox.Text = GetOr(map, "HorrorTeleport", HorrorTeleportBox.Text);
        HorrorMaxDistBox.Text = GetOr(map, "HorrorMaxDist", HorrorMaxDistBox.Text);
        HorrorHealthBox.Text = GetOr(map, "HorrorHealth", HorrorHealthBox.Text);

        ArenaWavesBox.Text = GetOr(map, "ArenaWaves", ArenaWavesBox.Text);
        ArenaPerWaveBox.Text = GetOr(map, "ArenaPerWave", ArenaPerWaveBox.Text);
        ArenaIntervalBox.Text = GetOr(map, "ArenaInterval", ArenaIntervalBox.Text);
        ArenaRadiusBox.Text = GetOr(map, "ArenaRadius", ArenaRadiusBox.Text);

        EscortDurationBox.Text = GetOr(map, "EscortDuration", EscortDurationBox.Text);

        StatusText.Text = $"Загружено из INI ({SafeNow()}).";
    }

    private void SaveApply_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetIniPath(out var iniPath))
            return;

        if (!TryGetInt(FakeHeroDurationBox, "FakeHeroDuration", 10, 600, out var fakeHeroDuration)) return;
        if (!TryGetDouble(FakeHeroActionIntervalBox, "FakeHeroActionInterval", 0.5, 10.0, out var fakeHeroActionInterval)) return;
        if (!TryGetDouble(FakeHeroDamageMultBox, "FakeHeroDamageMult", 0.2, 5.0, out var fakeHeroDamageMult)) return;
        if (!TryGetInt(FakeHeroPushForceBox, "FakeHeroPushForce", 0, 50, out var fakeHeroPushForce)) return;
        if (!TryGetInt(FakeHeroShoutChanceBox, "FakeHeroShoutChance", 0, 100, out var fakeHeroShoutChance)) return;
        if (!TryGetInt(FakeHeroSpellChanceBox, "FakeHeroSpellChance", 0, 100, out var fakeHeroSpellChance)) return;

        if (!TryGetInt(HorrorDurationBox, "HorrorDuration", 10, 600, out var horrorDuration)) return;
        if (!TryGetInt(HorrorSpawnBox, "HorrorSpawn", 200, 3000, out var horrorSpawn)) return;
        if (!TryGetInt(HorrorTeleportBox, "HorrorTeleport", 200, 2000, out var horrorTeleport)) return;
        if (!TryGetInt(HorrorMaxDistBox, "HorrorMaxDist", 1000, 6000, out var horrorMaxDist)) return;
        if (!TryGetInt(HorrorHealthBox, "HorrorHealth", 50, 5000, out var horrorHealth)) return;

        if (!TryGetInt(ArenaWavesBox, "ArenaWaves", 1, 10, out var arenaWaves)) return;
        if (!TryGetInt(ArenaPerWaveBox, "ArenaPerWave", 1, 20, out var arenaPerWave)) return;
        if (!TryGetDouble(ArenaIntervalBox, "ArenaInterval", 0.5, 10.0, out var arenaInterval)) return;
        if (!TryGetInt(ArenaRadiusBox, "ArenaRadius", 200, 3000, out var arenaRadius)) return;

        if (!TryGetInt(EscortDurationBox, "EscortDuration", 30, 600, out var escortDuration)) return;

        var kv = new List<string>
        {
            $"FakeHeroDuration={fakeHeroDuration}",
            $"FakeHeroActionInterval={fakeHeroActionInterval.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"FakeHeroDamageMult={fakeHeroDamageMult.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"FakeHeroPushForce={fakeHeroPushForce}",
            $"FakeHeroShoutChance={fakeHeroShoutChance}",
            $"FakeHeroSpellChance={fakeHeroSpellChance}",

            $"HorrorDuration={horrorDuration}",
            $"HorrorSpawn={horrorSpawn}",
            $"HorrorTeleport={horrorTeleport}",
            $"HorrorMaxDist={horrorMaxDist}",
            $"HorrorHealth={horrorHealth}",

            $"ArenaWaves={arenaWaves}",
            $"ArenaPerWave={arenaPerWave}",
            $"ArenaInterval={arenaInterval.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"ArenaRadius={arenaRadius}",

            $"EscortDuration={escortDuration}",
        };

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(iniPath)!);
            UpsertSection(iniPath, SectionName, kv);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                "Не удалось сохранить INI.\n" + ex.Message,
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (TryApplyInGame(out var reason))
        {
            StatusText.Text = $"Сохранено и применено ({SafeNow()}).";
        }
        else
        {
            StatusText.Text = $"Сохранено, но не применено ({SafeNow()}).";
            System.Windows.MessageBox.Show(
                "INI сохранён, но применить в игре не удалось.\n" + reason,
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private bool TryApplyInGame(out string reason)
    {
        reason = "";
        if (!TryGetGamePath(out var gamePath))
        {
            reason = "Путь к игре не задан.";
            return false;
        }

        var tdlSend = Path.Combine(gamePath, ToolsRelativePath);
        if (!File.Exists(tdlSend))
        {
            reason = $"tdl_send.exe не найден: {tdlSend}";
            return false;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = tdlSend,
                Arguments = "NORMAL SYSTEM_RELOAD_CONFIG 2",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(tdlSend) ?? gamePath,
            };

            using var p = Process.Start(psi);
            if (p == null)
            {
                reason = "Не удалось запустить tdl_send.exe.";
                return false;
            }

            if (!p.WaitForExit(3500))
            {
                try { p.Kill(entireProcessTree: true); } catch { }
                reason = "tdl_send.exe не завершился по таймауту.";
                return false;
            }

            var stdout = p.StandardOutput.ReadToEnd().Trim();
            var stderr = p.StandardError.ReadToEnd().Trim();

            if (p.ExitCode != 0)
            {
                reason = $"Код выхода: {p.ExitCode}\n{(string.IsNullOrWhiteSpace(stderr) ? stdout : stderr)}".Trim();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            reason = ex.Message;
            return false;
        }
    }

    private void DefaultsAll_Click(object sender, RoutedEventArgs e)
    {
        ApplyDefaultsToUi();
        StatusText.Text = $"Сброшено на default ({SafeNow()}).";
    }

    private void DefaultRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not WpfButton btn)
            return;

        var key = btn.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(key))
            return;

        SetDefaultForKey(key);
        StatusText.Text = $"Default: {key} ({SafeNow()}).";
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

        StatusText.Text = "Готово (default).";
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

    // ---------- INI helpers ----------
    private static string GetOr(Dictionary<string, string> map, string key, string fallback)
        => map.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : fallback;

    private static bool IsSectionHeader(string line)
    {
        var t = (line ?? "").Trim();
        return t.StartsWith("[") && t.EndsWith("]");
    }

    private static Dictionary<string, string> ReadSection(string filePath, string sectionName)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);

        var inSection = false;
        var wanted = $"[{sectionName}]";

        foreach (var raw in lines)
        {
            var line = (raw ?? "").Trim();

            if (line.Length == 0) continue;
            if (line.StartsWith(";") || line.StartsWith("#")) continue;

            if (IsSectionHeader(line))
            {
                inSection = line.Equals(wanted, StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inSection) continue;

            var eq = line.IndexOf('=');
            if (eq <= 0) continue;

            var key = line.Substring(0, eq).Trim();
            var val = line.Substring(eq + 1).Trim();
            if (key.Length == 0) continue;

            result[key] = val;
        }

        return result;
    }

    private static void UpsertSection(string filePath, string sectionName, List<string> keyValueLines)
    {
        var lines = File.Exists(filePath)
            ? File.ReadAllLines(filePath, Encoding.UTF8).ToList()
            : new List<string>();

        var wanted = $"[{sectionName}]";
        var start = -1;
        var end = -1;

        for (var i = 0; i < lines.Count; i++)
        {
            var t = (lines[i] ?? "").Trim();
            if (!IsSectionHeader(t)) continue;

            if (t.Equals(wanted, StringComparison.OrdinalIgnoreCase))
            {
                start = i;
                continue;
            }

            if (start != -1)
            {
                end = i;
                break;
            }
        }

        if (start == -1)
        {
            if (lines.Count > 0 && lines[^1].Trim().Length != 0)
                lines.Add(string.Empty);

            lines.Add(wanted);
            lines.AddRange(keyValueLines);
        }
        else
        {
            if (end == -1)
                end = lines.Count;

            var removeCount = end - (start + 1);
            if (removeCount > 0)
                lines.RemoveRange(start + 1, removeCount);

            lines.InsertRange(start + 1, keyValueLines);
        }

        File.WriteAllLines(filePath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    // ---------- Validation ----------
    private static bool TryGetInt(WpfTextBox box, string name, int min, int max, out int value)
    {
        value = 0;
        if (!int.TryParse((box.Text ?? "").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            System.Windows.MessageBox.Show(
                $"{name}: введи целое число.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show(
                $"{name}: допустимый диапазон {min}..{max}.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private static bool TryGetDouble(WpfTextBox box, string name, double min, double max, out double value)
    {
        value = 0;
        var text = (box.Text ?? "").Trim().Replace(',', '.');
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            System.Windows.MessageBox.Show(
                $"{name}: введи число.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show(
                $"{name}: допустимый диапазон {min}..{max}.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }
}
