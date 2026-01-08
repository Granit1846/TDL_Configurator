using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using TDL.Configurator.Core;

namespace TDL.Configurator.App.Pages;

public partial class InventoryPage : IniPageBase

{
    public InventoryPage()
    {
        InitializeComponent();
        ApplyDefaultsToUi(); // чтобы поля не были пустые при первом открытии
    }

    private string GamePath => AppSettings.Load().GamePath.Trim();
    private string IniPath => Path.Combine(GamePath, "Data", "SKSE", "Plugins", "TDL_StreamPlugin.ini");

    private const string SectionName = "Inventory";

    // Дефолты из шаблона, который создаёт QuickAccessPage. :contentReference[oaicite:2]{index=2}
    private readonly Dictionary<string, string> _defaults = new()
    {
        ["ScatterExactCount"] = "0",
        ["ScatterMinCount"] = "150",
        ["ScatterMaxCount"] = "200",
        ["ScatterRadius"] = "800",
        ["DropBatchSize"] = "10",
        ["DropInterval"] = "0.20",
        ["DropTimeout"] = "30",
        ["ProtectTokensByName"] = "1",
        ["DropShowProgress"] = "0",
    };

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
            System.Windows.MessageBox.Show(
                $"INI не найден:\n{IniPath}\n\nОставляю значения по умолчанию (можно нажать «Сохранить» чтобы создать секцию).",
                "Inventory",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            ApplyDefaultsToUi();
            return;
        }

        var map = ReadSection(IniPath, SectionName);
        if (map.Count == 0)
        {
            ApplyDefaultsToUi();
            return;
        }

        SetText(ScatterExactCountBox, GetOrDefault(map, "ScatterExactCount"));
        SetText(ScatterMinCountBox, GetOrDefault(map, "ScatterMinCount"));
        SetText(ScatterMaxCountBox, GetOrDefault(map, "ScatterMaxCount"));
        SetText(ScatterRadiusBox, GetOrDefault(map, "ScatterRadius"));

        SetText(DropBatchSizeBox, GetOrDefault(map, "DropBatchSize"));
        SetText(DropIntervalBox, GetOrDefault(map, "DropInterval"));
        SetText(DropTimeoutBox, GetOrDefault(map, "DropTimeout"));

