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

public partial class InventoryPage : System.Windows.Controls.UserControl
{
    private const string IniRelativePath = @"Data\SKSE\Plugins\TDL_StreamPlugin.ini";
    private const string ToolsRelativePath = @"Data\TDL\Tools\tdl_send.exe";

    private const string SectionName = "Inventory";
    private const string UiTitle = "TDL Configurator";

    // TDL_AllRanges.txt → INVENTORY
    private const int DefaultScatterExactCount = 0;  // 0..2000
    private const int DefaultScatterMinCount = 150;  // 1..2000
    private const int DefaultScatterMaxCount = 200;  // 1..2000
    private const int DefaultScatterRadius = 800;    // 100..5000

    private const int DefaultDropBatchSize = 10;     // 1..100
    private const double DefaultDropInterval = 0.20; // 0.05..1.0
    private const int DefaultDropTimeout = 30;       // 5..120

    private const bool DefaultProtectTokensByName = true;
    private const bool DefaultDropShowProgress = false;

    public InventoryPage()
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
            InventoryStatusText.Text = "Путь к игре не задан (default).";
            return;
        }

        if (!File.Exists(iniPath))
        {
            InventoryStatusText.Text = "INI не найден (default).";
            return;
        }

        var map = ReadSection(iniPath, SectionName);
        ScatterExactCountBox.Text = GetOr(map, "ScatterExactCount", ScatterExactCountBox.Text);
        ScatterMinCountBox.Text = GetOr(map, "ScatterMinCount", ScatterMinCountBox.Text);
        ScatterMaxCountBox.Text = GetOr(map, "ScatterMaxCount", ScatterMaxCountBox.Text);
        ScatterRadiusBox.Text = GetOr(map, "ScatterRadius", ScatterRadiusBox.Text);

        DropBatchSizeBox.Text = GetOr(map, "DropBatchSize", DropBatchSizeBox.Text);
        DropIntervalBox.Text = GetOr(map, "DropInterval", DropIntervalBox.Text);
        DropTimeoutBox.Text = GetOr(map, "DropTimeout", DropTimeoutBox.Text);

        ProtectTokensByNameCheck.IsChecked = GetOr(map, "ProtectTokensByName", DefaultProtectTokensByName ? "1" : "0") != "0";
        DropShowProgressCheck.IsChecked = GetOr(map, "DropShowProgress", DefaultDropShowProgress ? "1" : "0") != "0";

        InventoryStatusText.Text = $"Загружено из INI ({SafeNow()}).";
    }

    private void SaveApply_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetIniPath(out var iniPath))
            return;

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

        if (!TryGetInt(DropBatchSizeBox, "DropBatchSize", 1, 100, out var dropBatch)) return;
        if (!TryGetDouble(DropIntervalBox, "DropInterval", 0.05, 1.0, out var dropInterval)) return;
        if (!TryGetInt(DropTimeoutBox, "DropTimeout", 5, 120, out var dropTimeout)) return;

        var protectTokens = ProtectTokensByNameCheck.IsChecked == true ? "1" : "0";
        var showProgress = DropShowProgressCheck.IsChecked == true ? "1" : "0";

        var kv = new List<string>
        {
            $"ScatterExactCount={scatterExact}",
            $"ScatterMinCount={scatterMin}",
            $"ScatterMaxCount={scatterMax}",
            $"ScatterRadius={scatterRadius}",
            $"DropBatchSize={dropBatch}",
            $"DropInterval={dropInterval.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"DropTimeout={dropTimeout}",
            $"ProtectTokensByName={protectTokens}",
            $"DropShowProgress={showProgress}",
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
            InventoryStatusText.Text = $"Сохранено и применено ({SafeNow()}).";
        }
        else
        {
            InventoryStatusText.Text = $"Сохранено, но не применено ({SafeNow()}).";
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
        InventoryStatusText.Text = $"Сброшено на default ({SafeNow()}).";
    }

    private void DefaultRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not WpfButton btn)
            return;

        var key = btn.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(key))
            return;

        SetDefaultForKey(key);
        InventoryStatusText.Text = $"Default: {key} ({SafeNow()}).";
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

        InventoryStatusText.Text = "Готово (default).";
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
