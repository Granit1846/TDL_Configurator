using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using TDL.Configurator.Core;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace TDL.Configurator.App.Pages;

public partial class ChaosPage : System.Windows.Controls.UserControl
{
    private const string IniRelativePath = @"Data\SKSE\Plugins\TDL_StreamPlugin.ini";
    private const string SectionName = "Chaos";
    private const string UiTitle = "TDL Configurator";

    private static void ShowInfo(string message)
    {
        System.Windows.MessageBox.Show(
            message,
            UiTitle,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    public ChaosPage()
    {
        InitializeComponent();
        SetDefaultsFromTemplate();
        StatusText.Text = "Готово (default).";
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
                "TDL Configurator",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
        {
            System.Windows.MessageBox.Show(
                "Путь к игре не задан или неверный.\nОткрой настройки и укажи папку Skyrim Special Edition.",
                "TDL Configurator",
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
        BackfireChanceBox.Text = "20";
        BackfireDurationBox.Text = "60";
        ShoutPushForceBox.Text = "20";
        ShoutPushDelayBox.Text = "0.05";

        KnockbackForceBox.Text = "25";
        KnockbackCooldownBox.Text = "0.35";
        KnockbackRadiusBox.Text = "900";
        KnockbackMeleeDelayBox.Text = "0.10";
        KnockbackBowDelayBox.Text = "0.10";
    }

    private void SetDefaultForKey(string key)
    {
        switch (key)
        {
            case "BackfireChance": BackfireChanceBox.Text = "20"; break;
            case "BackfireDuration": BackfireDurationBox.Text = "60"; break;
            case "ShoutPushForce": ShoutPushForceBox.Text = "20"; break;
            case "ShoutPushDelay": ShoutPushDelayBox.Text = "0.05"; break;

            case "KnockbackForce": KnockbackForceBox.Text = "25"; break;
            case "KnockbackCooldown": KnockbackCooldownBox.Text = "0.35"; break;
            case "KnockbackRadius": KnockbackRadiusBox.Text = "900"; break;
            case "KnockbackMeleeDelay": KnockbackMeleeDelayBox.Text = "0.10"; break;
            case "KnockbackBowDelay": KnockbackBowDelayBox.Text = "0.10"; break;
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
        if (sender is not System.Windows.Controls.Button btn)
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
            System.Windows.MessageBox.Show(
                $"INI не найден:\n{iniPath}\n\nСоздай его на вкладке Quick access (кнопка «Создать INI (шаблон)»).",
                "TDL Configurator",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var map = ReadSection(iniPath, SectionName);

        // если ключа нет в ini — оставляем текущее (обычно default)
        BackfireChanceBox.Text = GetOr(map, "BackfireChance", BackfireChanceBox.Text);
        BackfireDurationBox.Text = GetOr(map, "BackfireDuration", BackfireDurationBox.Text);
        ShoutPushForceBox.Text = GetOr(map, "ShoutPushForce", ShoutPushForceBox.Text);
        ShoutPushDelayBox.Text = GetOr(map, "ShoutPushDelay", ShoutPushDelayBox.Text);

        KnockbackForceBox.Text = GetOr(map, "KnockbackForce", KnockbackForceBox.Text);
        KnockbackCooldownBox.Text = GetOr(map, "KnockbackCooldown", KnockbackCooldownBox.Text);
        KnockbackRadiusBox.Text = GetOr(map, "KnockbackRadius", KnockbackRadiusBox.Text);
        KnockbackMeleeDelayBox.Text = GetOr(map, "KnockbackMeleeDelay", KnockbackMeleeDelayBox.Text);
        KnockbackBowDelayBox.Text = GetOr(map, "KnockbackBowDelay", KnockbackBowDelayBox.Text);

        StatusText.Text = $"Загружено ({SafeNow()}).";
        ShowInfo("Успешно загружено.");

    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetIniPath(out var iniPath))
            return;

        // Валидация диапазонов (как договорились)
        if (!TryGetInt(BackfireChanceBox, "BackfireChance", 0, 100, out var backfireChance)) return;
        if (!TryGetInt(BackfireDurationBox, "BackfireDuration", 1, 600, out var backfireDuration)) return;
        if (!TryGetInt(ShoutPushForceBox, "ShoutPushForce", 0, 200, out var shoutPushForce)) return;
        if (!TryGetDouble(ShoutPushDelayBox, "ShoutPushDelay", 0.0, 0.5, out var shoutPushDelay)) return;

        if (!TryGetInt(KnockbackForceBox, "KnockbackForce", 0, 200, out var knockbackForce)) return;
        if (!TryGetDouble(KnockbackCooldownBox, "KnockbackCooldown", 0.0, 2.0, out var knockbackCooldown)) return;
        if (!TryGetInt(KnockbackRadiusBox, "KnockbackRadius", 0, 20000, out var knockbackRadius)) return;
        if (!TryGetDouble(KnockbackMeleeDelayBox, "KnockbackMeleeDelay", 0.0, 0.5, out var knockbackMeleeDelay)) return;
        if (!TryGetDouble(KnockbackBowDelayBox, "KnockbackBowDelay", 0.0, 0.5, out var knockbackBowDelay)) return;

        var kv = new List<string>
        {
            $"BackfireChance={backfireChance}",
            $"BackfireDuration={backfireDuration}",
            $"ShoutPushForce={shoutPushForce}",
            $"ShoutPushDelay={shoutPushDelay.ToString("0.##", CultureInfo.InvariantCulture)}",

            $"KnockbackForce={knockbackForce}",
            $"KnockbackCooldown={knockbackCooldown.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"KnockbackRadius={knockbackRadius}",
            $"KnockbackMeleeDelay={knockbackMeleeDelay.ToString("0.##", CultureInfo.InvariantCulture)}",
            $"KnockbackBowDelay={knockbackBowDelay.ToString("0.##", CultureInfo.InvariantCulture)}",
        };

        UpsertSection(iniPath, SectionName, kv);

        StatusText.Text = $"Сохранено ({SafeNow()}).";
        ShowInfo("Успешно сохранено");

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
                "TDL Configurator",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show(
                $"{name}: значение должно быть в диапазоне {min}..{max}.",
                "TDL Configurator",
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
                $"{name} должен быть числом (пример: 0.35).",
                "TDL Configurator",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show(
                $"{name}: значение должно быть в диапазоне {min}..{max}.",
                "TDL Configurator",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }
}
