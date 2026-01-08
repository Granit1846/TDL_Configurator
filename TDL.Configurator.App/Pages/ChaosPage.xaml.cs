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
    private const string SectionName = "Chaos";

    public ChaosPage()
    {
        InitializeComponent();
        SetDefaultsFromTemplate();
        SetStatus("Готово (default).");
    }

    // -----------------------
    // Paths / Settings
    // -----------------------
    private string GamePath
    {
        get
        {
            try
            {
                return (AppSettings.Load().GamePath ?? "").Trim();
            }
            catch
            {
                return "";
            }
        }
    }

    private string IniPath =>
        Path.Combine(GamePath, "Data", "SKSE", "Plugins", "TDL_StreamPlugin.ini");

    private string PluginsFolder =>
        Path.Combine(GamePath, "Data", "SKSE", "Plugins");

    private bool EnsureGamePath()
    {
        if (string.IsNullOrWhiteSpace(GamePath) || !Directory.Exists(GamePath))
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

    private void SetStatus(string text)
    {
        if (StatusText != null)
            StatusText.Text = text;
    }

    // -----------------------
    // Defaults
    // -----------------------
    private void SetDefaultsFromTemplate()
    {
        // Дефолты соответствуют твоему шаблону INI
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
        StatusText.Text = "Сброшено на default.";
    }

    private void DefaultRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button btn)
            return;

        var key = btn.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(key))
            return;

        SetDefaultForKey(key);
        StatusText.Text = $"Default: {key}";
    }

    // -----------------------
    // Load / Save
    // -----------------------
    private void Load_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath())
            return;

        if (!File.Exists(IniPath))
        {
            System.Windows.MessageBox.Show(
                $"INI не найден:\n{IniPath}\n\nСоздай его на вкладке Quick access (кнопка «Создать INI (шаблон)»).",
                "TDL Configurator",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var map = ReadSection(IniPath, SectionName);

        BackfireChanceBox.Text = GetOr(map, "BackfireChance", BackfireChanceBox.Text);
        BackfireDurationBox.Text = GetOr(map, "BackfireDuration", BackfireDurationBox.Text);
        ShoutPushForceBox.Text = GetOr(map, "ShoutPushForce", ShoutPushForceBox.Text);
        ShoutPushDelayBox.Text = GetOr(map, "ShoutPushDelay", ShoutPushDelayBox.Text);

        KnockbackForceBox.Text = GetOr(map, "KnockbackForce", KnockbackForceBox.Text);
        KnockbackCooldownBox.Text = GetOr(map, "KnockbackCooldown", KnockbackCooldownBox.Text);
        KnockbackRadiusBox.Text = GetOr(map, "KnockbackRadius", KnockbackRadiusBox.Text);
        KnockbackMeleeDelayBox.Text = GetOr(map, "KnockbackMeleeDelay", KnockbackMeleeDelayBox.Text);
        KnockbackBowDelayBox.Text = GetOr(map, "KnockbackBowDelay", KnockbackBowDelayBox.Text);

        SetStatus($"Загружено: {DateTime.Now:HH:mm:ss}");
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath())
            return;

        Directory.CreateDirectory(PluginsFolder);

        // Валидация + нормализация
        if (!TryGetInt(BackfireChanceBox, "BackfireChance", out var backfireChance)) return;
        if (!TryGetInt(BackfireDurationBox, "BackfireDuration", out var backfireDuration)) return;
        if (!TryGetInt(ShoutPushForceBox, "ShoutPushForce", out var shoutPushForce)) return;
        if (!TryGetDouble(ShoutPushDelayBox, "ShoutPushDelay", out var shoutPushDelay)) return;

        if (!TryGetInt(KnockbackForceBox, "KnockbackForce", out var knockbackForce)) return;
        if (!TryGetDouble(KnockbackCooldownBox, "KnockbackCooldown", out var knockbackCooldown)) return;
        if (!TryGetInt(KnockbackRadiusBox, "KnockbackRadius", out var knockbackRadius)) return;
        if (!TryGetDouble(KnockbackMeleeDelayBox, "KnockbackMeleeDelay", out var knockbackMeleeDelay)) return;
        if (!TryGetDouble(KnockbackBowDelayBox, "KnockbackBowDelay", out var knockbackBowDelay)) return;

        var lines = new List<string>
        {
            $"BackfireChance={backfireChance}",
            $"BackfireDuration={backfireDuration}",
            $"ShoutPushForce={shoutPushForce}",
            $"ShoutPushDelay={FormatDouble(shoutPushDelay)}",

            $"KnockbackForce={knockbackForce}",
            $"KnockbackCooldown={FormatDouble(knockbackCooldown)}",
            $"KnockbackRadius={knockbackRadius}",
            $"KnockbackMeleeDelay={FormatDouble(knockbackMeleeDelay)}",
            $"KnockbackBowDelay={FormatDouble(knockbackBowDelay)}",
        };

        UpsertSection(IniPath, SectionName, lines);

        SetStatus($"Сохранено: {DateTime.Now:HH:mm:ss}");
    }

    // -----------------------
    // INI helpers (local)
    // -----------------------
    private static string GetOr(Dictionary<string, string> map, string key, string fallback)
        => map.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : fallback;

    private static bool IsSectionHeader(string line)
    {
        var t = (line ?? "").Trim();
        return t.StartsWith("[") && t.EndsWith("]") && t.Length >= 3;
    }

    private static Dictionary<string, string> ReadSection(string filePath, string sectionName)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        var wanted = $"[{sectionName}]";
        var inSection = false;

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

        int start = -1;
        int end = -1;

        for (int i = 0; i < lines.Count; i++)
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

        // пустая строка после секции для читаемости
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

        File.WriteAllLines(filePath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    // -----------------------
    // Parsing helpers
    // -----------------------
    private static string FormatDouble(double value)
        => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static bool TryParseDoubleFlexible(string s, out double value)
    {
        s = (s ?? "").Trim();

        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return true;

        if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
            return true;

        // попытка заменить разделитель
        var swapped = s.Contains(',') ? s.Replace(',', '.') : s.Replace('.', ',');
        if (double.TryParse(swapped, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return true;

        value = 0;
        return false;
    }

    private static bool TryGetInt(WpfTextBox box, string name, out int value)
    {
        value = 0;
        var text = (box.Text ?? "").Trim();

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ||
            int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
            return true;

        System.Windows.MessageBox.Show(
            $"{name} должен быть целым числом.",
            "TDL Configurator",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);

        box.Focus();
        return false;
    }

    private static bool TryGetDouble(WpfTextBox box, string name, out double value)
    {
        value = 0;
        var text = (box.Text ?? "").Trim();

        if (TryParseDoubleFlexible(text, out value))
            return true;

        System.Windows.MessageBox.Show(
            $"{name} должен быть числом (пример: 0.35).",
            "TDL Configurator",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);

        box.Focus();
        return false;
    }
}
