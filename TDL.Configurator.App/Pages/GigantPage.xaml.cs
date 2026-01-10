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

public partial class GigantPage : System.Windows.Controls.UserControl
{
    private const string IniRelativePath = @"Data\SKSE\Plugins\TDL_StreamPlugin.ini";
    private const string ToolsRelativePath = @"Data\TDL\Tools\tdl_send.exe";

    private const string SectionName = "Gigant";
    private const string UiTitle = "TDL Configurator";

    // TDL_AllRanges.txt → GIGANT
    private const int DefaultSizeDuration = 60;   // 5..600
    private const int DefaultSpeedDuration = 60;  // 5..600

    private const double DefaultScaleBig = 2.0;   // 0.1..5.0
    private const double DefaultDamageBig = 5.0;  // 0.0..10.0

    private const double DefaultScaleSmall = 0.33; // 0.1..1.0
    private const double DefaultDamageSmall = 0.5; // 0.0..1.0

    private const double DefaultSpeedFast = 3.0;   // 1.0..10.0
    private const double DefaultSpeedSlow = 0.5;   // 0.1..1.0

    public GigantPage()
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
            GigantStatusText.Text = "Путь к игре не задан (default).";
            return;
        }

        if (!File.Exists(iniPath))
        {
            GigantStatusText.Text = "INI не найден (default).";
            return;
        }

        var map = ReadSection(iniPath, SectionName);

        SizeDurationBox.Text = GetOr(map, "SizeDuration", SizeDurationBox.Text);
        SpeedDurationBox.Text = GetOr(map, "SpeedDuration", SpeedDurationBox.Text);

        ScaleBigBox.Text = GetOr(map, "ScaleBig", ScaleBigBox.Text);
        DamageBigBox.Text = GetOr(map, "DamageBig", DamageBigBox.Text);

        ScaleSmallBox.Text = GetOr(map, "ScaleSmall", ScaleSmallBox.Text);
        DamageSmallBox.Text = GetOr(map, "DamageSmall", DamageSmallBox.Text);

        SpeedFastBox.Text = GetOr(map, "SpeedFast", SpeedFastBox.Text);
        SpeedSlowBox.Text = GetOr(map, "SpeedSlow", SpeedSlowBox.Text);

        GigantStatusText.Text = $"Загружено из INI ({SafeNow()}).";
    }

    private void SaveApply_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetIniPath(out var iniPath))
            return;

        if (!TryGetInt(SizeDurationBox, "SizeDuration", 5, 600, out var sizeDuration)) return;
        if (!TryGetInt(SpeedDurationBox, "SpeedDuration", 5, 600, out var speedDuration)) return;

        if (!TryGetDouble(ScaleBigBox, "ScaleBig", 0.1, 5.0, out var scaleBig)) return;
        if (!TryGetDouble(DamageBigBox, "DamageBig", 0.0, 10.0, out var damageBig)) return;

        if (!TryGetDouble(ScaleSmallBox, "ScaleSmall", 0.1, 1.0, out var scaleSmall)) return;
        if (!TryGetDouble(DamageSmallBox, "DamageSmall", 0.0, 1.0, out var damageSmall)) return;

        if (!TryGetDouble(SpeedFastBox, "SpeedFast", 1.0, 10.0, out var speedFast)) return;
        if (!TryGetDouble(SpeedSlowBox, "SpeedSlow", 0.1, 1.0, out var speedSlow)) return;

        var kv = new List<string>
        {
            $"SizeDuration={sizeDuration}",
            $"SpeedDuration={speedDuration}",
            $"ScaleBig={scaleBig.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"DamageBig={damageBig.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"ScaleSmall={scaleSmall.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"DamageSmall={damageSmall.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"SpeedFast={speedFast.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"SpeedSlow={speedSlow.ToString("0.##", CultureInfo.InvariantCulture)}",
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
            GigantStatusText.Text = $"Сохранено и применено ({SafeNow()}).";
        }
        else
        {
            GigantStatusText.Text = $"Сохранено, но не применено ({SafeNow()}).";
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
        GigantStatusText.Text = $"Сброшено на default ({SafeNow()}).";
    }

    private void DefaultRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not WpfButton btn)
            return;

        var key = btn.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(key))
            return;

        SetDefaultForKey(key);
        GigantStatusText.Text = $"Default: {key} ({SafeNow()}).";
    }

    private void ApplyDefaultsToUi()
    {
        SizeDurationBox.Text = DefaultSizeDuration.ToString(CultureInfo.InvariantCulture);
        SpeedDurationBox.Text = DefaultSpeedDuration.ToString(CultureInfo.InvariantCulture);

        ScaleBigBox.Text = DefaultScaleBig.ToString("0.##", CultureInfo.InvariantCulture);
        DamageBigBox.Text = DefaultDamageBig.ToString("0.##", CultureInfo.InvariantCulture);

        ScaleSmallBox.Text = DefaultScaleSmall.ToString("0.##", CultureInfo.InvariantCulture);
        DamageSmallBox.Text = DefaultDamageSmall.ToString("0.##", CultureInfo.InvariantCulture);

        SpeedFastBox.Text = DefaultSpeedFast.ToString("0.##", CultureInfo.InvariantCulture);
        SpeedSlowBox.Text = DefaultSpeedSlow.ToString("0.##", CultureInfo.InvariantCulture);

        GigantStatusText.Text = "Готово (default).";
    }

    private void SetDefaultForKey(string key)
    {
        switch (key)
        {
            case "SizeDuration": SizeDurationBox.Text = DefaultSizeDuration.ToString(CultureInfo.InvariantCulture); break;
            case "SpeedDuration": SpeedDurationBox.Text = DefaultSpeedDuration.ToString(CultureInfo.InvariantCulture); break;
            case "ScaleBig": ScaleBigBox.Text = DefaultScaleBig.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "DamageBig": DamageBigBox.Text = DefaultDamageBig.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "ScaleSmall": ScaleSmallBox.Text = DefaultScaleSmall.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "DamageSmall": DamageSmallBox.Text = DefaultDamageSmall.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "SpeedFast": SpeedFastBox.Text = DefaultSpeedFast.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "SpeedSlow": SpeedSlowBox.Text = DefaultSpeedSlow.ToString("0.##", CultureInfo.InvariantCulture); break;
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
