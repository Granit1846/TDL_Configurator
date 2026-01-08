using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TDL.Configurator.Core;

namespace TDL.Configurator.App.Pages
{
    public partial class ChaosPage : System.Windows.Controls.UserControl
    {
        private const string IniRelativePath = @"Data\SKSE\Plugins\TDL_StreamPlugin.ini";
        private const string SectionName = "Chaos";

        public ObservableCollection<IniEntry> Entries { get; } = new ObservableCollection<IniEntry>();

        public ChaosPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetIniPath(out var iniPath))
                return;

            Entries.Clear();
            foreach (var item in IniFile.ReadSection(iniPath, SectionName))
                Entries.Add(item);

            System.Windows.MessageBox.Show($"Загружено: {Entries.Count} строк(и)\n\n{iniPath}",
                "Chaos", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetIniPath(out var iniPath))
                return;

            IniFile.WriteSection(iniPath, SectionName, Entries);

            System.Windows.MessageBox.Show($"Сохранено.\n\n{iniPath}",
                "Chaos", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            Entries.Add(new IniEntry { Key = "", Value = "" });
        }

        private void RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            if (ChaosGrid.SelectedItem is IniEntry selected)
                Entries.Remove(selected);
        }

        private bool TryGetIniPath(out string iniPath)
        {
            iniPath = "";

            // ВАЖНО: подставь здесь ровно тот способ, которым ты уже читаешь GamePath
            // (как в QuickAccessPage / TestPage / SettingsWindow).
            //
            // Ниже пример, если у тебя AppSettings.Load() возвращает объект с GamePath:
            //
            // var settings = AppSettings.Load();
            // var gamePath = settings.GamePath;

            string gamePath = GetGamePathFromYourSettings();

            if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
            {
                System.Windows.MessageBox.Show(
                    "Путь к игре не задан или неверный.\nОткрой настройки и укажи папку Skyrim Special Edition.",
                    "Chaos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            iniPath = Path.Combine(gamePath, IniRelativePath);
            return true;
        }

        private string GetGamePathFromYourSettings()
        {
            // TODO: замени на свой реальный код из проекта.
            // Я оставил так, чтобы проект не “угадывал” за тебя.
            return "";
        }
    }
}
