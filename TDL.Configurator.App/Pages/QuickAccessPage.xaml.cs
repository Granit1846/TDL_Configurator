using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using TDL.Configurator.Core;
using System.Diagnostics;

namespace TDL.Configurator.App.Pages;

public partial class QuickAccessPage : System.Windows.Controls.UserControl
{
    private const string UiTitle = "TDL Configurator";

    public QuickAccessPage()
    {
        InitializeComponent();
        UpdateStatus();
    }

    private string GamePath => AppSettings.Load().GamePath.Trim();

    private string PluginsFolder => Path.Combine(GamePath, "Data", "SKSE", "Plugins");
    private string IniPath => Path.Combine(PluginsFolder, "TDL_StreamPlugin.ini");

    private string ToolsFolder => Path.Combine(GamePath, "Data", "TDL", "Tools");
    private string TdlDataFolder => Path.Combine(GamePath, "Data", "TDL");

    private string DocsRoot => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private string TdlLogFolder => Path.Combine(DocsRoot, "My Games", "Skyrim Special Edition", "Logs", "Script", "User");
    private string TdlLogPath => Path.Combine(TdlLogFolder, "TDL.0.log");

    private string SkseDocsFolder => Path.Combine(DocsRoot, "My Games", "Skyrim Special Edition", "SKSE");
    private string PluginLogPath => Path.Combine(SkseDocsFolder, "TDL_StreamPlugin.log");

    private bool HasGamePath()
        => !string.IsNullOrWhiteSpace(GamePath) && Directory.Exists(GamePath);

    private bool EnsureGamePath()
    {
        if (!HasGamePath())
        {
            System.Windows.MessageBox.Show(
                "Сначала укажи путь к игре в Настройках (корень Skyrim Special Edition).",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return false;
        }

        return true;
    }

    private void OpenFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            System.Windows.MessageBox.Show(
                $"Папка не найдена:\n{folder}",
                UiTitle,
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
                $"Файл не найден:\n{file}",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
    }

    private void UpdateStatus()
    {
        // ВАЖНО: без MessageBox при старте страницы (как у остальных вкладок)
        if (!HasGamePath())
        {
            StatusText.Text = "Статус: путь к игре не задан (Настройки).";
            return;
        }

        var okIni = File.Exists(IniPath);
        var okLog = File.Exists(TdlLogPath);
        var okTools = Directory.Exists(ToolsFolder);

        StatusText.Text =
            $"Статус: INI={(okIni ? "OK" : "нет")} | TDL.0.log={(okLog ? "OK" : "нет")} | Tools={(okTools ? "OK" : "нет")}";
    }

    private void ApplyConfig_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath()) return;

        var tdlSend = Path.Combine(GamePath, "Data", "TDL", "Tools", "tdl_send.exe");
        if (!File.Exists(tdlSend))
        {
            System.Windows.MessageBox.Show(
                $"Не найден tdl_send.exe:\n{tdlSend}\n\nПроверь, что Tools установлен в папку Data\\TDL\\Tools.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = tdlSend,
                Arguments = "NORMAL SYSTEM_RELOAD_CONFIG 2",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi);
            var stdout = p!.StandardOutput.ReadToEnd();
            var stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode == 0)
            {
                System.Windows.MessageBox.Show(
                    string.IsNullOrWhiteSpace(stdout) ? "Настройки применены (OK)." : stdout,
                    UiTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show(
                    $"Не удалось применить настройки.\nExitCode: {p.ExitCode}\n\n{(string.IsNullOrWhiteSpace(stderr) ? stdout : stderr)}",
                    UiTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            UpdateStatus();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Ошибка запуска tdl_send.exe:\n{ex.Message}",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OpenPluginsFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath()) return;
        OpenFolder(PluginsFolder);
    }

    private void OpenIni_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath()) return;
        OpenFile(IniPath);
    }

    private void OpenToolsFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath()) return;
        OpenFolder(ToolsFolder);
    }

    private void OpenTdlDataFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath()) return;
        OpenFolder(TdlDataFolder);
    }

    private void OpenTdlLog_Click(object sender, RoutedEventArgs e) => OpenFile(TdlLogPath);
    private void OpenTdlLogFolder_Click(object sender, RoutedEventArgs e) => OpenFolder(TdlLogFolder);

    private void OpenSkseDocsFolder_Click(object sender, RoutedEventArgs e) => OpenFolder(SkseDocsFolder);
    private void OpenPluginLog_Click(object sender, RoutedEventArgs e) => OpenFile(PluginLogPath);

    private void CreateIni_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureGamePath()) return;

        try
        {
            Directory.CreateDirectory(PluginsFolder);

            if (File.Exists(IniPath))
            {
                System.Windows.MessageBox.Show(
                    "INI уже существует.",
                    UiTitle,
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
            sb.AppendLine("ShoutPushDelay=0.05");
            sb.AppendLine("KnockbackForce=25");
            sb.AppendLine("KnockbackCooldown=0.35");
            sb.AppendLine("KnockbackRadius=900");
            sb.AppendLine("KnockbackMeleeDelay=0.10");
            sb.AppendLine("KnockbackBowDelay=0.10");
            sb.AppendLine();

            sb.AppendLine("[Inventory]");
            sb.AppendLine("ScatterExactCount=0");
            sb.AppendLine("ScatterMinCount=150");
            sb.AppendLine("ScatterMaxCount=200");
            sb.AppendLine("ScatterRadius=800");
            sb.AppendLine("DropBatchSize=10");
            sb.AppendLine("DropInterval=0.20");
            sb.AppendLine("DropTimeout=30");
            sb.AppendLine("ProtectTokensByName=1");
            sb.AppendLine("DropShowProgress=0");
            sb.AppendLine();

            sb.AppendLine("[Wrath]");
            sb.AppendLine("TotalBursts=6");
            sb.AppendLine("Interval=0.4");
            sb.AppendLine("Radius=300");
            sb.AppendLine("ZOffset=50");
            sb.AppendLine("DamageMin=5");
            sb.AppendLine("DamageMax=15");
            sb.AppendLine("FireDamageMult=1.0");
            sb.AppendLine("StormMagickaMult=1.0");
            sb.AppendLine("FrostStaminaMult=1.0");
            sb.AppendLine("LevelScale=0.0");
            sb.AppendLine("LevelCap=3.0");
            sb.AppendLine("ShakeChance=0");
            sb.AppendLine("ShakeStrength=0.0");
            sb.AppendLine("ShakeDuration=0.0");
            sb.AppendLine();

            sb.AppendLine("[Hunter]");
            sb.AppendLine("Duration=90");
            sb.AppendLine("ReAggroInterval=4.0");
            sb.AppendLine("MaxDistance=5500");
            sb.AppendLine("SpawnOffset=1200");
            sb.AppendLine("CorpseLifetime=20");
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
                "INI создан.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            UpdateStatus();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Не удалось создать INI:\n{ex.Message}",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
