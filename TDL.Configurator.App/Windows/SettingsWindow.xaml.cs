using System.IO;
using System.Windows;
using TDL.Configurator.Core;
using Forms = System.Windows.Forms;

namespace TDL.Configurator.App.Windows;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;

    public SettingsWindow()
    {
        InitializeComponent();
        _settings = AppSettings.Load();
        GamePathBox.Text = _settings.GamePath;
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        using var dlg = new Forms.FolderBrowserDialog
        {
            Description = "Выберите папку игры Skyrim Special Edition",
            UseDescriptionForTitle = true
        };

        if (dlg.ShowDialog() == Forms.DialogResult.OK)
            GamePathBox.Text = dlg.SelectedPath;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var path = GamePathBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            System.Windows.MessageBox.Show("Укажите существующую папку игры.", "Настройки",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _settings.GamePath = path;
        _settings.Save();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
