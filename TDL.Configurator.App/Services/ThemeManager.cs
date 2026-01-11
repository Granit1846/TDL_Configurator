using System;
using System.Linq;
using System.Windows;
using TDL.Configurator.Core;

namespace TDL.Configurator.App.Services;

public static class ThemeManager
{
    private const string ColorsPrefix = "/Resources/Themes/Colors.";

    public static void ApplyTheme(AppTheme theme)
    {
        var app = System.Windows.Application.Current;
        if (app == null)
            return;

        var targetSource = theme switch
        {
            AppTheme.Dark => new Uri($"{ColorsPrefix}Dark.xaml", UriKind.RelativeOrAbsolute),
            AppTheme.Nexus => new Uri($"{ColorsPrefix}Nexus.xaml", UriKind.RelativeOrAbsolute),
            _ => new Uri($"{ColorsPrefix}Light.xaml", UriKind.RelativeOrAbsolute)
        };

        var merged = app.Resources.MergedDictionaries;

        // 1) Remove ALL existing Colors.* dictionaries (otherwise Light can stay "active").
        for (var i = merged.Count - 1; i >= 0; i--)
        {
            var src = merged[i].Source?.OriginalString ?? string.Empty;

            // Match both short and full pack URIs
            if (src.Contains("/Resources/Themes/Colors.", StringComparison.OrdinalIgnoreCase) ||
                src.Contains("Resources/Themes/Colors.", StringComparison.OrdinalIgnoreCase) ||
                src.Contains("Colors.Light.xaml", StringComparison.OrdinalIgnoreCase) ||
                src.Contains("Colors.Dark.xaml", StringComparison.OrdinalIgnoreCase) ||
                src.Contains("Colors.Nexus.xaml", StringComparison.OrdinalIgnoreCase))
            {
                merged.RemoveAt(i);
            }
        }

        // 2) Add the selected theme dictionary as the only Colors provider.
        merged.Add(new ResourceDictionary { Source = targetSource });
    }
}
