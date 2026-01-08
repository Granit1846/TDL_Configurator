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
using WpfUserControl = System.Windows.Controls.UserControl;
using WpfMessageBox = System.Windows.MessageBox;
using WpfMessageBoxButton = System.Windows.MessageBoxButton;
using WpfMessageBoxImage = System.Windows.MessageBoxImage;


namespace TDL.Configurator.App.Pages;

public partial class WrathPage : WpfUserControl

{
    private const string IniRelativePath = @"Data\SKSE\Plugins\TDL_StreamPlugin.ini";
    private const string SectionName = "Wrath";
    private const string UiTitle = "TDL Configurator";

    // Диапазоны и default — из TDL_AllRanges.txt (WRATH).
    // Валидация: min/max (step не форсируем в ini).

    private const int DefaultTotalBursts = 6;        // 1..50
    private const double DefaultInterval = 0.40;     // 0.05..2.0
    private const int DefaultRadius = 300;           // 100..2000
    private const int DefaultZOffset = 50;           // 0..500

    private const int DefaultDamageMin = 5;          // 1..100
    private const int DefaultDamageMax = 15;         // 1..100
    private const double DefaultFireMult = 1.0;      // 0..5
    private const double DefaultStormMag = 1.0;      // 0..5
    private const double DefaultFrostSta = 1.0;      // 0..5

    private const double DefaultLevelScale = 0.0;    // 0..0.10
    private const double DefaultLevelCap = 3.0;      // 1..5

    private const int DefaultShakeChance = 0;        // 0..100
    private const double DefaultShakeDuration = 0.0; // 0..1
    private const double DefaultShakeStrength = 0.0; // 0..1

    private static string SafeNow() => DateTime.Now.ToString("HH:mm:ss");

