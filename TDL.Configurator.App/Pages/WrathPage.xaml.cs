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

public partial class WrathPage : System.Windows.Controls.UserControl
{
    private const string UiTitle = "Wrath";
    private const string SectionName = "Wrath";

    // INI: один и тот же файл, разные секции
    private string GamePath => AppSettings.Load().GamePath.Trim();
    private string PluginsFolder => Path.Combine(GamePath, "Data", "SKSE", "Plugins");
    private string IniPath => Path.Combine(PluginsFolder, "TDL_StreamPlugin.ini");

    // Defaults (из диапазонов MCM)
    private const int DefaultWrathTotalBursts = 24;
    private const double DefaultWrathInterval = 0.10;
    private const int DefaultWrathRadius = 350;
    private const int DefaultWrathZOffset = 100;

    private const int DefaultWrathDamageMin = 35;
    private const int DefaultWrathDamageMax = 70;
    private const double DefaultWrathFireMult = 1.0;
    private const double DefaultWrathStormMag = 100.0;
    private const double DefaultWrathFrostSta = 50.0;
    private const double DefaultWrathLevelScale = 0.5;
    private const int DefaultWrathLevelCap = 30;

    private const double DefaultWrathShakeChance = 0.25;
    private const double DefaultWrathShakeStrength = 2.0;
    private const double DefaultWrathShakeDuration = 0.35;

    public WrathPage()
    {
        InitializeComponent();
        ApplyDefaultsToUi();
        WrathStatusText.Text = "Готово (default).";
    }