        ProtectTokensByNameCheck.IsChecked = GetOrDefault(map, "ProtectTokensByName") != "0";
        DropShowProgressCheck.IsChecked = GetOrDefault(map, "DropShowProgress") != "0";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath()) return;

        if (!TryBuildValues(out var values))
            return;

        Directory.CreateDirectory(Path.GetDirectoryName(IniPath)!);

        var lines = IniFile.LoadLines(IniPath);
        IniFile.UpsertSection(lines, SectionName, values);
        IniFile.SaveLines(IniPath, lines);

        ShowSaved("Inventory");
    }


    private void DefaultsAll_Click(object sender, RoutedEventArgs e)
        => ApplyDefaultsToUi();

    private void DefaultRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button b) return;
        if (b.Tag is not string key) return;

        if (!_defaults.TryGetValue(key, out var def))
            return;

        ApplyOneDefaultToUi(key, def);
    }

    private void ApplyDefaultsToUi()
    {
        foreach (var kv in _defaults)
            ApplyOneDefaultToUi(kv.Key, kv.Value);
    }

    private void ApplyOneDefaultToUi(string key, string value)
    {
        switch (key)
        {
            case "ScatterExactCount": SetText(ScatterExactCountBox, value); break;
            case "ScatterMinCount": SetText(ScatterMinCountBox, value); break;
            case "ScatterMaxCount": SetText(ScatterMaxCountBox, value); break;
            case "ScatterRadius": SetText(ScatterRadiusBox, value); break;

            case "DropBatchSize": SetText(DropBatchSizeBox, value); break;
            case "DropInterval": SetText(DropIntervalBox, value); break;
            case "DropTimeout": SetText(DropTimeoutBox, value); break;

            case "ProtectTokensByName": ProtectTokensByNameCheck.IsChecked = value != "0"; break;
            case "DropShowProgress": DropShowProgressCheck.IsChecked = value != "0"; break;
        }
    }

    private static void SetText(System.Windows.Controls.TextBox tb, string v)
        => tb.Text = (v ?? "").Trim();

    private string GetOrDefault(Dictionary<string, string> map, string key)
        => map.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : _defaults[key];

    private bool TryBuildValues(out Dictionary<string, string> values)
    {
        values = new Dictionary<string, string>();

        // Int
        if (!TryParseIntBox(ScatterExactCountBox, min: 0, max: 5000, out var scatterExact)) return false;
        if (!TryParseIntBox(ScatterMinCountBox, min: 0, max: 5000, out var scatterMin)) return false;
        if (!TryParseIntBox(ScatterMaxCountBox, min: 0, max: 5000, out var scatterMax)) return false;
        if (!TryParseIntBox(ScatterRadiusBox, min: 0, max: 20000, out var scatterRadius)) return false;

        if (scatterExact == 0 && scatterMin > scatterMax)
        {
            System.Windows.MessageBox.Show("Минимум предметов не может быть больше максимума (когда точное количество = 0).",
                "Inventory", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!TryParseIntBox(DropBatchSizeBox, min: 1, max: 200, out var dropBatch)) return false;
        if (!TryParseIntBox(DropTimeoutBox, min: 1, max: 600, out var dropTimeout)) return false;

        // Double
        if (!TryParseDoubleBox(DropIntervalBox, min: 0.01, max: 10.0, out var dropInterval)) return false;

        values["ScatterExactCount"] = scatterExact.ToString(CultureInfo.InvariantCulture);
        values["ScatterMinCount"] = scatterMin.ToString(CultureInfo.InvariantCulture);
        values["ScatterMaxCount"] = scatterMax.ToString(CultureInfo.InvariantCulture);
        values["ScatterRadius"] = scatterRadius.ToString(CultureInfo.InvariantCulture);

        values["DropBatchSize"] = dropBatch.ToString(CultureInfo.InvariantCulture);
        values["DropInterval"] = dropInterval.ToString("0.##", CultureInfo.InvariantCulture);
        values["DropTimeout"] = dropTimeout.ToString(CultureInfo.InvariantCulture);

        values["ProtectTokensByName"] = (ProtectTokensByNameCheck.IsChecked == true) ? "1" : "0";
        values["DropShowProgress"] = (DropShowProgressCheck.IsChecked == true) ? "1" : "0";

        return true;
    }

    private static bool TryParseIntBox(System.Windows.Controls.TextBox tb, int min, int max, out int value)
    {
        value = 0;
        var s = (tb.Text ?? "").Trim();

        if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) &&
            !int.TryParse(s, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
        {
            System.Windows.MessageBox.Show("Поле должно быть целым числом.", "Inventory",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (value < min) value = min;
        if (value > max) value = max;

        tb.Text = value.ToString(CultureInfo.InvariantCulture);
        return true;
    }

    private static bool TryParseDoubleBox(System.Windows.Controls.TextBox tb, double min, double max, out double value)
    {
        value = 0;
        var s = (tb.Text ?? "").Trim();

        if (!TryParseDoubleFlexible(s, out value))
        {
            System.Windows.MessageBox.Show("Поле должно быть числом (пример: 0.20).", "Inventory",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (value < min) value = min;
        if (value > max) value = max;

        tb.Text = value.ToString("0.##", CultureInfo.InvariantCulture);
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

    private static Dictionary<string, string> ReadSection(string path, string sectionName)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(path))
            return result;

        var lines = File.ReadAllLines(path, Encoding.UTF8);
        var inSection = false;

        foreach (var raw in lines)
        {
            var line = raw.Trim();
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

            var key = line[..eq].Trim();
            var val = line[(eq + 1)..].Trim();
            if (key.Length == 0) continue;

            result[key] = val;
        }

        return result;
    }

    private static void UpsertSection(List<string> lines, string sectionName, Dictionary<string, string> values)
    {
        var header = $"[{sectionName}]";
        var start = lines.FindIndex(l => string.Equals(l.Trim(), header, StringComparison.OrdinalIgnoreCase));

        var newSection = new List<string> { header };
        foreach (var kv in values)
            newSection.Add($"{kv.Key}={kv.Value}");

        if (start < 0)
        {
            if (lines.Count > 0 && lines[^1].Trim().Length != 0)
                lines.Add("");
            lines.AddRange(newSection);
            return;
        }

        var end = start + 1;
        while (end < lines.Count)
        {
            var t = lines[end].Trim();
            if (t.StartsWith("[") && t.EndsWith("]"))
                break;
            end++;
        }

        lines.RemoveRange(start, end - start);
        lines.InsertRange(start, newSection);
    }
}
