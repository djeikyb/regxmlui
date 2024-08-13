using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using HotAvalonia;
using UlReg.ViewModels;
using UlReg.Views;

namespace UlReg;

public class App : Application
{
    public override void Initialize()
    {
        this.EnableHotReload(); // MUST precede AvaloniaXamlLoader.Load
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            desktop.MainWindow = mainWindow;
        }
        else
        {
            throw new NotSupportedException($"{nameof(ApplicationLifetime)} is not supported: {ApplicationLifetime?.GetType().FullName}");
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void ToggleAcrylicBlur()
    {
        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow is MainWindow mainWindow)
            {
                if (mainWindow.TransparencyLevelHint.Contains(WindowTransparencyLevel.AcrylicBlur))
                {
                    mainWindow.SystemDecorations = SystemDecorations.None;
                    mainWindow.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
                    mainWindow.ExtendClientAreaToDecorationsHint = false;
                    mainWindow.TransparencyLevelHint = new [] { WindowTransparencyLevel.Transparent };
                    mainWindow.AcrylicBorder.IsVisible = false;
                }
                else
                {
                    mainWindow.SystemDecorations = SystemDecorations.Full;
                    mainWindow.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome;
                    mainWindow.ExtendClientAreaToDecorationsHint = true;
                    mainWindow.TransparencyLevelHint = new []{ WindowTransparencyLevel.AcrylicBlur };
                    mainWindow.AcrylicBorder.IsVisible = true;
                }
            }
        }
    }

}
