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
using TDL.Configurator.App.Services;
using TDL.Configurator.Core;

using WpfButton = System.Windows.Controls.Button;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace TDL.Configurator.App.Pages;

public partial class ChaosPage : System.Windows.Controls.UserControl
{
    private const string IniRelativePath = @"Data\SKSE\Plugins\TDL_StreamPlugin.ini";
    private const string ToolsRelativePath = @"Data\TDL\Tools\tdl_send.exe";

    private const string SectionName = "Chaos";

    // TDL_AllRanges.txt → CHAOS
    private const int DefaultBackfireChance = 20;         // 0..100
    private const int DefaultBackfireDuration = 60;       // 1..600
    private const int DefaultShoutPushForce = 20;         // 0..200
    private const double DefaultShoutPushDelay = 0.1;     // 0.0..0.5

    private const int DefaultKnockbackForce = 25;         // 0..200
    private const double DefaultKnockbackCooldown = 0.35; // 0.0..2.0
    private const int DefaultKnockbackRadius = 900;       // 0..20000
    private const double DefaultKnockbackMeleeDelay = 0.12; // 0.0..0.5
    private const double DefaultKnockbackBowDelay = 0.14;   // 0.0..0.5

    private bool _subscribed;

    // Чтобы корректно обновлять StatusText при смене языка (без перезаписи данных UI),
    // запоминаем последний "шаблон" статуса и его аргументы.
    private string? _lastStatusKey;
    private string? _lastStatusFallbackRu;
    private string? _lastStatusFallbackEn;
    private object[]? _lastStatusArgs;

    public ChaosPage()
    {
        InitializeComponent();

        Loaded += (_, __) =>
        {
            if (_subscribed) return;
            LocalizationManager.LanguageChanged += OnLanguageChanged;
            _subscribed = true;
        };

        Unloaded += (_, __) =>
        {
            if (!_subscribed) return;
            LocalizationManager.LanguageChanged -= OnLanguageChanged;
            _subscribed = false;
        };

        ApplyDefaultsToUi();
        AutoLoadFromIniSilent();
    }

    private void OnLanguageChanged(AppLanguage _)
    {
        RefreshStatus();
    }

    private static bool IsRu => LocalizationManager.CurrentLanguage != AppLanguage.En;

    private static string L(string key, string fallbackRu, string fallbackEn)
    {
        object? v = System.Windows.Application.Current?.TryFindResource(key);
        if (v is string s && !string.IsNullOrWhiteSpace(s))
            return s;

        return IsRu ? fallbackRu : fallbackEn;
    }

    private static string LF(string key, string fallbackRu, string fallbackEn, params object[] args)
    {
        var fmt = L(key, fallbackRu, fallbackEn);
        try { return string.Format(fmt, args); }
        catch { return fmt; }
    }

    private static string UiTitle => L("STR_App_Title", "TDL Configurator", "TDL Configurator");

    private static string SafeNow() => DateTime.Now.ToString("HH:mm:ss");

    private void SetStatus(string key, string fallbackRu, string fallbackEn, params object[] args)
    {
        _lastStatusKey = key;
        _lastStatusFallbackRu = fallbackRu;
        _lastStatusFallbackEn = fallbackEn;
        _lastStatusArgs = args;

        StatusText.Text = (args != null && args.Length > 0)
            ? LF(key, fallbackRu, fallbackEn, args)
            : L(key, fallbackRu, fallbackEn);
    }

