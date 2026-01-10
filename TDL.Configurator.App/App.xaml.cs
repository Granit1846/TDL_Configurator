using System.Windows;
using TDL.Configurator.App.Services;
using TDL.Configurator.Core;

namespace TDL.Configurator.App;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var s = AppSettings.Load();
        ThemeManager.ApplyTheme(s.Theme);
        LocalizationManager.ApplyLanguage(s.Language);
    }
}
