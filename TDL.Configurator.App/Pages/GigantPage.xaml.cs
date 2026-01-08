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

public partial class GigantPage : System.Windows.Controls.UserControl
{
    private const string IniRelativePath = @"Data\SKSE\Plugins\TDL_StreamPlugin.ini";
    private const string SectionName = "Gigant";
    private const string UiTitle = "TDL Configurator";

    // Диапазоны и defaults ДЛЯ ЭТОЙ СТРАНИЦЫ берём из обновлённого TDL_AllRanges.txt (секция GIGANT).
    private const double DefaultDamageBig = 5.0;      // range 0.0..10.0
    private const double DefaultDamageSmall = 0.5;    // range 0.0..1.0
    private const double DefaultScaleBig = 2.0;       // range 0.1..5.0
    private const double DefaultScaleSmall = 0.33;    // range 0.1..1.0
    private const int DefaultSizeDuration = 60;       // range 5..600
    private const int DefaultSpeedDuration = 60;      // range 5..600
    private const double DefaultSpeedFast = 3.0;      // range 1.0..10.0
    private const double DefaultSpeedSlow = 0.5;      // range 0.1..1.0

    private static void ShowInfo(string message)
    {
        System.Windows.MessageBox.Show(
            message,
            UiTitle,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    public GigantPage()
    {
        InitializeComponent();
        ApplyDefaultsToUi();
        GigantStatusText.Text = "Готово (default).";
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

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetIniPath(out var iniPath)) return;

        if (!File.Exists(iniPath))
        {
            ApplyDefaultsToUi();
            GigantStatusText.Text = "INI не найден (default).";
            return;
        }

        var map = ReadSection(iniPath, SectionName);
        if (map.Count == 0)
        {
            ApplyDefaultsToUi();
            GigantStatusText.Text = "Секция [Gigant] не найдена (default).";
            return;
        }

        ScaleBigBox.Text = GetOr(map, "ScaleBig", DefaultScaleBig.ToString("0.##", CultureInfo.InvariantCulture));
        DamageBigBox.Text = GetOr(map, "DamageBig", DefaultDamageBig.ToString("0.##", CultureInfo.InvariantCulture));
        ScaleSmallBox.Text = GetOr(map, "ScaleSmall", DefaultScaleSmall.ToString("0.##", CultureInfo.InvariantCulture));
        DamageSmallBox.Text = GetOr(map, "DamageSmall", DefaultDamageSmall.ToString("0.##", CultureInfo.InvariantCulture));
        SizeDurationBox.Text = GetOr(map, "SizeDuration", DefaultSizeDuration.ToString(CultureInfo.InvariantCulture));

        SpeedFastBox.Text = GetOr(map, "SpeedFast", DefaultSpeedFast.ToString("0.##", CultureInfo.InvariantCulture));
        SpeedSlowBox.Text = GetOr(map, "SpeedSlow", DefaultSpeedSlow.ToString("0.##", CultureInfo.InvariantCulture));
        SpeedDurationBox.Text = GetOr(map, "SpeedDuration", DefaultSpeedDuration.ToString(CultureInfo.InvariantCulture));

        GigantStatusText.Text = $"Загружено: {SafeNow()}";
        ShowInfo("Успешно загружено.");
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetIniPath(out var iniPath)) return;

        // Валидация по min/max из TDL_AllRanges.txt (GIGANT)
        if (!TryGetDouble(DamageBigBox, "DamageBig", 0.0, 10.0, out var damageBig)) return;
        if (!TryGetDouble(DamageSmallBox, "DamageSmall", 0.0, 1.0, out var damageSmall)) return;

        if (!TryGetDouble(ScaleBigBox, "ScaleBig", 0.1, 5.0, out var scaleBig)) return;
        if (!TryGetDouble(ScaleSmallBox, "ScaleSmall", 0.1, 1.0, out var scaleSmall)) return;

        if (!TryGetInt(SizeDurationBox, "SizeDuration", 5, 600, out var sizeDuration)) return;

        if (!TryGetDouble(SpeedFastBox, "SpeedFast", 1.0, 10.0, out var speedFast)) return;
        if (!TryGetDouble(SpeedSlowBox, "SpeedSlow", 0.1, 1.0, out var speedSlow)) return;
        if (!TryGetInt(SpeedDurationBox, "SpeedDuration", 5, 600, out var speedDuration)) return;

        var iniDir = Path.GetDirectoryName(iniPath);
        if (!string.IsNullOrWhiteSpace(iniDir))
            Directory.CreateDirectory(iniDir);

        var lines = File.Exists(iniPath)
            ? File.ReadAllLines(iniPath, Encoding.UTF8).ToList()
            : new List<string>();

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ScaleBig"] = scaleBig.ToString("0.##", CultureInfo.InvariantCulture),
            ["DamageBig"] = damageBig.ToString("0.##", CultureInfo.InvariantCulture),
            ["ScaleSmall"] = scaleSmall.ToString("0.##", CultureInfo.InvariantCulture),
            ["DamageSmall"] = damageSmall.ToString("0.##", CultureInfo.InvariantCulture),
            ["SizeDuration"] = sizeDuration.ToString(CultureInfo.InvariantCulture),

            ["SpeedFast"] = speedFast.ToString("0.##", CultureInfo.InvariantCulture),
            ["SpeedSlow"] = speedSlow.ToString("0.##", CultureInfo.InvariantCulture),
            ["SpeedDuration"] = speedDuration.ToString(CultureInfo.InvariantCulture),
        };

