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

public partial class HunterPage : System.Windows.Controls.UserControl
{
    private const string IniRelativePath = @"Data\SKSE\Plugins\TDL_StreamPlugin.ini";
    private const string ToolsRelativePath = @"Data\TDL\Tools\tdl_send.exe";

    private const string SectionName = "Hunter";
    private const string UiTitle = "TDL Configurator";

    // TDL_AllRanges.txt → HUNTER
    private const int DefaultCorpseTime = 30;     // 0..300
    private const int DefaultDuration = 90;       // 5..600
    private const int DefaultMaxDistance = 5500;  // 1500..10000
    private const double DefaultReAggro = 4.0;    // 1.0..10.0
    private const int DefaultSpawnOffset = 1200;  // 300..3000


    // ---- Localization helpers (RU/EN) ----
    private static string L(string key, string fallbackRu, string fallbackEn)
    {
        try
        {
            if (System.Windows.Application.Current?.TryFindResource(key) is string s && !string.IsNullOrWhiteSpace(s))
                return s;
        }
        catch
        {
            // ignore and use fallback
        }

        return LocalizationManager.CurrentLanguage == AppLanguage.En ? fallbackEn : fallbackRu;
    }

    private static string LF(string key, string fallbackRu, string fallbackEn, params object[] args)
        => string.Format(L(key, fallbackRu, fallbackEn), args);

    private static string UiTitleText => L("STR_App_Title", "TDL Configurator", "TDL Configurator");

    private (string key, string ru, string en, object[] args)? _lastStatus;

    private void SetStatus(string key, string fallbackRu, string fallbackEn, params object[] args)
    {
        _lastStatus = (key, fallbackRu, fallbackEn, args ?? Array.Empty<object>());
        var template = L(key, fallbackRu, fallbackEn);
        HunterStatusText.Text = (_lastStatus.Value.args.Length > 0) ? string.Format(template, _lastStatus.Value.args) : template;
    }

    private void RefreshStatus()
    {
        if (_lastStatus is null)
            return;

        var s = _lastStatus.Value;
        var template = L(s.key, s.ru, s.en);
        HunterStatusText.Text = (s.args.Length > 0) ? string.Format(template, s.args) : template;
    }

    private void OnLanguageChanged(AppLanguage _)
        => RefreshStatus();

    public HunterPage()
    {
        InitializeComponent();

        LocalizationManager.LanguageChanged += OnLanguageChanged;
        Unloaded += (_, __) => LocalizationManager.LanguageChanged -= OnLanguageChanged;

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
                LF("STR_Common_Msg_SettingsReadFailed", "Не удалось прочитать settings.json.\n{0}", "Failed to read settings.json.\n{0}", ex.Message),
                UiTitleText,
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
                UiTitleText,
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
            SetStatus("STR_Hunter_Status_GamePathMissing", "Путь к игре не задан (default).", "Game path is not set (default).");
            return;
        }

        if (!File.Exists(iniPath))
        {
            SetStatus("STR_Hunter_Status_IniMissing", "INI не найден (default).", "INI was not found (default).");
            return;
        }

        var map = ReadSection(iniPath, SectionName);
        CorpseTimeBox.Text = GetOr(map, "CorpseTime", CorpseTimeBox.Text);
        DurationBox.Text = GetOr(map, "Duration", DurationBox.Text);
        MaxDistanceBox.Text = GetOr(map, "MaxDistance", MaxDistanceBox.Text);
        ReAggroBox.Text = GetOr(map, "ReAggro", ReAggroBox.Text);
        SpawnOffsetBox.Text = GetOr(map, "SpawnOffset", SpawnOffsetBox.Text);