    private bool EnsureGamePath()
    {
        if (string.IsNullOrWhiteSpace(GamePath) || !Directory.Exists(GamePath))
        {
            System.Windows.MessageBox.Show(
                "Сначала укажи путь к игре в Настройках (корень Skyrim Special Edition).",
                "TDL Configurator",
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
            WrathStatusText.Text = "INI не найден — показаны default.";
            ShowInfo("Файл TDL_StreamPlugin.ini не найден. Показаны значения по умолчанию.");
            return;
        }

        var map = ReadSection(IniPath, SectionName);

        WrathTotalBurstsBox.Text = GetOr(map, "WrathTotalBursts", DefaultWrathTotalBursts.ToString(CultureInfo.InvariantCulture));
        WrathIntervalBox.Text = GetOr(map, "WrathInterval", DefaultWrathInterval.ToString(CultureInfo.InvariantCulture));
        WrathRadiusBox.Text = GetOr(map, "WrathRadius", DefaultWrathRadius.ToString(CultureInfo.InvariantCulture));
        WrathZOffsetBox.Text = GetOr(map, "WrathZOffset", DefaultWrathZOffset.ToString(CultureInfo.InvariantCulture));

        WrathDamageMinBox.Text = GetOr(map, "WrathDamageMin", DefaultWrathDamageMin.ToString(CultureInfo.InvariantCulture));
        WrathDamageMaxBox.Text = GetOr(map, "WrathDamageMax", DefaultWrathDamageMax.ToString(CultureInfo.InvariantCulture));
        WrathFireMultBox.Text = GetOr(map, "WrathFireMult", DefaultWrathFireMult.ToString(CultureInfo.InvariantCulture));
        WrathStormMagBox.Text = GetOr(map, "WrathStormMag", DefaultWrathStormMag.ToString(CultureInfo.InvariantCulture));
        WrathFrostStaBox.Text = GetOr(map, "WrathFrostSta", DefaultWrathFrostSta.ToString(CultureInfo.InvariantCulture));
        WrathLevelScaleBox.Text = GetOr(map, "WrathLevelScale", DefaultWrathLevelScale.ToString(CultureInfo.InvariantCulture));
        WrathLevelCapBox.Text = GetOr(map, "WrathLevelCap", DefaultWrathLevelCap.ToString(CultureInfo.InvariantCulture));

        WrathShakeChanceBox.Text = GetOr(map, "WrathShakeChance", DefaultWrathShakeChance.ToString(CultureInfo.InvariantCulture));
        WrathShakeStrengthBox.Text = GetOr(map, "WrathShakeStrength", DefaultWrathShakeStrength.ToString(CultureInfo.InvariantCulture));
        WrathShakeDurationBox.Text = GetOr(map, "WrathShakeDuration", DefaultWrathShakeDuration.ToString(CultureInfo.InvariantCulture));

        WrathStatusText.Text = $"Загружено: {DateTime.Now:HH:mm:ss}";
        ShowInfo("Загружено");
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath()) return;

        // Диапазоны (из MCM)
        if (!TryGetInt(WrathTotalBurstsBox, "WrathTotalBursts", 1, 500, out var totalBursts)) return;
        if (!TryGetDouble(WrathIntervalBox, "WrathInterval", 0.01, 1.0, out var interval)) return;
        if (!TryGetInt(WrathRadiusBox, "WrathRadius", 0, 3000, out var radius)) return;
        if (!TryGetInt(WrathZOffsetBox, "WrathZOffset", -500, 500, out var zOffset)) return;

        if (!TryGetInt(WrathDamageMinBox, "WrathDamageMin", 0, 999, out var dmgMin)) return;
        if (!TryGetInt(WrathDamageMaxBox, "WrathDamageMax", 0, 999, out var dmgMax)) return;

        if (!TryGetDouble(WrathFireMultBox, "WrathFireMult", 0.0, 10.0, out var fireMult)) return;
        if (!TryGetDouble(WrathStormMagBox, "WrathStormMag", 0.0, 1000.0, out var stormMag)) return;
        if (!TryGetDouble(WrathFrostStaBox, "WrathFrostSta", 0.0, 1000.0, out var frostSta)) return;

        if (!TryGetDouble(WrathLevelScaleBox, "WrathLevelScale", 0.0, 5.0, out var levelScale)) return;
        if (!TryGetInt(WrathLevelCapBox, "WrathLevelCap", 1, 999, out var levelCap)) return;

        if (!TryGetDouble(WrathShakeChanceBox, "WrathShakeChance", 0.0, 1.0, out var shakeChance)) return;
        if (!TryGetDouble(WrathShakeStrengthBox, "WrathShakeStrength", 0.0, 15.0, out var shakeStrength)) return;
        if (!TryGetDouble(WrathShakeDurationBox, "WrathShakeDuration", 0.0, 30.0, out var shakeDuration)) return;

        Directory.CreateDirectory(PluginsFolder);

        var lines = File.Exists(IniPath)
            ? File.ReadAllLines(IniPath, Encoding.UTF8).ToList()
            : new List<string>();

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["WrathTotalBursts"] = totalBursts.ToString(CultureInfo.InvariantCulture),
            ["WrathInterval"] = interval.ToString(CultureInfo.InvariantCulture),
            ["WrathRadius"] = radius.ToString(CultureInfo.InvariantCulture),
            ["WrathZOffset"] = zOffset.ToString(CultureInfo.InvariantCulture),

            ["WrathDamageMin"] = dmgMin.ToString(CultureInfo.InvariantCulture),
            ["WrathDamageMax"] = dmgMax.ToString(CultureInfo.InvariantCulture),
            ["WrathFireMult"] = fireMult.ToString(CultureInfo.InvariantCulture),
            ["WrathStormMag"] = stormMag.ToString(CultureInfo.InvariantCulture),
            ["WrathFrostSta"] = frostSta.ToString(CultureInfo.InvariantCulture),
            ["WrathLevelScale"] = levelScale.ToString(CultureInfo.InvariantCulture),
            ["WrathLevelCap"] = levelCap.ToString(CultureInfo.InvariantCulture),

            ["WrathShakeChance"] = shakeChance.ToString(CultureInfo.InvariantCulture),
            ["WrathShakeStrength"] = shakeStrength.ToString(CultureInfo.InvariantCulture),
            ["WrathShakeDuration"] = shakeDuration.ToString(CultureInfo.InvariantCulture),
        };

        UpsertSection(lines, SectionName, values);

        File.WriteAllLines(IniPath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        WrathStatusText.Text = $"Сохранено: {DateTime.Now:HH:mm:ss}";
        ShowInfo("Успешно сохранено");
    }

    private void DefaultsAll_Click(object sender, RoutedEventArgs e)
    {
        ApplyDefaultsToUi();
        WrathStatusText.Text = "Готово (default).";
        ShowInfo("Сброшено на значения по умолчанию.");
    }