    private static void ShowInfo(string message)
    {
        WpfMessageBox.Show(message, UiTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public WrathPage()
    {
        InitializeComponent();
        SetDefaultsFromTemplate();
        StatusText.Text = "Готово (default).";
    }

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
            WpfMessageBox.Show(
                "Не удалось прочитать settings.json.\n" + ex.Message,
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
        {
            WpfMessageBox.Show(
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

    // ---------- Defaults ----------
    private void SetDefaultsFromTemplate()
    {
        TotalBurstsBox.Text = DefaultTotalBursts.ToString(CultureInfo.InvariantCulture);
        IntervalBox.Text = DefaultInterval.ToString("0.###", CultureInfo.InvariantCulture);
        RadiusBox.Text = DefaultRadius.ToString(CultureInfo.InvariantCulture);
        ZOffsetBox.Text = DefaultZOffset.ToString(CultureInfo.InvariantCulture);

        DamageMinBox.Text = DefaultDamageMin.ToString(CultureInfo.InvariantCulture);
        DamageMaxBox.Text = DefaultDamageMax.ToString(CultureInfo.InvariantCulture);
        FireMultBox.Text = DefaultFireMult.ToString("0.###", CultureInfo.InvariantCulture);
        StormMagBox.Text = DefaultStormMag.ToString("0.###", CultureInfo.InvariantCulture);
        FrostStaBox.Text = DefaultFrostSta.ToString("0.###", CultureInfo.InvariantCulture);

        LevelScaleBox.Text = DefaultLevelScale.ToString("0.###", CultureInfo.InvariantCulture);
        LevelCapBox.Text = DefaultLevelCap.ToString("0.###", CultureInfo.InvariantCulture);

        ShakeChanceBox.Text = DefaultShakeChance.ToString(CultureInfo.InvariantCulture);
        ShakeDurationBox.Text = DefaultShakeDuration.ToString("0.###", CultureInfo.InvariantCulture);
        ShakeStrengthBox.Text = DefaultShakeStrength.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private void SetDefaultForKey(string key)
    {
        switch (key)
        {
            case "TotalBursts": TotalBurstsBox.Text = DefaultTotalBursts.ToString(CultureInfo.InvariantCulture); break;
            case "Interval": IntervalBox.Text = DefaultInterval.ToString("0.###", CultureInfo.InvariantCulture); break;
            case "Radius": RadiusBox.Text = DefaultRadius.ToString(CultureInfo.InvariantCulture); break;
            case "ZOffset": ZOffsetBox.Text = DefaultZOffset.ToString(CultureInfo.InvariantCulture); break;

            case "DamageMin": DamageMinBox.Text = DefaultDamageMin.ToString(CultureInfo.InvariantCulture); break;
            case "DamageMax": DamageMaxBox.Text = DefaultDamageMax.ToString(CultureInfo.InvariantCulture); break;
            case "FireMult": FireMultBox.Text = DefaultFireMult.ToString("0.###", CultureInfo.InvariantCulture); break;
            case "StormMag": StormMagBox.Text = DefaultStormMag.ToString("0.###", CultureInfo.InvariantCulture); break;
            case "FrostSta": FrostStaBox.Text = DefaultFrostSta.ToString("0.###", CultureInfo.InvariantCulture); break;

            case "LevelScale": LevelScaleBox.Text = DefaultLevelScale.ToString("0.###", CultureInfo.InvariantCulture); break;
            case "LevelCap": LevelCapBox.Text = DefaultLevelCap.ToString("0.###", CultureInfo.InvariantCulture); break;

            case "ShakeChance": ShakeChanceBox.Text = DefaultShakeChance.ToString(CultureInfo.InvariantCulture); break;
            case "ShakeDuration": ShakeDurationBox.Text = DefaultShakeDuration.ToString("0.###", CultureInfo.InvariantCulture); break;
            case "ShakeStrength": ShakeStrengthBox.Text = DefaultShakeStrength.ToString("0.###", CultureInfo.InvariantCulture); break;
        }
    }

    private void DefaultsAll_Click(object sender, RoutedEventArgs e)
    {
        SetDefaultsFromTemplate();
        StatusText.Text = $"Сброшено на default ({SafeNow()}).";
        ShowInfo("Сброшено на значения по умолчанию.");
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

    // ---------- Load / Save ----------
    private void Load_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetIniPath(out var iniPath))
            return;

        if (!File.Exists(iniPath))
        {
            WpfMessageBox.Show(
                $"INI не найден:\n{iniPath}\n\nСоздай его на вкладке Quick access (кнопка «Создать INI (шаблон)»).",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var map = ReadSection(iniPath, SectionName);

        // если ключа нет в ini — оставляем текущее (обычно default)
        TotalBurstsBox.Text = GetOr(map, "TotalBursts", TotalBurstsBox.Text);
        IntervalBox.Text = GetOr(map, "Interval", IntervalBox.Text);
        RadiusBox.Text = GetOr(map, "Radius", RadiusBox.Text);
        ZOffsetBox.Text = GetOr(map, "ZOffset", ZOffsetBox.Text);

        DamageMinBox.Text = GetOr(map, "DamageMin", DamageMinBox.Text);
        DamageMaxBox.Text = GetOr(map, "DamageMax", DamageMaxBox.Text);
        FireMultBox.Text = GetOr(map, "FireMult", FireMultBox.Text);
        StormMagBox.Text = GetOr(map, "StormMag", StormMagBox.Text);
        FrostStaBox.Text = GetOr(map, "FrostSta", FrostStaBox.Text);

        LevelScaleBox.Text = GetOr(map, "LevelScale", LevelScaleBox.Text);
        LevelCapBox.Text = GetOr(map, "LevelCap", LevelCapBox.Text);

        ShakeChanceBox.Text = GetOr(map, "ShakeChance", ShakeChanceBox.Text);
        ShakeDurationBox.Text = GetOr(map, "ShakeDuration", ShakeDurationBox.Text);
        ShakeStrengthBox.Text = GetOr(map, "ShakeStrength", ShakeStrengthBox.Text);

        StatusText.Text = $"Загружено ({SafeNow()}).";
        ShowInfo("Успешно загружено.");
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetIniPath(out var iniPath))
            return;

        if (!TryGetInt(TotalBurstsBox, "TotalBursts", 1, 50, out var totalBursts)) return;
        if (!TryGetDouble(IntervalBox, "Interval", 0.05, 2.0, out var interval)) return;
        if (!TryGetInt(RadiusBox, "Radius", 100, 2000, out var radius)) return;
        if (!TryGetInt(ZOffsetBox, "ZOffset", 0, 500, out var zOffset)) return;

        if (!TryGetInt(DamageMinBox, "DamageMin", 1, 100, out var damageMin)) return;
        if (!TryGetInt(DamageMaxBox, "DamageMax", 1, 100, out var damageMax)) return;

        if (damageMin > damageMax)
        {
            WpfMessageBox.Show(
                "DamageMin не может быть больше DamageMax.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (!TryGetDouble(FireMultBox, "FireMult", 0.0, 5.0, out var fireMult)) return;
        if (!TryGetDouble(StormMagBox, "StormMag", 0.0, 5.0, out var stormMag)) return;
        if (!TryGetDouble(FrostStaBox, "FrostSta", 0.0, 5.0, out var frostSta)) return;

        if (!TryGetDouble(LevelScaleBox, "LevelScale", 0.0, 0.10, out var levelScale)) return;
        if (!TryGetDouble(LevelCapBox, "LevelCap", 1.0, 5.0, out var levelCap)) return;

        if (!TryGetInt(ShakeChanceBox, "ShakeChance", 0, 100, out var shakeChance)) return;
        if (!TryGetDouble(ShakeDurationBox, "ShakeDuration", 0.0, 1.0, out var shakeDuration)) return;
        if (!TryGetDouble(ShakeStrengthBox, "ShakeStrength", 0.0, 1.0, out var shakeStrength)) return;

        var kv = new List<string>
        {
            $"TotalBursts={totalBursts}",
            $"Interval={interval.ToString("0.###", CultureInfo.InvariantCulture)}",
            $"Radius={radius}",
            $"ZOffset={zOffset}",

            $"DamageMin={damageMin}",
            $"DamageMax={damageMax}",
            $"FireMult={fireMult.ToString("0.###", CultureInfo.InvariantCulture)}",
            $"StormMag={stormMag.ToString("0.###", CultureInfo.InvariantCulture)}",
            $"FrostSta={frostSta.ToString("0.###", CultureInfo.InvariantCulture)}",

            $"LevelScale={levelScale.ToString("0.###", CultureInfo.InvariantCulture)}",
            $"LevelCap={levelCap.ToString("0.###", CultureInfo.InvariantCulture)}",

            $"ShakeChance={shakeChance}",
            $"ShakeDuration={shakeDuration.ToString("0.###", CultureInfo.InvariantCulture)}",
            $"ShakeStrength={shakeStrength.ToString("0.###", CultureInfo.InvariantCulture)}",
        };

        UpsertSection(iniPath, SectionName, kv);

        StatusText.Text = $"Сохранено ({SafeNow()}).";
        ShowInfo("Успешно сохранено.");
    }

    // ---------- INI helpers ----------
    private static string GetOr(Dictionary<string, string> map, string key, string fallback)
        => map.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : fallback;

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
            if (t.Equals(wanted, StringComparison.OrdinalIgnoreCase))
            {
                start = i;
                end = i + 1;
                while (end < lines.Count && !IsSectionHeader(lines[end]))
                    end++;
                break;
            }
        }

        var newBlock = new List<string> { wanted };
        newBlock.AddRange(keyValueLines);
        newBlock.Add("");

        if (start < 0)
        {
            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                lines.Add("");

            lines.AddRange(newBlock);
        }
        else
        {
            lines.RemoveRange(start, end - start);
            lines.InsertRange(start, newBlock);
        }

        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllLines(filePath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    // ---------- Parsing helpers ----------
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

    private static bool TryGetInt(WpfTextBox box, string name, int min, int max, out int value)
    {
        value = 0;
        var text = (box.Text ?? "").Trim();

        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) &&
            !int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
        {
            WpfMessageBox.Show(
                $"{name} должен быть целым числом.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            WpfMessageBox.Show(
                $"{name}: значение должно быть в диапазоне {min}..{max}.",
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
        var text = (box.Text ?? "").Trim();

        if (!TryParseDoubleFlexible(text, out value))
        {
            WpfMessageBox.Show(
                $"{name} должен быть числом (пример: 0.40).",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            WpfMessageBox.Show(
                $"{name}: значение должно быть в диапазоне {min}..{max}.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }
}