        SetStatus("STR_Hunter_Status_LoadedIni", "Загружено из INI ({0}).", "Loaded from INI ({0}).", SafeNow());
    }

    private void SaveApply_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetIniPath(out var iniPath))
            return;

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

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(iniPath)!);
            UpsertSection(iniPath, SectionName, kv);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                LF("STR_Common_Msg_IniSaveFailed", "Не удалось сохранить INI.\n{0}", "Failed to save INI.\n{0}", ex.Message),
                UiTitleText,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (TryApplyInGame(out var reason))
        {
            SetStatus("STR_Hunter_Status_SavedApplied", "Сохранено и применено ({0}).", "Saved and applied ({0}).", SafeNow());
        }
        else
        {
            SetStatus("STR_Hunter_Status_SavedNotApplied", "Сохранено, но не применено ({0}).", "Saved, but not applied ({0}).", SafeNow());
            System.Windows.MessageBox.Show(
                LF("STR_Common_Msg_IniSavedButNotApplied",
                    "INI сохранён, но применить в игре не удалось.\n{0}",
                    "The INI was saved, but applying it in-game failed.\n{0}",
                    reason),
                UiTitleText,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private bool TryApplyInGame(out string reason)
    {
        reason = "";
        if (!TryGetGamePath(out var gamePath))
        {
            reason = L("STR_Common_Reason_GamePathMissing", "Путь к игре не задан.", "Game path is not set.");
            return false;
        }

        var tdlSend = Path.Combine(gamePath, ToolsRelativePath);
        if (!File.Exists(tdlSend))
        {
            reason = LF("STR_Common_Reason_TdlSendMissing", "tdl_send.exe не найден: {0}", "tdl_send.exe was not found: {0}", tdlSend);
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
                reason = L("STR_Common_Reason_TdlSendStartFailed", "Не удалось запустить tdl_send.exe.", "Failed to start tdl_send.exe.");
                return false;
            }

            if (!p.WaitForExit(3500))
            {
                try { p.Kill(entireProcessTree: true); } catch { }
                reason = L("STR_Common_Reason_TdlSendTimeout", "tdl_send.exe не завершился по таймауту.", "tdl_send.exe timed out.");
                return false;
            }

            var stdout = p.StandardOutput.ReadToEnd().Trim();
            var stderr = p.StandardError.ReadToEnd().Trim();

            if (p.ExitCode != 0)
            {
                reason = LF("STR_Common_Reason_TdlSendExitCode", "Код выхода: {0}\n{1}", "Exit code: {0}\n{1}", p.ExitCode, (string.IsNullOrWhiteSpace(stderr) ? stdout : stderr)).Trim();
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
        SetStatus("STR_Hunter_Status_ResetDefault", "Сброшено на default ({0}).", "Reset to default ({0}).", SafeNow());
    }

    private void DefaultRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not WpfButton btn)
            return;

        var key = btn.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(key))
            return;

        SetDefaultForKey(key);
        SetStatus("STR_Hunter_Status_RowDefault", "Default: {0} ({1}).", "Default: {0} ({1}).", key, SafeNow());
    }

    private void ApplyDefaultsToUi()
    {
        CorpseTimeBox.Text = DefaultCorpseTime.ToString(CultureInfo.InvariantCulture);
        DurationBox.Text = DefaultDuration.ToString(CultureInfo.InvariantCulture);
        MaxDistanceBox.Text = DefaultMaxDistance.ToString(CultureInfo.InvariantCulture);
        ReAggroBox.Text = DefaultReAggro.ToString("0.##", CultureInfo.InvariantCulture);
        SpawnOffsetBox.Text = DefaultSpawnOffset.ToString(CultureInfo.InvariantCulture);

        SetStatus("STR_Hunter_Status_ReadyDefault", "Готово (default).", "Ready (default).");
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
                LF("STR_Common_Validation_Int", "{0}: введи целое число.", "{0}: enter an integer.", name),
                UiTitleText,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show(
                LF("STR_Common_Validation_Range", "{0}: допустимый диапазон {1}..{2}.", "{0}: allowed range is {1}..{2}.", name, min, max),
                UiTitleText,
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
                LF("STR_Common_Validation_Number", "{0}: введи число.", "{0}: enter a number.", name),
                UiTitleText,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        if (value < min || value > max)
        {
            System.Windows.MessageBox.Show(
                LF("STR_Common_Validation_Range", "{0}: допустимый диапазон {1}..{2}.", "{0}: allowed range is {1}..{2}.", name, min, max),
                UiTitleText,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        return true;
    }
}
