using System;
using System.Linq;
using System.Windows;
using TDL.Configurator.Core;

namespace TDL.Configurator.App.Services;

public static class LocalizationManager
{
    private const string StringsPrefix = "/Resources/Strings/Strings.";

    public static void ApplyLanguage(AppLanguage language)
    {
        var app = System.Windows.Application.Current;
        if (app == null)
            return;

        var targetSource = language switch
        {
            AppLanguage.En => new Uri($"{StringsPrefix}en.xaml", UriKind.Relative),
            _ => new Uri($"{StringsPrefix}ru.xaml", UriKind.Relative)
        };

        var merged = app.Resources.MergedDictionaries;

        var existing = merged.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("/Resources/Strings/Strings."));
        if (existing != null)
        {
            existing.Source = targetSource;
            return;
        }

        merged.Add(new ResourceDictionary { Source = targetSource });
    }
}
