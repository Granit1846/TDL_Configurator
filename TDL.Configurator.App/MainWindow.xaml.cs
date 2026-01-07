using System.Windows;
using System.Windows.Controls;
using TDL.Configurator.App.Pages;


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

    private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NavList.SelectedItem is not ListBoxItem item)
            return;

        var key = item.Content?.ToString() ?? "";

        MainContent.Content = key switch
        {
            "Chaos" => new ChaosPage(),
            "Quick access" => new QuickAccessPage(),
            "Test" => new TestPage(),
            _ => new TextBlock
            {
                Text = $"Страница: {key}\n(контент добавим позже)",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold
            }
        };
    }

    private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
    {

    }
}