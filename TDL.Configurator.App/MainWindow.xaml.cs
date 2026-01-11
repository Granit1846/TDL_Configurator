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