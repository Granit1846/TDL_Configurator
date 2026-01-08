using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TDL.Configurator.Core;

namespace TDL.Configurator.App.Pages;

public class IniPageBase : System.Windows.Controls.UserControl
{
    protected string GamePath => (AppSettings.Load().GamePath ?? "").Trim();

    protected string IniPath =>
        Path.Combine(GamePath, "Data", "SKSE", "Plugins", "TDL_StreamPlugin.ini");

    protected bool EnsureGamePath()
    {
        if (string.IsNullOrWhiteSpace(GamePath) || !Directory.Exists(GamePath))
        {
            System.Windows.MessageBox.Show(
                "Сначала укажи путь к игре в Настройках (корень Skyrim Special Edition).",
                "TDL Configurator",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            SetStatus("Ошибка: путь к игре не задан.");
            return false;
        }

        return true;
    }

    protected void SetStatus(string text)
    {
        if (this.FindName("StatusText") is System.Windows.Controls.TextBlock tb)
            tb.Text = text;
    }

    protected void ShowSaved(string title)
    {
        System.Windows.MessageBox.Show(
            "Сохранено.",
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        SetStatus($"Сохранено: {DateTime.Now:HH:mm:ss}");
    }

    protected void ShowLoaded()
        => SetStatus($"Загружено: {DateTime.Now:HH:mm:ss}");
}
