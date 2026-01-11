using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TDL.Configurator.App.Pages;
using TDL.Configurator.Core;


namespace TDL.Configurator.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Выберем первую вкладку при старте
        NavList.SelectedIndex = 0;
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        var w = new TDL.Configurator.App.Windows.SettingsWindow
        {
            Owner = this
        };
        w.ShowDialog();

        // Пока ничего не обновляем принудительно — QuickAccess будем читать настройки при открытии.
        // Если нужно — позже добавим "Refresh current page".
    }

    private void OnLaunchGameClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var s = AppSettings.Load();
            var gamePath = (s?.GamePath ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
            {
                System.Windows.MessageBox.Show(
                    "Путь к игре не задан или папка не существует.\n\nОткрой Настройки и укажи папку: Skyrim Special Edition.",
                    "TDL Configurator",
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
                    "Не найден skse64_loader.exe. Проверь версию SKSE и путь к игре в Настройках.",
                    "TDL Configurator",
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
                "Не удалось запустить игру через SKSE.\n\n" + ex.Message,
                "TDL Configurator",
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
