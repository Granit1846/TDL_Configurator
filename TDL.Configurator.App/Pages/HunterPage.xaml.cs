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

public partial class HunterPage : System.Windows.Controls.UserControl
{
    private const string IniRelativePath = @"Data\SKSE\Plugins\TDL_StreamPlugin.ini";
    private const string SectionName = "Hunter";
    private const string UiTitle = "TDL Configurator";

    // Defaults + диапазоны из /mnt/data/TDL_AllRanges.txt (секция HUNTER)
    private const int DefaultCorpseTime = 20;     // 0..300
    private const int DefaultDuration = 90;       // 5..600
    private const int DefaultMaxDistance = 5500;  // 1500..10000
    private const double DefaultReAggro = 4.0;    // 1.0..10.0
    private const int DefaultSpawnOffset = 1200;  // 300..3000

    private static void ShowInfo(string message)
    {
        System.Windows.MessageBox.Show(
            message,
            UiTitle,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    public HunterPage()
    {
        InitializeComponent();
        ApplyDefaultsToUi();
        HunterStatusText.Text = "Готово (default).";
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
        if (!TryGetIniPath(out var iniPath))
            return;

        if (!File.Exists(iniPath))
        {
            System.Windows.MessageBox.Show(
                $"INI не найден:\n{iniPath}\n\nСоздай его на вкладке Quick access (кнопка «Создать INI (шаблон)»).",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var map = ReadSection(iniPath, SectionName);

        // если ключа нет — оставляем текущее (обычно default)
        CorpseTimeBox.Text = GetOr(map, "CorpseTime", CorpseTimeBox.Text);
        DurationBox.Text = GetOr(map, "Duration", DurationBox.Text);
        MaxDistanceBox.Text = GetOr(map, "MaxDistance", MaxDistanceBox.Text);
        ReAggroBox.Text = GetOr(map, "ReAggro", ReAggroBox.Text);
        SpawnOffsetBox.Text = GetOr(map, "SpawnOffset", SpawnOffsetBox.Text);

        HunterStatusText.Text = $"Загружено ({SafeNow()}).";
        ShowInfo("Успешно загружено.");
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetIniPath(out var iniPath))
            return;

        // Валидация диапазонов (TDL_AllRanges.txt → HUNTER)
        if (!TryGetInt(CorpseTimeBox, "CorpseTime", 0, 300, out var corpseTime)) return;
        if (!TryGetInt(DurationBox, "Duration", 5, 600, out var duration)) return;
        if (!TryGetInt(MaxDistanceBox, "MaxDistance", 1500, 10000, out var maxDistance)) return;
        if (!TryGetDouble(ReAggroBox, "ReAggro", 1.0, 10.0, out var reAggro)) return;
        if (!TryGetInt(SpawnOffsetBox, "SpawnOffset", 300, 3000, out var spawnOffset)) return;

        var kv = new List<string>
        {
            $"CorpseTime={corpseTime}",
            $"Duration={duration}",
            $"MaxDistance={maxDistance}",
            $"ReAggro={reAggro.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"SpawnOffset={spawnOffset}",
        };

        UpsertSection(iniPath, SectionName, kv);

        HunterStatusText.Text = $"Сохранено ({SafeNow()}).";
        ShowInfo("Успешно сохранено");
    }

    private void DefaultsAll_Click(object sender, RoutedEventArgs e)
    {
        ApplyDefaultsToUi();
        HunterStatusText.Text = $"Сброшено на default ({SafeNow()}).";
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
        HunterStatusText.Text = $"Default: {key} ({SafeNow()}).";
    }

    private void ApplyDefaultsToUi()
    {
        CorpseTimeBox.Text = DefaultCorpseTime.ToString(CultureInfo.InvariantCulture);
        DurationBox.Text = DefaultDuration.ToString(CultureInfo.InvariantCulture);
        MaxDistanceBox.Text = DefaultMaxDistance.ToString(CultureInfo.InvariantCulture);
        ReAggroBox.Text = DefaultReAggro.ToString("0.##", CultureInfo.InvariantCulture);
        SpawnOffsetBox.Text = DefaultSpawnOffset.ToString(CultureInfo.InvariantCulture);
    }

    private void SetDefaultForKey(string key)
    {
        switch (key)
        {
            case "CorpseTime": CorpseTimeBox.Text = DefaultCorpseTime.ToString(CultureInfo.InvariantCulture); break;
            case "Duration": DurationBox.Text = DefaultDuration.ToString(CultureInfo.InvariantCulture); break;
            case "MaxDistance": MaxDistanceBox.Text = DefaultMaxDistance.ToString(CultureInfo.InvariantCulture); break;
            case "ReAggro": ReAggroBox.Text = DefaultReAggro.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "SpawnOffset": SpawnOffsetBox.Text = DefaultSpawnOffset.ToString(CultureInfo.InvariantCulture); break;
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
            if (t.Equals(wanted, StringComparison.OrdinalIgnoreCase))
            {
                start = i;
                end = i + 1;
                while (end < lines.Count && !IsSectionHeader(lines[end]))
                    end++;
                break;
            }
        }

        var newBlock = new List<string>();
        newBlock.Add(wanted);
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
            System.Windows.MessageBox.Show(
                $"{name} должен быть целым числом.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show(
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
            System.Windows.MessageBox.Show(
                $"{name} должен быть числом (пример: 4.0).",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show(
                $"{name}: значение должно быть в диапазоне {min}..{max}.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }
}
