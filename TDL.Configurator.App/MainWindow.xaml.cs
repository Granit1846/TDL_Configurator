using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TDL.Configurator.App.Services;
using TDL.Configurator.App.Pages;
using TDL.Configurator.Core;


namespace TDL.Configurator.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ApplyAdvancedMode();

        // Выберем первую вкладку при старте
        NavList.SelectedIndex = 0;
    }

    private static string L(string key, string fallback)
        => System.Windows.Application.Current?.TryFindResource(key) as string ?? fallback;

    private static string LF(string key, string fallbackFormat, params object[] args)
    {
        var fmt = L(key, fallbackFormat);
        try
        {
            return string.Format(fmt, args);
        }
        catch
        {
            return fmt;
        }
    }

    private void ApplyAdvancedMode()
    {
        try
        {
            var s = AppSettings.Load();
            var enabled = s?.AdvancedMode ?? false;

            if (NavTestItem != null)
                NavTestItem.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;

            if (!enabled && NavList.SelectedItem is ListBoxItem li && string.Equals(li.Tag?.ToString(), "Test", StringComparison.OrdinalIgnoreCase))
                NavList.SelectedIndex = 0;
        }
        catch
        {
            // ignore
        }
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        var w = new TDL.Configurator.App.Windows.SettingsWindow
        {
            Owner = this
        };

        var ok = w.ShowDialog() == true;
        if (ok)
        {
            ApplyAdvancedMode();

            if (MainContent.Content is DocumentationPage dp)
                dp.ReloadFromSettings();
        }

        // Пока ничего не обновляем принудительно — QuickAccess будем читать настройки при открытии.
        // Если нужно — позже добавим "Refresh current page".
    }

    private void OnLaunchGameClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var s = AppSettings.Load();
            var gamePath = (s?.GamePath ?? string.Empty).Trim();

            var caption = L("STR_App_Title", "TDL Configurator");

            if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
            {
                System.Windows.MessageBox.Show(
                    L(
                        "STR_LaunchGame_Msg_GamePathMissing",
                        LocalizationManager.CurrentLanguage == AppLanguage.En
                            ? "Game path is not set or the folder does not exist.\n\nOpen Settings and select the Skyrim Special Edition folder."
                            : "Путь к игре не задан или папка не существует.\n\nОткрой Настройки и укажи папку: Skyrim Special Edition."),
                    caption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // Удобно сразу открыть настройки.
                OnSettingsClick(this, new RoutedEventArgs());
                return;
            }

            var loaderPath = Path.Combine(gamePath, "skse64_loader.exe");
            if (!File.Exists(loaderPath))
            {
                System.Windows.MessageBox.Show(
                    LF(
                        "STR_LaunchGame_Msg_LoaderMissing_Fmt",
                        LocalizationManager.CurrentLanguage == AppLanguage.En
                            ? "skse64_loader.exe was not found:\nCheck your SKSE installation and the game path in Settings."
                            : "Не найден skse64_loader.exe:\nПроверь версию SKSE и путь к игре в Настройках.",
                        loaderPath),
                    caption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = loaderPath,
                WorkingDirectory = gamePath,
                UseShellExecute = true
            };

            Process.Start(psi);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                LF(
                    "STR_LaunchGame_Msg_LaunchFailed_Fmt",
                    LocalizationManager.CurrentLanguage == AppLanguage.En
                        ? "Failed to launch the game via SKSE.\n\n{0}"
                        : "Не удалось запустить игру через SKSE.\n\n{0}",
                    ex.Message),
                L("STR_App_Title", "TDL Configurator"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavList.SelectedItem is not ListBoxItem item)
            return;

        var key = item.Tag?.ToString() ?? "";

        string GetPlaceholderText(string pageKey)
        {
            var fmt = System.Windows.Application.Current?.TryFindResource("STR_Page_Placeholder") as string;
            if (string.IsNullOrWhiteSpace(fmt))
                fmt = "Page: {0}";

            try
            {
                return string.Format(fmt, pageKey);
            }
            catch
            {
                // Если строка ресурса случайно не форматная.
                return fmt + " " + pageKey;
            }
        }

        MainContent.Content = key switch
        {
            "Chaos" => new ChaosPage(),
            "QuickAccess" => new QuickAccessPage(),
            "Test" => new TestPage(),

            "Inventory" => new InventoryPage(),
            "Wrath" => new WrathPage(),
            "Hunter" => new HunterPage(),
            "Characteristics" => new GigantPage(),
            "Show" => new ComedyPage(),
            "Information" => new DocumentationPage(),
            _ => new TextBlock
            {
                Text = GetPlaceholderText(key),
                FontSize = 20,
                FontWeight = FontWeights.SemiBold
            }
        };
    }

    private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
    {

    }
}
