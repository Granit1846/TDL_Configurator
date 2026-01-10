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

public partial class WrathPage : System.Windows.Controls.UserControl
{
    private const string IniRelativePath = @"Data\SKSE\Plugins\TDL_StreamPlugin.ini";
    private const string ToolsRelativePath = @"Data\TDL\Tools\tdl_send.exe";

    private const string SectionName = "Wrath";
    private const string UiTitle = "TDL Configurator";

    // TDL_AllRanges.txt → WRATH
    private const int DefaultWrathTotalBursts = 10;   // 1..50
    private const double DefaultWrathInterval = 0.4; // 0.05..2.0
    private const int DefaultWrathRadius = 800;      // 100..2000
    private const int DefaultWrathZOffset = 50;      // 0..500

    private const int DefaultWrathDamageMin = 5;     // 1..100
    private const int DefaultWrathDamageMax = 15;    // 1..100
    private const double DefaultWrathFireMult = 1.0;   // 0.0..5.0
    private const double DefaultWrathStormMag = 1.0;   // 0.0..5.0
    private const double DefaultWrathFrostSta = 1.0;   // 0.0..5.0
    private const double DefaultWrathLevelScale = 0.0; // 0.0..0.10
    private const double DefaultWrathLevelCap = 3.0;   // 1.0..5.0

    private const int DefaultWrathShakeChance = 20;      // 0..100 (percent)
    private const double DefaultWrathShakeStrength = 0.1; // 0.0..1.0
    private const double DefaultWrathShakeDuration = 0.2; // 0.0..1.0