        UpsertSection(lines, SectionName, values);

        File.WriteAllLines(iniPath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        GigantStatusText.Text = $"Сохранено: {SafeNow()}";
        ShowInfo("Успешно сохранено");
    }

    private void DefaultsAll_Click(object sender, RoutedEventArgs e)
    {
        ApplyDefaultsToUi();
        GigantStatusText.Text = $"Сброшено на default ({SafeNow()}).";
        ShowInfo("Сброшено на значения по умолчанию.");
    }

    private void DefaultRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not WpfButton btn) return;

        var key = btn.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(key)) return;

        SetDefaultForKey(key);
        GigantStatusText.Text = $"Default: {key}";
    }

    private void ApplyDefaultsToUi()
    {
        ScaleBigBox.Text = DefaultScaleBig.ToString("0.##", CultureInfo.InvariantCulture);
        DamageBigBox.Text = DefaultDamageBig.ToString("0.##", CultureInfo.InvariantCulture);
        ScaleSmallBox.Text = DefaultScaleSmall.ToString("0.##", CultureInfo.InvariantCulture);
        DamageSmallBox.Text = DefaultDamageSmall.ToString("0.##", CultureInfo.InvariantCulture);
        SizeDurationBox.Text = DefaultSizeDuration.ToString(CultureInfo.InvariantCulture);

        SpeedFastBox.Text = DefaultSpeedFast.ToString("0.##", CultureInfo.InvariantCulture);
        SpeedSlowBox.Text = DefaultSpeedSlow.ToString("0.##", CultureInfo.InvariantCulture);
        SpeedDurationBox.Text = DefaultSpeedDuration.ToString(CultureInfo.InvariantCulture);
    }

    private void SetDefaultForKey(string key)
    {
        switch (key)
        {
            case "ScaleBig": ScaleBigBox.Text = DefaultScaleBig.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "DamageBig": DamageBigBox.Text = DefaultDamageBig.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "ScaleSmall": ScaleSmallBox.Text = DefaultScaleSmall.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "DamageSmall": DamageSmallBox.Text = DefaultDamageSmall.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "SizeDuration": SizeDurationBox.Text = DefaultSizeDuration.ToString(CultureInfo.InvariantCulture); break;

            case "SpeedFast": SpeedFastBox.Text = DefaultSpeedFast.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "SpeedSlow": SpeedSlowBox.Text = DefaultSpeedSlow.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "SpeedDuration": SpeedDurationBox.Text = DefaultSpeedDuration.ToString(CultureInfo.InvariantCulture); break;
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

    // ---------------- Parse helpers (min/max) ----------------

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
            System.Windows.MessageBox.Show($"{name} должен быть числом (пример: 0.33).", UiTitle,
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
