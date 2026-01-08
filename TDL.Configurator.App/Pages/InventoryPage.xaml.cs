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

public partial class InventoryPage : System.Windows.Controls.UserControl
{
    private const string SectionName = "Inventory";
    private const string UiTitle = "TDL Configurator";

    private static void ShowInfo(string message)
    {
        System.Windows.MessageBox.Show(
            message,
            UiTitle,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    // Default значения (как у тебя в шаблоне INI)
    private const int DefaultScatterExactCount = 0;
    private const int DefaultScatterMinCount = 150;
    private const int DefaultScatterMaxCount = 200;
    private const int DefaultScatterRadius = 800;

    private const int DefaultDropBatchSize = 10;
    private const double DefaultDropInterval = 0.20;
    private const int DefaultDropTimeout = 30;

    private const bool DefaultProtectTokensByName = true;
    private const bool DefaultDropShowProgress = false;

    public InventoryPage()
    {
        InitializeComponent();
        ApplyDefaultsToUi();
        InventoryStatusText.Text = "Готово (default).";
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
            InventoryStatusText.Text = "INI не найден (default).";
            return;
        }

        var map = ReadSection(IniPath, SectionName);
        if (map.Count == 0)
        {
            ApplyDefaultsToUi();
            InventoryStatusText.Text = "Секция [Inventory] не найдена (default).";
            return;
        }

        ScatterExactCountBox.Text = GetOr(map, "ScatterExactCount", DefaultScatterExactCount.ToString(CultureInfo.InvariantCulture));
        ScatterMinCountBox.Text = GetOr(map, "ScatterMinCount", DefaultScatterMinCount.ToString(CultureInfo.InvariantCulture));
        ScatterMaxCountBox.Text = GetOr(map, "ScatterMaxCount", DefaultScatterMaxCount.ToString(CultureInfo.InvariantCulture));
        ScatterRadiusBox.Text = GetOr(map, "ScatterRadius", DefaultScatterRadius.ToString(CultureInfo.InvariantCulture));

        DropBatchSizeBox.Text = GetOr(map, "DropBatchSize", DefaultDropBatchSize.ToString(CultureInfo.InvariantCulture));
        DropIntervalBox.Text = GetOr(map, "DropInterval", DefaultDropInterval.ToString("0.##", CultureInfo.InvariantCulture));
        DropTimeoutBox.Text = GetOr(map, "DropTimeout", DefaultDropTimeout.ToString(CultureInfo.InvariantCulture));

        ProtectTokensByNameCheck.IsChecked = GetOr(map, "ProtectTokensByName", DefaultProtectTokensByName ? "1" : "0") != "0";
        DropShowProgressCheck.IsChecked = GetOr(map, "DropShowProgress", DefaultDropShowProgress ? "1" : "0") != "0";

        InventoryStatusText.Text = $"Загружено: {DateTime.Now:HH:mm:ss}";
        ShowInfo("Успешно загружено.");
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath()) return;

        if (!TryGetInt(ScatterExactCountBox, "ScatterExactCount", 0, 2000, out var scatterExact)) return;
        if (!TryGetInt(ScatterMinCountBox, "ScatterMinCount", 1, 2000, out var scatterMin)) return;
        if (!TryGetInt(ScatterMaxCountBox, "ScatterMaxCount", 1, 2000, out var scatterMax)) return;
        if (!TryGetInt(ScatterRadiusBox, "ScatterRadius", 100, 5000, out var scatterRadius)) return;

        if (scatterExact == 0 && scatterMin > scatterMax)
        {
            System.Windows.MessageBox.Show(
                "ScatterMinCount не может быть больше ScatterMaxCount (когда точное количество = 0).",
                "Inventory",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (!TryGetInt(DropBatchSizeBox, "DropBatchSize", 1, 100, out var dropBatchSize)) return;
        if (!TryGetDouble(DropIntervalBox, "DropInterval", 0.05, 1.0, out var dropInterval)) return;
        if (!TryGetInt(DropTimeoutBox, "DropTimeout", 5, 120, out var dropTimeout)) return;

        var protectTokens = ProtectTokensByNameCheck.IsChecked == true ? 1 : 0;
        var showProgress = DropShowProgressCheck.IsChecked == true ? 1 : 0;

        Directory.CreateDirectory(PluginsFolder);

        var lines = File.Exists(IniPath)
            ? File.ReadAllLines(IniPath, Encoding.UTF8).ToList()
            : new List<string>();

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ScatterExactCount"] = scatterExact.ToString(CultureInfo.InvariantCulture),
            ["ScatterMinCount"] = scatterMin.ToString(CultureInfo.InvariantCulture),
            ["ScatterMaxCount"] = scatterMax.ToString(CultureInfo.InvariantCulture),
            ["ScatterRadius"] = scatterRadius.ToString(CultureInfo.InvariantCulture),

            ["DropBatchSize"] = dropBatchSize.ToString(CultureInfo.InvariantCulture),
            ["DropInterval"] = dropInterval.ToString("0.##", CultureInfo.InvariantCulture),
            ["DropTimeout"] = dropTimeout.ToString(CultureInfo.InvariantCulture),

            ["ProtectTokensByName"] = protectTokens.ToString(CultureInfo.InvariantCulture),
            ["DropShowProgress"] = showProgress.ToString(CultureInfo.InvariantCulture),
        };

        UpsertSection(lines, SectionName, values);

        File.WriteAllLines(IniPath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        InventoryStatusText.Text = $"Сохранено: {DateTime.Now:HH:mm:ss}";
        ShowInfo("Успешно сохранено");
    }

    private void DefaultsAll_Click(object sender, RoutedEventArgs e)
    {
        ApplyDefaultsToUi();
        InventoryStatusText.Text = "Готово (default).";
        ShowInfo("Сброшено на значения по умолчанию.");
    }

    private void DefaultRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not WpfButton btn) return;

        var key = btn.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(key)) return;