    private void DefaultRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not WpfButton btn) return;

        var key = btn.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(key)) return;

        SetDefaultForKey(key);
        WrathStatusText.Text = $"Default: {key}";
    }

    private void ApplyDefaultsToUi()
    {
        WrathTotalBurstsBox.Text = DefaultWrathTotalBursts.ToString(CultureInfo.InvariantCulture);
        WrathIntervalBox.Text = DefaultWrathInterval.ToString(CultureInfo.InvariantCulture);
        WrathRadiusBox.Text = DefaultWrathRadius.ToString(CultureInfo.InvariantCulture);
        WrathZOffsetBox.Text = DefaultWrathZOffset.ToString(CultureInfo.InvariantCulture);

        WrathDamageMinBox.Text = DefaultWrathDamageMin.ToString(CultureInfo.InvariantCulture);
        WrathDamageMaxBox.Text = DefaultWrathDamageMax.ToString(CultureInfo.InvariantCulture);
        WrathFireMultBox.Text = DefaultWrathFireMult.ToString(CultureInfo.InvariantCulture);
        WrathStormMagBox.Text = DefaultWrathStormMag.ToString(CultureInfo.InvariantCulture);
        WrathFrostStaBox.Text = DefaultWrathFrostSta.ToString(CultureInfo.InvariantCulture);
        WrathLevelScaleBox.Text = DefaultWrathLevelScale.ToString(CultureInfo.InvariantCulture);
        WrathLevelCapBox.Text = DefaultWrathLevelCap.ToString(CultureInfo.InvariantCulture);

        WrathShakeChanceBox.Text = DefaultWrathShakeChance.ToString(CultureInfo.InvariantCulture);
        WrathShakeStrengthBox.Text = DefaultWrathShakeStrength.ToString(CultureInfo.InvariantCulture);
        WrathShakeDurationBox.Text = DefaultWrathShakeDuration.ToString(CultureInfo.InvariantCulture);
    }

    private void SetDefaultForKey(string key)
    {
        switch (key)
        {
            case "WrathTotalBursts": WrathTotalBurstsBox.Text = DefaultWrathTotalBursts.ToString(CultureInfo.InvariantCulture); break;
            case "WrathInterval": WrathIntervalBox.Text = DefaultWrathInterval.ToString(CultureInfo.InvariantCulture); break;
            case "WrathRadius": WrathRadiusBox.Text = DefaultWrathRadius.ToString(CultureInfo.InvariantCulture); break;
            case "WrathZOffset": WrathZOffsetBox.Text = DefaultWrathZOffset.ToString(CultureInfo.InvariantCulture); break;

            case "WrathDamageMin": WrathDamageMinBox.Text = DefaultWrathDamageMin.ToString(CultureInfo.InvariantCulture); break;
            case "WrathDamageMax": WrathDamageMaxBox.Text = DefaultWrathDamageMax.ToString(CultureInfo.InvariantCulture); break;
            case "WrathFireMult": WrathFireMultBox.Text = DefaultWrathFireMult.ToString(CultureInfo.InvariantCulture); break;
            case "WrathStormMag": WrathStormMagBox.Text = DefaultWrathStormMag.ToString(CultureInfo.InvariantCulture); break;
            case "WrathFrostSta": WrathFrostStaBox.Text = DefaultWrathFrostSta.ToString(CultureInfo.InvariantCulture); break;
            case "WrathLevelScale": WrathLevelScaleBox.Text = DefaultWrathLevelScale.ToString(CultureInfo.InvariantCulture); break;
            case "WrathLevelCap": WrathLevelCapBox.Text = DefaultWrathLevelCap.ToString(CultureInfo.InvariantCulture); break;

            case "WrathShakeChance": WrathShakeChanceBox.Text = DefaultWrathShakeChance.ToString(CultureInfo.InvariantCulture); break;
            case "WrathShakeStrength": WrathShakeStrengthBox.Text = DefaultWrathShakeStrength.ToString(CultureInfo.InvariantCulture); break;
            case "WrathShakeDuration": WrathShakeDurationBox.Text = DefaultWrathShakeDuration.ToString(CultureInfo.InvariantCulture); break;
        }
    }

    private static void ShowInfo(string text)
    {
        System.Windows.MessageBox.Show(
            text,
            "TDL Configurator",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
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

    // ---------------- Parse helpers (диапазоны) ----------------

    private static bool TryGetInt(WpfTextBox box, string name, int min, int max, out int value)
    {
        value = 0;
        var text = (box.Text ?? "").Trim();

        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) &&
            !int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
        {
            System.Windows.MessageBox.Show($"{name} должен быть целым числом.", UiTitle,
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show($"{name}: значение должно быть в диапазоне {min}..{max}.", UiTitle,
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
            System.Windows.MessageBox.Show($"{name} должен быть числом (пример: 0.20).", UiTitle,
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show($"{name}: значение должно быть в диапазоне {min}..{max}.", UiTitle,
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
