using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TDL.Configurator.App.Services;
using TDL.Configurator.Core;
using Forms = System.Windows.Forms;

namespace TDL.Configurator.App.Windows;

public partial class SettingsWindow : Window
{
    // Links are not provided yet (placeholders).
    private const string UrlNexus = "";
    private const string UrlGitHub = "";
    private const string UrlDonation = "";
    private const string UrlDiscord = "";
    private const string UrlUpdateCheck = "";

    private readonly AppSettings _settings;
    private readonly AppTheme _originalTheme;
    private readonly AppLanguage _originalLanguage;

    private bool _saved;

    private bool _isInitializing;

    public SettingsWindow()
    {
        InitializeComponent();

        Closing += (_, __) =>
        {
            // If the user closes the window via X/Alt+F4 without saving,
            // revert preview changes.
            if (!_saved)
            {
                ThemeManager.ApplyTheme(_originalTheme);
                LocalizationManager.ApplyLanguage(_originalLanguage);
            }
        };

        _settings = AppSettings.Load();
        _originalTheme = _settings.Theme;
        _originalLanguage = _settings.Language;

        _isInitializing = true;
        GamePathBox.Text = _settings.GamePath;
        SelectComboItemByTag(ThemeBox, _settings.Theme.ToString());
        SelectComboItemByTag(LanguageBox, _settings.Language.ToString());
        _isInitializing = false;
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

    private void ThemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing)
            return;

        ThemeManager.ApplyTheme(GetSelectedTheme());
    }

    private void LanguageBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing)
            return;

        LocalizationManager.ApplyLanguage(GetSelectedLanguage());
    }

    private void CheckUpdates_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UrlUpdateCheck))
        {
            System.Windows.MessageBox.Show(
                GetString("STR_Msg_UpdateUrlNotSet"),
                GetString("STR_Msg_Settings"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        OpenUrl(UrlUpdateCheck);
    }

    private void Nexus_Click(object sender, RoutedEventArgs e) => OpenLinkOrWarn(UrlNexus);
    private void GitHub_Click(object sender, RoutedEventArgs e) => OpenLinkOrWarn(UrlGitHub);
    private void Donation_Click(object sender, RoutedEventArgs e) => OpenLinkOrWarn(UrlDonation);
    private void Discord_Click(object sender, RoutedEventArgs e) => OpenLinkOrWarn(UrlDiscord);

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var path = (GamePathBox.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            System.Windows.MessageBox.Show(
                GetString("STR_Msg_SelectExistingFolder"),
                GetString("STR_Msg_Settings"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        _settings.GamePath = path;
        _settings.Theme = GetSelectedTheme();
        _settings.Language = GetSelectedLanguage();
        _settings.Save();

        _saved = true;

        _saved = true;

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        // Roll back preview changes
        ThemeManager.ApplyTheme(_originalTheme);
        LocalizationManager.ApplyLanguage(_originalLanguage);

        _saved = false;

        DialogResult = false;
        Close();
    }

    private AppTheme GetSelectedTheme() => EnumTryParseFromTag<AppTheme>(ThemeBox);
    private AppLanguage GetSelectedLanguage() => EnumTryParseFromTag<AppLanguage>(LanguageBox);

    private static TEnum EnumTryParseFromTag<TEnum>(Selector selector) where TEnum : struct
    {
        var item = selector.SelectedItem as ComboBoxItem;
        var tag = item?.Tag?.ToString();
        return Enum.TryParse(tag, out TEnum value) ? value : default;
    }

    private static void SelectComboItemByTag(Selector selector, string tag)
    {
        if (selector is not System.Windows.Controls.ComboBox cb)
            return;

        var match = cb.Items
            .OfType<ComboBoxItem>()
            .FirstOrDefault(i => string.Equals(i.Tag?.ToString(), tag, StringComparison.OrdinalIgnoreCase));

        if (match != null)
            cb.SelectedItem = match;
    }

    private static string GetString(string key)
    {
        var v = System.Windows.Application.Current.TryFindResource(key);
        return v?.ToString() ?? key;
    }

    private static void OpenLinkOrWarn(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            System.Windows.MessageBox.Show(
                GetString("STR_Msg_LinkNotSet"),
                GetString("STR_Msg_Settings"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        OpenUrl(url);
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // Silent fail: nothing critical
        }
    }
}