    public WrathPage()
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
            WrathStatusText.Text = "Путь к игре не задан (default).";
            return;
        }

        if (!File.Exists(iniPath))
        {
            WrathStatusText.Text = "INI не найден (default).";
            return;
        }

        var map = ReadSection(iniPath, SectionName);

        WrathTotalBurstsBox.Text = GetOr(map, "WrathTotalBursts", WrathTotalBurstsBox.Text);
        WrathIntervalBox.Text = GetOr(map, "WrathInterval", WrathIntervalBox.Text);
        WrathRadiusBox.Text = GetOr(map, "WrathRadius", WrathRadiusBox.Text);
        WrathZOffsetBox.Text = GetOr(map, "WrathZOffset", WrathZOffsetBox.Text);

        WrathDamageMinBox.Text = GetOr(map, "WrathDamageMin", WrathDamageMinBox.Text);
        WrathDamageMaxBox.Text = GetOr(map, "WrathDamageMax", WrathDamageMaxBox.Text);
        WrathFireMultBox.Text = GetOr(map, "WrathFireMult", WrathFireMultBox.Text);
        WrathStormMagBox.Text = GetOr(map, "WrathStormMag", WrathStormMagBox.Text);
        WrathFrostStaBox.Text = GetOr(map, "WrathFrostSta", WrathFrostStaBox.Text);
        WrathLevelScaleBox.Text = GetOr(map, "WrathLevelScale", WrathLevelScaleBox.Text);
        WrathLevelCapBox.Text = GetOr(map, "WrathLevelCap", WrathLevelCapBox.Text);

        WrathShakeChanceBox.Text = GetOr(map, "WrathShakeChance", WrathShakeChanceBox.Text);
        WrathShakeStrengthBox.Text = GetOr(map, "WrathShakeStrength", WrathShakeStrengthBox.Text);
        WrathShakeDurationBox.Text = GetOr(map, "WrathShakeDuration", WrathShakeDurationBox.Text);

        WrathStatusText.Text = $"Загружено из INI ({SafeNow()}).";
    }

    private void SaveApply_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetIniPath(out var iniPath))
            return;

        if (!TryGetInt(WrathTotalBurstsBox, "WrathTotalBursts", 1, 50, out var totalBursts)) return;
        if (!TryGetDouble(WrathIntervalBox, "WrathInterval", 0.05, 2.0, out var interval)) return;
        if (!TryGetInt(WrathRadiusBox, "WrathRadius", 100, 2000, out var radius)) return;
        if (!TryGetInt(WrathZOffsetBox, "WrathZOffset", 0, 500, out var zOffset)) return;

        if (!TryGetInt(WrathDamageMinBox, "WrathDamageMin", 1, 100, out var dmgMin)) return;
        if (!TryGetInt(WrathDamageMaxBox, "WrathDamageMax", 1, 100, out var dmgMax)) return;

        if (!TryGetDouble(WrathFireMultBox, "WrathFireMult", 0.0, 5.0, out var fireMult)) return;
        if (!TryGetDouble(WrathStormMagBox, "WrathStormMag", 0.0, 5.0, out var stormMag)) return;
        if (!TryGetDouble(WrathFrostStaBox, "WrathFrostSta", 0.0, 5.0, out var frostSta)) return;

        if (!TryGetDouble(WrathLevelScaleBox, "WrathLevelScale", 0.0, 0.10, out var levelScale)) return;
        if (!TryGetDouble(WrathLevelCapBox, "WrathLevelCap", 1.0, 5.0, out var levelCap)) return;

        if (!TryGetInt(WrathShakeChanceBox, "WrathShakeChance", 0, 100, out var shakeChance)) return;
        if (!TryGetDouble(WrathShakeStrengthBox, "WrathShakeStrength", 0.0, 1.0, out var shakeStrength)) return;
        if (!TryGetDouble(WrathShakeDurationBox, "WrathShakeDuration", 0.0, 1.0, out var shakeDuration)) return;

        var kv = new List<string>
        {
            $"WrathTotalBursts={totalBursts}",
            $"WrathInterval={interval.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"WrathRadius={radius}",
            $"WrathZOffset={zOffset}",

            $"WrathDamageMin={dmgMin}",
            $"WrathDamageMax={dmgMax}",
            $"WrathFireMult={fireMult.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"WrathStormMag={stormMag.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"WrathFrostSta={frostSta.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"WrathLevelScale={levelScale.ToString("0.###", CultureInfo.InvariantCulture)}",
            $"WrathLevelCap={levelCap.ToString("0.##", CultureInfo.InvariantCulture)}",

            $"WrathShakeChance={shakeChance}",
            $"WrathShakeStrength={shakeStrength.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"WrathShakeDuration={shakeDuration.ToString("0.##", CultureInfo.InvariantCulture)}",
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
            WrathStatusText.Text = $"Сохранено и применено ({SafeNow()}).";
        }
        else
        {
            WrathStatusText.Text = $"Сохранено, но не применено ({SafeNow()}).";
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
        WrathStatusText.Text = $"Сброшено на default ({SafeNow()}).";
    }

    private void DefaultRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not WpfButton btn)
            return;

        var key = btn.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(key))
            return;

        SetDefaultForKey(key);
        WrathStatusText.Text = $"Default: {key} ({SafeNow()}).";
    }

    private void ApplyDefaultsToUi()
    {
        WrathTotalBurstsBox.Text = DefaultWrathTotalBursts.ToString(CultureInfo.InvariantCulture);
        WrathIntervalBox.Text = DefaultWrathInterval.ToString("0.##", CultureInfo.InvariantCulture);
        WrathRadiusBox.Text = DefaultWrathRadius.ToString(CultureInfo.InvariantCulture);
        WrathZOffsetBox.Text = DefaultWrathZOffset.ToString(CultureInfo.InvariantCulture);

        WrathDamageMinBox.Text = DefaultWrathDamageMin.ToString(CultureInfo.InvariantCulture);
        WrathDamageMaxBox.Text = DefaultWrathDamageMax.ToString(CultureInfo.InvariantCulture);
        WrathFireMultBox.Text = DefaultWrathFireMult.ToString("0.##", CultureInfo.InvariantCulture);
        WrathStormMagBox.Text = DefaultWrathStormMag.ToString("0.##", CultureInfo.InvariantCulture);
        WrathFrostStaBox.Text = DefaultWrathFrostSta.ToString("0.##", CultureInfo.InvariantCulture);
        WrathLevelScaleBox.Text = DefaultWrathLevelScale.ToString("0.###", CultureInfo.InvariantCulture);
        WrathLevelCapBox.Text = DefaultWrathLevelCap.ToString("0.##", CultureInfo.InvariantCulture);

        WrathShakeChanceBox.Text = DefaultWrathShakeChance.ToString(CultureInfo.InvariantCulture);
        WrathShakeStrengthBox.Text = DefaultWrathShakeStrength.ToString("0.##", CultureInfo.InvariantCulture);
        WrathShakeDurationBox.Text = DefaultWrathShakeDuration.ToString("0.##", CultureInfo.InvariantCulture);

        WrathStatusText.Text = "Готово (default).";
    }

    private void SetDefaultForKey(string key)
    {
        switch (key)
        {
            case "WrathTotalBursts": WrathTotalBurstsBox.Text = DefaultWrathTotalBursts.ToString(CultureInfo.InvariantCulture); break;
            case "WrathInterval": WrathIntervalBox.Text = DefaultWrathInterval.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "WrathRadius": WrathRadiusBox.Text = DefaultWrathRadius.ToString(CultureInfo.InvariantCulture); break;
            case "WrathZOffset": WrathZOffsetBox.Text = DefaultWrathZOffset.ToString(CultureInfo.InvariantCulture); break;

            case "WrathDamageMin": WrathDamageMinBox.Text = DefaultWrathDamageMin.ToString(CultureInfo.InvariantCulture); break;
            case "WrathDamageMax": WrathDamageMaxBox.Text = DefaultWrathDamageMax.ToString(CultureInfo.InvariantCulture); break;
            case "WrathFireMult": WrathFireMultBox.Text = DefaultWrathFireMult.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "WrathStormMag": WrathStormMagBox.Text = DefaultWrathStormMag.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "WrathFrostSta": WrathFrostStaBox.Text = DefaultWrathFrostSta.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "WrathLevelScale": WrathLevelScaleBox.Text = DefaultWrathLevelScale.ToString("0.###", CultureInfo.InvariantCulture); break;
            case "WrathLevelCap": WrathLevelCapBox.Text = DefaultWrathLevelCap.ToString("0.##", CultureInfo.InvariantCulture); break;

            case "WrathShakeChance": WrathShakeChanceBox.Text = DefaultWrathShakeChance.ToString(CultureInfo.InvariantCulture); break;
            case "WrathShakeStrength": WrathShakeStrengthBox.Text = DefaultWrathShakeStrength.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "WrathShakeDuration": WrathShakeDurationBox.Text = DefaultWrathShakeDuration.ToString("0.##", CultureInfo.InvariantCulture); break;
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