        SetDefaultForKey(key);
        InventoryStatusText.Text = $"Default: {key}";
    }

    private void ApplyDefaultsToUi()
    {
        ScatterExactCountBox.Text = DefaultScatterExactCount.ToString(CultureInfo.InvariantCulture);
        ScatterMinCountBox.Text = DefaultScatterMinCount.ToString(CultureInfo.InvariantCulture);
        ScatterMaxCountBox.Text = DefaultScatterMaxCount.ToString(CultureInfo.InvariantCulture);
        ScatterRadiusBox.Text = DefaultScatterRadius.ToString(CultureInfo.InvariantCulture);

        DropBatchSizeBox.Text = DefaultDropBatchSize.ToString(CultureInfo.InvariantCulture);
        DropIntervalBox.Text = DefaultDropInterval.ToString("0.##", CultureInfo.InvariantCulture);
        DropTimeoutBox.Text = DefaultDropTimeout.ToString(CultureInfo.InvariantCulture);

        ProtectTokensByNameCheck.IsChecked = DefaultProtectTokensByName;
        DropShowProgressCheck.IsChecked = DefaultDropShowProgress;
    }

    private void SetDefaultForKey(string key)
    {
        switch (key)
        {
            case "ScatterExactCount": ScatterExactCountBox.Text = DefaultScatterExactCount.ToString(CultureInfo.InvariantCulture); break;
            case "ScatterMinCount": ScatterMinCountBox.Text = DefaultScatterMinCount.ToString(CultureInfo.InvariantCulture); break;
            case "ScatterMaxCount": ScatterMaxCountBox.Text = DefaultScatterMaxCount.ToString(CultureInfo.InvariantCulture); break;
            case "ScatterRadius": ScatterRadiusBox.Text = DefaultScatterRadius.ToString(CultureInfo.InvariantCulture); break;

            case "DropBatchSize": DropBatchSizeBox.Text = DefaultDropBatchSize.ToString(CultureInfo.InvariantCulture); break;
            case "DropInterval": DropIntervalBox.Text = DefaultDropInterval.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "DropTimeout": DropTimeoutBox.Text = DefaultDropTimeout.ToString(CultureInfo.InvariantCulture); break;

            case "ProtectTokensByName": ProtectTokensByNameCheck.IsChecked = DefaultProtectTokensByName; break;
            case "DropShowProgress": DropShowProgressCheck.IsChecked = DefaultDropShowProgress; break;
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

    // ---------------- Parse helpers (диапазоны) ----------------

    private static bool TryGetInt(WpfTextBox box, string name, int min, int max, out int value)
    {
        value = 0;
        var text = (box.Text ?? "").Trim();

        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) &&
            !int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
        {
            System.Windows.MessageBox.Show($"{name} должен быть целым числом.", "Inventory",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show($"{name}: значение должно быть в диапазоне {min}..{max}.", "Inventory",
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
            System.Windows.MessageBox.Show($"{name} должен быть числом (пример: 0.20).", "Inventory",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show($"{name}: значение должно быть в диапазоне {min}..{max}.", "Inventory",
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
