using System;
using System.Linq;
using System.Windows;
using TDL.Configurator.Core;

namespace TDL.Configurator.App.Services;

public static class LocalizationManager
{
    private const string StringsPrefix = "/Resources/Strings/Strings.";

    // ¬ажно: отражает “≈ ”ў»… применЄнный €зык (включа€ preview в Settings)
    public static AppLanguage CurrentLanguage { get; private set; } = AppLanguage.Ru;

    // —обытие дл€ страниц/окон, которым нужно обновить runtime-тексты (Status, MessageBox и т.п.)
    public static event Action<AppLanguage>? LanguageChanged;

    public static void ApplyLanguage(AppLanguage language)
    {
        CurrentLanguage = language;

        var app = System.Windows.Application.Current;
        if (app == null)
        {
            LanguageChanged?.Invoke(language);
            return;
        }

        var targetSource = language switch
        {
            AppLanguage.En => new Uri($"{StringsPrefix}en.xaml", UriKind.Relative),
            _ => new Uri($"{StringsPrefix}ru.xaml", UriKind.Relative)
        };

        var merged = app.Resources.MergedDictionaries;

        var existing = merged.FirstOrDefault(d =>
            d.Source != null && d.Source.OriginalString.Contains("/Resources/Strings/Strings."));
        if (existing != null)
        {
            existing.Source = targetSource;
            LanguageChanged?.Invoke(language);
            return;
        }

        merged.Add(new ResourceDictionary { Source = targetSource });
        LanguageChanged?.Invoke(language);
    }
}