    private void RefreshStatus()
    {
        if (string.IsNullOrWhiteSpace(_lastStatusKey))
            return;

        var key = _lastStatusKey!;
        var fru = _lastStatusFallbackRu ?? string.Empty;
        var fen = _lastStatusFallbackEn ?? string.Empty;
        var args = _lastStatusArgs ?? Array.Empty<object>();

        StatusText.Text = (args.Length > 0)
            ? LF(key, fru, fen, args)
            : L(key, fru, fen);
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
            System.Windows.MessageBox.Show(
                LF("STR_Common_Msg_SettingsReadFailed",
                    "Не удалось прочитать settings.json.\n{0}",
                    "Failed to read settings.json.\n{0}",
                    ex.Message),
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
        {
            System.Windows.MessageBox.Show(
                L("STR_Common_Msg_GamePathInvalid",
                    "Путь к игре не задан или неверный.\nОткрой настройки и укажи папку Skyrim Special Edition.",
                    "Game path is not set or invalid.\nOpen Settings and select the Skyrim Special Edition folder."),
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
            SetStatus("STR_Chaos_Status_GamePathMissing_Default",
                "Путь к игре не задан (default).",
                "Game path is not set (defaults).");
            return;
        }

        if (!File.Exists(iniPath))
        {
            SetStatus("STR_Chaos_Status_IniMissing_Default",
                "INI не найден (default).",
                "INI not found (defaults).");
            return;
        }

        var map = ReadSection(iniPath, SectionName);
        BackfireChanceBox.Text = GetOr(map, "BackfireChance", BackfireChanceBox.Text);
        BackfireDurationBox.Text = GetOr(map, "BackfireDuration", BackfireDurationBox.Text);
        ShoutPushForceBox.Text = GetOr(map, "ShoutPushForce", ShoutPushForceBox.Text);
        ShoutPushDelayBox.Text = GetOr(map, "ShoutPushDelay", ShoutPushDelayBox.Text);

        KnockbackForceBox.Text = GetOr(map, "KnockbackForce", KnockbackForceBox.Text);
        KnockbackCooldownBox.Text = GetOr(map, "KnockbackCooldown", KnockbackCooldownBox.Text);
        KnockbackRadiusBox.Text = GetOr(map, "KnockbackRadius", KnockbackRadiusBox.Text);
        KnockbackMeleeDelayBox.Text = GetOr(map, "KnockbackMeleeDelay", KnockbackMeleeDelayBox.Text);
        KnockbackBowDelayBox.Text = GetOr(map, "KnockbackBowDelay", KnockbackBowDelayBox.Text);

        SetStatus("STR_Chaos_Status_LoadedFromIni",
            "Загружено из INI ({0}).",
            "Loaded from INI ({0}).",
            SafeNow());
    }

    private void SaveApply_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetIniPath(out var iniPath))
            return;

        // Validate (TDL_AllRanges.txt → CHAOS)
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

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(iniPath)!);
            UpsertSection(iniPath, SectionName, kv);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                LF("STR_Common_Msg_IniSaveFailed",
                    "Не удалось сохранить INI.\n{0}",
                    "Failed to save INI.\n{0}",
                    ex.Message),
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        // Apply in-game (SYSTEM_RELOAD_CONFIG)
        if (TryApplyInGame(out var reason))
        {
            SetStatus("STR_Chaos_Status_SavedApplied",
                "Сохранено и применено ({0}).",
                "Saved and applied ({0}).",
                SafeNow());
        }
        else
        {
            SetStatus("STR_Chaos_Status_SavedNotApplied",
                "Сохранено, но не применено ({0}).",
                "Saved, but not applied ({0}).",
                SafeNow());

            System.Windows.MessageBox.Show(
                LF("STR_Chaos_Msg_AppliedFailed",
                    "INI сохранён, но применить в игре не удалось.\n{0}",
                    "INI saved, but could not apply in-game.\n{0}",
                    reason),
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
            reason = L("STR_Common_Reason_GamePathMissing",
                "Путь к игре не задан.",
                "Game path is not set.");
            return false;
        }

        var tdlSend = Path.Combine(gamePath, ToolsRelativePath);
        if (!File.Exists(tdlSend))
        {
            reason = LF("STR_Common_Reason_TdlSendMissing",
                "tdl_send.exe не найден: {0}",
                "tdl_send.exe not found: {0}",
                tdlSend);
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
                reason = L("STR_Common_Reason_ProcessStartFailed",
                    "Не удалось запустить tdl_send.exe.",
                    "Failed to start tdl_send.exe.");
                return false;
            }

            if (!p.WaitForExit(3500))
            {
                try { p.Kill(entireProcessTree: true); } catch { }
                reason = L("STR_Common_Reason_ProcessTimeout",
                    "tdl_send.exe не завершился по таймауту.",
                    "tdl_send.exe timed out.");
                return false;
            }

            var stdout = p.StandardOutput.ReadToEnd().Trim();
            var stderr = p.StandardError.ReadToEnd().Trim();

            if (p.ExitCode != 0)
            {
                var details = (string.IsNullOrWhiteSpace(stderr) ? stdout : stderr).Trim();
                reason = LF("STR_Common_Reason_ExitCode",
                    "Код выхода: {0}\n{1}",
                    "Exit code: {0}\n{1}",
                    p.ExitCode,
                    details);
                return false;
            }

            // Даже при ExitCode=0 сервер может быть недоступен — но это лучше, чем ничего.
            // stdout/stderr оставляем только на случай будущей диагностики.
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
        SetStatus("STR_Chaos_Status_ResetDefaults",
            "Сброшено на default ({0}).",
            "Reset to defaults ({0}).",
            SafeNow());
    }

