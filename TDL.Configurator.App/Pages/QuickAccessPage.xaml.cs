// QuickAccessPage: dynamic status text updates on language change (LocalizationManager.LanguageChanged)

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using TDL.Configurator.App.Services;
using TDL.Configurator.Core;

namespace TDL.Configurator.App.Pages;

public partial class QuickAccessPage : System.Windows.Controls.UserControl
{
    private bool _subscribed;

    public QuickAccessPage()
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

        UpdateStatus();
    }

    private void OnLanguageChanged(AppLanguage _)
    {
        // При смене языка нужно пересобрать строку статуса (она генерируется кодом)
        UpdateStatus();
    }

    private static bool IsRu => LocalizationManager.CurrentLanguage != AppLanguage.En;

    private static string L(string key, string fallbackRu, string fallbackEn)
    {
        object v = System.Windows.Application.Current?.TryFindResource(key);
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

    private string GamePath
    {
        get
        {
            var p = AppSettings.Load().GamePath ?? string.Empty;
            return p.Trim();
        }
    }

    private string PluginsFolder => Path.Combine(GamePath, "Data", "SKSE", "Plugins");
    private string IniPath => Path.Combine(PluginsFolder, "TDL_StreamPlugin.ini");

    private string ToolsFolder => Path.Combine(GamePath, "Data", "TDL", "Tools");
    private string TdlDataFolder => Path.Combine(GamePath, "Data", "TDL");

    private string DocsRoot => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private string TdlLogFolder => Path.Combine(DocsRoot, "My Games", "Skyrim Special Edition", "Logs", "Script", "User");
    private string TdlLogPath => Path.Combine(TdlLogFolder, "TDL.0.log");

    private string SkseDocsFolder => Path.Combine(DocsRoot, "My Games", "Skyrim Special Edition", "SKSE");
    private string PluginLogPath => Path.Combine(SkseDocsFolder, "TDL_StreamPlugin.log");

    private bool EnsureGamePath(bool showMessage)
    {
        if (string.IsNullOrWhiteSpace(GamePath) || !Directory.Exists(GamePath))
        {
            if (showMessage)
            {
                System.Windows.MessageBox.Show(
                    L("STR_QuickAccess_Msg_GamePathRequired",
                        "Сначала укажи путь к игре в Настройках (корень Skyrim Special Edition).",
                        "Please set the game folder in Settings (Skyrim Special Edition root)."),
                    L("STR_App_Title", "TDL Configurator", "TDL Configurator"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            return false;
        }

        return true;
    }

    private void OpenFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            System.Windows.MessageBox.Show(
                LF("STR_QuickAccess_Msg_FolderNotFound",
                    "Папка не найдена:\n{0}",
                    "Folder not found:\n{0}",
                    folder),
                L("STR_App_Title", "TDL Configurator", "TDL Configurator"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{folder}\"") { UseShellExecute = true });
    }

    private void OpenFile(string file)
    {
        if (!File.Exists(file))
        {
            System.Windows.MessageBox.Show(
                LF("STR_QuickAccess_Msg_FileNotFound",
                    "Файл не найден:\n{0}",
                    "File not found:\n{0}",
                    file),
                L("STR_App_Title", "TDL Configurator", "TDL Configurator"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
    }

    private void UpdateStatus()
    {
        // Статус должен быть БЕЗ всплывающих окон, потому что он будет вызываться при смене языка.
        if (!EnsureGamePath(showMessage: false))
        {
            StatusText.Text = L("STR_QuickAccess_Status_GamePathMissing",
                "Статус: путь к игре не задан (Настройки).",
                "Status: game path is not set (Settings).");
            return;
        }

        var okIni = File.Exists(IniPath);
        var okLog = File.Exists(TdlLogPath);
        var okTools = Directory.Exists(ToolsFolder);

        var okText = L("STR_Common_Ok", "OK", "OK");
        var noText = L("STR_Common_No", "нет", "No");

        var fmt = L("STR_QuickAccess_StatusLine_Format",
            "Статус: INI={0} | TDL.0.log={1} | Инструменты={2}",
            "Status: INI={0} | TDL.0.log={1} | Tools={2}");

        StatusText.Text = string.Format(fmt,
            okIni ? okText : noText,
            okLog ? okText : noText,
            okTools ? okText : noText);
    }

    private void OpenPluginsFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath(showMessage: true)) return;
        OpenFolder(PluginsFolder);
        UpdateStatus();
    }

    private void OpenIni_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath(showMessage: true)) return;
        OpenFile(IniPath);
        UpdateStatus();
    }

    private void OpenToolsFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath(showMessage: true)) return;
        OpenFolder(ToolsFolder);
        UpdateStatus();
    }

    private void OpenTdlDataFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath(showMessage: true)) return;
        OpenFolder(TdlDataFolder);
        UpdateStatus();
    }

    private void OpenTdlLog_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath(showMessage: true)) return;
        OpenFile(TdlLogPath);
        UpdateStatus();
    }

    private void OpenTdlLogFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath(showMessage: true)) return;
        OpenFolder(TdlLogFolder);
        UpdateStatus();
    }

    private void OpenSkseDocsFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath(showMessage: true)) return;
        OpenFolder(SkseDocsFolder);
        UpdateStatus();
    }

    private void OpenPluginLog_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath(showMessage: true)) return;
        OpenFile(PluginLogPath);
        UpdateStatus();
    }

    private void CreateIni_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath(showMessage: true)) return;

        try
        {
            Directory.CreateDirectory(PluginsFolder);

            if (File.Exists(IniPath))
            {
                System.Windows.MessageBox.Show(
                    L("STR_QuickAccess_Msg_IniAlreadyExists", "INI уже существует.", "INI already exists."),
                    L("STR_App_Title", "TDL Configurator", "TDL Configurator"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("; TDL_StreamPlugin.ini (template)");
            sb.AppendLine("; Generated by TDL Configurator");
            sb.AppendLine();

            sb.AppendLine("[Chaos]");
            sb.AppendLine("BackfireChance=20");
            sb.AppendLine("BackfireDuration=60");
            sb.AppendLine("ShoutPushForce=20");
            sb.AppendLine("ShoutPushDelay=0.1");
            sb.AppendLine("KnockbackForce=25");
            sb.AppendLine("KnockbackCooldown=0.35");
            sb.AppendLine("KnockbackRadius=900");
            sb.AppendLine("KnockbackMeleeDelay=0.12");
            sb.AppendLine("KnockbackBowDelay=0.14");
            sb.AppendLine();

            sb.AppendLine("[Inventory]");
            sb.AppendLine("ScatterExactCount=0");
            sb.AppendLine("ScatterMinCount=150");
            sb.AppendLine("ScatterMaxCount=200");
            sb.AppendLine("ScatterRadius=1200");
            sb.AppendLine("DropBatchSize=10");
            sb.AppendLine("DropInterval=0.2");
            sb.AppendLine("DropTimeout=30");
            sb.AppendLine("ProtectTokensByName=1");
            sb.AppendLine("DropShowProgress=0");
            sb.AppendLine();

            sb.AppendLine("[Comedy]");
            sb.AppendLine("FakeHeroDuration=120");
            sb.AppendLine("FakeHeroActionInterval=3");
            sb.AppendLine("FakeHeroDamageMult=0.8");
            sb.AppendLine("FakeHeroPushForce=15");
            sb.AppendLine("FakeHeroShoutChance=30");
            sb.AppendLine("FakeHeroSpellChance=30");
            sb.AppendLine("HorrorDuration=120");
            sb.AppendLine("HorrorSpawn=1200");
            sb.AppendLine("HorrorTeleport=1000");
            sb.AppendLine("HorrorMaxDist=3000");
            sb.AppendLine("HorrorHealth=1000");
            sb.AppendLine("ArenaWaves=3");
            sb.AppendLine("ArenaPerWave=3");
            sb.AppendLine("ArenaInterval=5");
            sb.AppendLine("ArenaRadius=1200");
            sb.AppendLine("EscortDuration=120");
            sb.AppendLine();

            sb.AppendLine("[Hunter]");
            sb.AppendLine("CorpseTime=30");
            sb.AppendLine("Duration=90");
            sb.AppendLine("MaxDistance=5500");
            sb.AppendLine("ReAggro=4");
            sb.AppendLine("SpawnOffset=1200");
            sb.AppendLine();

            sb.AppendLine("[Wrath]");
            sb.AppendLine("WrathTotalBursts=10");
            sb.AppendLine("WrathInterval=0.4");
            sb.AppendLine("WrathRadius=800");
            sb.AppendLine("WrathZOffset=50");
            sb.AppendLine("WrathDamageMin=5");
            sb.AppendLine("WrathDamageMax=15");
            sb.AppendLine("WrathFireMult=1");
            sb.AppendLine("WrathStormMag=1");
            sb.AppendLine("WrathFrostSta=1");
            sb.AppendLine("WrathLevelScale=0");
            sb.AppendLine("WrathLevelCap=3");
            sb.AppendLine("WrathShakeChance=20");
            sb.AppendLine("WrathShakeStrength=0.1");
            sb.AppendLine("WrathShakeDuration=0.2");
            sb.AppendLine();

            sb.AppendLine("[Gigant]");
            sb.AppendLine("SizeDuration=60");
            sb.AppendLine("SpeedDuration=60");
            sb.AppendLine("ScaleBig=2.0");
            sb.AppendLine("DamageBig=5.0");
            sb.AppendLine("ScaleSmall=0.33");
            sb.AppendLine("DamageSmall=0.5");
            sb.AppendLine("SpeedFast=3.0");
            sb.AppendLine("SpeedSlow=0.5");
            sb.AppendLine();

            File.WriteAllText(IniPath, sb.ToString(), Encoding.UTF8);

            System.Windows.MessageBox.Show(
                L("STR_QuickAccess_Msg_IniCreated", "INI создан.", "INI created."),
                L("STR_App_Title", "TDL Configurator", "TDL Configurator"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            UpdateStatus();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                LF("STR_QuickAccess_Msg_CreateIniFailed",
                    "Не удалось создать INI:\n{0}",
                    "Failed to create INI:\n{0}",
                    ex.Message),
                L("STR_App_Title", "TDL Configurator", "TDL Configurator"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
