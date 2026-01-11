using System.Windows;
using System.Windows.Input;
using TDL.Configurator.App.Services;
using TDL.Configurator.Core;

namespace TDL.Configurator.App;

public partial class App : System.Windows.Application
{
    private static bool _chromeCommandsRegistered;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        RegisterChromeCommandBindingsOnce();

        var s = AppSettings.Load();
        ThemeManager.ApplyTheme(s.Theme);
        LocalizationManager.ApplyLanguage(s.Language);
    }

    private static void RegisterChromeCommandBindingsOnce()
    {
        if (_chromeCommandsRegistered)
        {
            return;
        }

        _chromeCommandsRegistered = true;

        CommandManager.RegisterClassCommandBinding(
            typeof(Window),
            new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, CanExecuteAlways));

        CommandManager.RegisterClassCommandBinding(
            typeof(Window),
            new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, CanExecuteAlways));

        CommandManager.RegisterClassCommandBinding(
            typeof(Window),
            new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, CanExecuteAlways));

        CommandManager.RegisterClassCommandBinding(
            typeof(Window),
            new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow, CanExecuteAlways));
    }

    private static void CanExecuteAlways(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
        e.Handled = true;
    }

    private static Window? ResolveTargetWindow(object sender, object? parameter)
    {
        if (parameter is Window w)
        {
            return w;
        }

        if (sender is Window w2)
        {
            return w2;
        }

        if (sender is FrameworkElement fe)
        {
            return Window.GetWindow(fe);
        }

        return null;
    }

    private static void OnMinimizeWindow(object sender, ExecutedRoutedEventArgs e)
    {
        var w = ResolveTargetWindow(sender, e.Parameter);
        if (w is null)
        {
            return;
        }

        SystemCommands.MinimizeWindow(w);
        e.Handled = true;
    }

    private static void OnMaximizeWindow(object sender, ExecutedRoutedEventArgs e)
    {
        var w = ResolveTargetWindow(sender, e.Parameter);
        if (w is null)
        {
            return;
        }

        SystemCommands.MaximizeWindow(w);
        e.Handled = true;
    }

    private static void OnRestoreWindow(object sender, ExecutedRoutedEventArgs e)
    {
        var w = ResolveTargetWindow(sender, e.Parameter);
        if (w is null)
        {
            return;
        }

        SystemCommands.RestoreWindow(w);
        e.Handled = true;
    }

    private static void OnCloseWindow(object sender, ExecutedRoutedEventArgs e)
    {
        var w = ResolveTargetWindow(sender, e.Parameter);
        if (w is null)
        {
            return;
        }

        SystemCommands.CloseWindow(w);
        e.Handled = true;
    }
}