    private void DefaultRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not WpfButton btn)
            return;

        var key = btn.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(key))
            return;

        SetDefaultForKey(key);
        SetStatus("STR_Chaos_Status_DefaultForKey",
            "Default: {0} ({1}).",
            "Default: {0} ({1}).",
            key!,
            SafeNow());
    }

    private void ApplyDefaultsToUi()
    {
        BackfireChanceBox.Text = DefaultBackfireChance.ToString(CultureInfo.InvariantCulture);
        BackfireDurationBox.Text = DefaultBackfireDuration.ToString(CultureInfo.InvariantCulture);
        ShoutPushForceBox.Text = DefaultShoutPushForce.ToString(CultureInfo.InvariantCulture);
        ShoutPushDelayBox.Text = DefaultShoutPushDelay.ToString("0.##", CultureInfo.InvariantCulture);

        KnockbackForceBox.Text = DefaultKnockbackForce.ToString(CultureInfo.InvariantCulture);
        KnockbackCooldownBox.Text = DefaultKnockbackCooldown.ToString("0.##", CultureInfo.InvariantCulture);
        KnockbackRadiusBox.Text = DefaultKnockbackRadius.ToString(CultureInfo.InvariantCulture);
        KnockbackMeleeDelayBox.Text = DefaultKnockbackMeleeDelay.ToString("0.##", CultureInfo.InvariantCulture);
        KnockbackBowDelayBox.Text = DefaultKnockbackBowDelay.ToString("0.##", CultureInfo.InvariantCulture);

        SetStatus("STR_Chaos_Status_ReadyDefault",
            "Готово (default).",
            "Ready (defaults).");
    }

    private void SetDefaultForKey(string key)
    {
        switch (key)
        {
            case "BackfireChance": BackfireChanceBox.Text = DefaultBackfireChance.ToString(CultureInfo.InvariantCulture); break;
            case "BackfireDuration": BackfireDurationBox.Text = DefaultBackfireDuration.ToString(CultureInfo.InvariantCulture); break;
            case "ShoutPushForce": ShoutPushForceBox.Text = DefaultShoutPushForce.ToString(CultureInfo.InvariantCulture); break;
            case "ShoutPushDelay": ShoutPushDelayBox.Text = DefaultShoutPushDelay.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "KnockbackForce": KnockbackForceBox.Text = DefaultKnockbackForce.ToString(CultureInfo.InvariantCulture); break;
            case "KnockbackCooldown": KnockbackCooldownBox.Text = DefaultKnockbackCooldown.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "KnockbackRadius": KnockbackRadiusBox.Text = DefaultKnockbackRadius.ToString(CultureInfo.InvariantCulture); break;
            case "KnockbackMeleeDelay": KnockbackMeleeDelayBox.Text = DefaultKnockbackMeleeDelay.ToString("0.##", CultureInfo.InvariantCulture); break;
            case "KnockbackBowDelay": KnockbackBowDelayBox.Text = DefaultKnockbackBowDelay.ToString("0.##", CultureInfo.InvariantCulture); break;
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

            // Remove old section content
            var removeCount = end - (start + 1);
            if (removeCount > 0)
                lines.RemoveRange(start + 1, removeCount);

            // Insert new content
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
                LF("STR_Common_Validation_IntRequired",
                    "{0}: введи целое число.",
                    "{0}: enter an integer.",
                    name),
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show(
                LF("STR_Common_Validation_RangeInt",
                    "{0}: допустимый диапазон {1}..{2}.",
                    "{0}: allowed range {1}..{2}.",
                    name, min, max),
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
                LF("STR_Common_Validation_NumberRequired",
                    "{0}: введи число.",
                    "{0}: enter a number.",
                    name),
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show(
                LF("STR_Common_Validation_RangeDouble",
                    "{0}: допустимый диапазон {1}..{2}.",
                    "{0}: allowed range {1}..{2}.",
                    name,
                    min.ToString("0.##", CultureInfo.InvariantCulture),
                    max.ToString("0.##", CultureInfo.InvariantCulture)),
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }
}
