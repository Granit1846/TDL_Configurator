using System;
using System.Linq;
using System.Windows;
using TDL.Configurator.Core;

namespace TDL.Configurator.App.Services;

public static class ThemeManager
{
    // Color dictionaries only (shared styles are in Theme.Shared.xaml)
    private const string ColorsPrefix = "/Resources/Themes/Colors.";

    public static void ApplyTheme(AppTheme theme)
    {
        var app = System.Windows.Application.Current;
        if (app == null)
            return;

        var targetSource = theme switch
        {
            AppTheme.Dark => new Uri($"{ColorsPrefix}Dark.xaml", UriKind.Relative),
            AppTheme.Nexus => new Uri($"{ColorsPrefix}Nexus.xaml", UriKind.Relative),
            _ => new Uri($"{ColorsPrefix}Light.xaml", UriKind.Relative)
        };

        var merged = app.Resources.MergedDictionaries;

        // Replace existing Colors.* dictionary if present
        var existing = merged.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("/Resources/Themes/Colors."));
        if (existing != null)
        {
            existing.Source = targetSource;
            return;
        }

        merged.Add(new ResourceDictionary { Source = targetSource });
    }
}
