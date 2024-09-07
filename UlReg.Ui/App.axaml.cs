using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HotAvalonia;
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
            var mainWindow = new MainWindow();
            mainWindow.DataContext = new Shim(new ApplicationService(mainWindow));
            desktop.MainWindow = mainWindow;
        }
        else
        {
            throw new NotSupportedException($"{nameof(ApplicationLifetime)} is not supported: {ApplicationLifetime?.GetType().FullName}");
        }

        base.OnFrameworkInitializationCompleted();
    }

}
