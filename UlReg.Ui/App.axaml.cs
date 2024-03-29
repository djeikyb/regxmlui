using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using HotAvalonia;
using Microsoft.Extensions.DependencyInjection;
using UlReg.Model.Services;
using UlReg.ViewModels;
using UlReg.Views;

namespace UlReg;

public partial class App : Application
{
    // private IDisposable? _settingsDisposable;

    public App()
    {
    }

    public static void ConfigureDesktopServices()
    {
        IServiceCollection services = new ServiceCollection();

        // Services
        // serviceCollection.AddSingleton<IStorageFactory, ApplicationDataStorageFactory>();
        // serviceCollection.AddSingleton<IApplicationService, ApplicationService>();
        // serviceCollection.AddSingleton<IPluginsService, PluginsService>();
        // serviceCollection.AddSingleton<IChatSerializer, SystemTextJsonChatSerializer>();
        // serviceCollection.AddSingleton<IChatService, ChatService>();
        // serviceCollection.AddSingleton<ICompletionsService, CompletionsService>();
        // serviceCollection.AddSingleton<MainViewModel>();
        // serviceCollection.AddSingleton<IPluginContext>(x => x.GetRequiredService<MainViewModel>());

        var gitlabToken = Environment.GetEnvironmentVariable("GITLAB_TOKEN") // TODO this sucks lol
                          ?? "invalid token"; // TODO this sucks even more

        // ViewModels
        // serviceCollection.AddTransient<ChatMessageViewModel>();
        // serviceCollection.AddTransient<ChatSettingsViewModel>();
        // serviceCollection.AddTransient<ChatResultViewModel>();
        // serviceCollection.AddTransient<ChatViewModel>();
        // serviceCollection.AddTransient<PromptViewModel>();
        // serviceCollection.AddTransient<WorkspaceViewModel>();
        // serviceCollection.AddTransient<WindowLayoutViewModel>();
        services.AddTransient<MainViewModel>();

        // Plugins
        // serviceCollection.AddTransient<IChatPlugin, ClipboardListenerChatPlugin>();
        // serviceCollection.AddTransient<IChatPlugin, DummyChatPlugin>();

        Defaults.Locator.ConfigureServices(services.BuildServiceProvider());
    }

    public static void ConfigureMobileServices()
    {
        IServiceCollection serviceCollection = new ServiceCollection();

        // Services
        // serviceCollection.AddSingleton<IStorageFactory, IsolatedStorageFactory>();
        // serviceCollection.AddSingleton<IApplicationService, ApplicationService>();
        // serviceCollection.AddSingleton<IPluginsService, PluginsService>();
        // serviceCollection.AddSingleton<IChatSerializer, SystemTextJsonChatSerializer>();
        // serviceCollection.AddSingleton<IChatService, ChatService>();
        // serviceCollection.AddSingleton<ICompletionsService, CompletionsService>();
        // serviceCollection.AddSingleton<MainViewModel>();
        // serviceCollection.AddSingleton<IPluginContext>(x => x.GetRequiredService<MainViewModel>());

        // ViewModels
        // serviceCollection.AddTransient<ChatMessageViewModel>();
        // serviceCollection.AddTransient<ChatSettingsViewModel>();
        // serviceCollection.AddTransient<ChatResultViewModel>();
        // serviceCollection.AddTransient<ChatViewModel>();
        // serviceCollection.AddTransient<PromptViewModel>();
        // serviceCollection.AddTransient<WorkspaceViewModel>();
        // serviceCollection.AddTransient<WindowLayoutViewModel>();

        // Plugins
        // serviceCollection.AddTransient<IChatPlugin, ClipboardListenerChatPlugin>();
        // serviceCollection.AddTransient<IChatPlugin, DummyChatPlugin>();

        Defaults.Locator.ConfigureServices(serviceCollection.BuildServiceProvider());
    }

    public override void Initialize()
    {
        this.EnableHotReload(); // MUST precede AvaloniaXamlLoader.Load
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        // Defaults.Locator.GetService<IPluginsService>()?.DiscoverPlugins();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = Defaults.Locator.GetService<MainViewModel>()
            };
            desktop.MainWindow = mainWindow;

            mainWindow.Closing += MainWindowOnClosing;
            
            desktop.Exit += DesktopOnExit;

            await LoadWindowLayoutAsync(mainWindow);

            await InitSettingsAsync();
            // SetTheme();

            // TODO: Enable plugins.
            // Defaults.Locator.GetService<IPluginsService>()?.InitPlugins();
            // Defaults.Locator.GetService<IPluginsService>()?.StartPlugins();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            single.MainView = new MainView
            {
                DataContext = Defaults.Locator.GetService<MainViewModel>()
            };

            await InitSettingsAsync();
            // SetTheme();
        }

        /*
        _settingsDisposable = DispatcherTimer.Run(
            () =>
            {
                Task.Run(async () => await SaveSettings());
                return true;
            }, 
            TimeSpan.FromSeconds(5));
        */

        base.OnFrameworkInitializationCompleted();
    }

    private async Task InitSettingsAsync()
    {
        try
        {
            await LoadSettingsAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public Task LoadWindowLayoutAsync(Window window)
    {
        // var factory = Defaults.Locator.GetService<IStorageFactory>();
        // var storage = factory?.CreateStorageService<WindowLayoutViewModel>();
        // if (storage is null)
        // {
        //     return;
        // }
        // var layout = await storage.LoadObjectAsync(
        //     "WindowLayout",
        //     WindowLayoutViewModelJsonContext.s_instance.WindowLayoutViewModel);
        // if (layout is { })
        // {
        //     window.Position = new PixelPoint(layout.X, layout.Y);
        //
        //     window.Width = layout.Width;
        //
        //     window.Height = layout.Height;
        //
        //     if (layout.WindowState is { })
        //     {
        //         if (Enum.TryParse<WindowState>(layout.WindowState, out var windowState))
        //         {
        //             window.WindowState = windowState;
        //         }
        //     }
        //
        //     if (layout.WindowStartupLocation is { })
        //     {
        //         if (Enum.TryParse<WindowState>(layout.WindowState, out var windowState))
        //         {
        //             window.WindowState = windowState;
        //         }
        //     }
        //
        //     window.Topmost = layout.Topmost;
        // }
        return Task.CompletedTask;
    }

    public Task SaveWindowLayoutAsync(Window window)
    {
        // var workspace = new WindowLayoutViewModel
        // {
        //     X = window.Position.X,
        //     Y = window.Position.Y,
        //     Width = window.Width,
        //     Height = window.Height,
        //     WindowState = window.WindowState.ToString(),
        //     WindowStartupLocation = WindowStartupLocation.Manual.ToString(),
        //     Topmost = window.Topmost
        // };
        //
        // var factory = Defaults.Locator.GetService<IStorageFactory>();
        // var storage = factory?.CreateStorageService<WindowLayoutViewModel>();
        // if (storage is { })
        // {
        //     await storage.SaveObjectAsync(
        //         workspace,
        //         "WindowLayout",
        //         WindowLayoutViewModelJsonContext.s_instance.WindowLayoutViewModel);
        // }
        return Task.CompletedTask;
    }

    private async void MainWindowOnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (sender is Window window)
        {
            await SaveWindowLayoutAsync(window);
        }
    }

    private void DesktopOnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        try
        {
            // _settingsDisposable?.Dispose();
            
            // Defaults.Locator.GetService<IPluginsService>()?.ShutdownPlugins();

            // SaveTheme();
            SaveSettings();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public Task LoadSettingsAsync()
    {
        // var mainViewModel = Defaults.Locator.GetService<MainViewModel>();
        // if (mainViewModel is null)
        // {
        //     return;
        // }
        // await mainViewModel.LoadSettingsAsync();
        return Task.CompletedTask;
    }

    public Task SaveSettingsAsync()
    {
        // var mainViewModel = Defaults.Locator.GetService<MainViewModel>();
        // if (mainViewModel is null)
        // {
        //     return;
        // }
        //
        // await mainViewModel.SaveSettingsAsync();
        return Task.CompletedTask;
    }

    public void LoadSettings()
    {
        // var mainViewModel = Defaults.Locator.GetService<MainViewModel>();
        // if (mainViewModel is null)
        // {
        //     return;
        // }
        // mainViewModel.LoadSettings();
    }

    public void SaveSettings()
    {
        // var mainViewModel = Defaults.Locator.GetService<MainViewModel>();
        // if (mainViewModel is null)
        // {
        //     return;
        // }
        //
        // mainViewModel.SaveSettings();
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

    public void ToggleWindowState()
    {
        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow is MainWindow mainWindow)
            {
                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.ShowInTaskbar = true;
                    mainWindow.Show();
                }
                else
                {
                    mainWindow.WindowState = WindowState.Minimized;
                    mainWindow.ShowInTaskbar = false;
                    mainWindow.Hide();
                }
            }
        }
    }

    private void TrayIcon_OnClicked(object? sender, EventArgs e)
    {
        ToggleWindowState();
    }

    private void ToggleShow_OnClick(object? sender, EventArgs e)
    {
        ToggleWindowState();
    }

    private void ToggleAcrylic_OnClick(object? sender, EventArgs e)
    {
        ToggleAcrylicBlur();
    }

    private void Quit_OnClick(object? sender, EventArgs e)
    {
        var app = Defaults.Locator.GetService<IApplicationService>();
        if (app is { })
        {
            app.Exit();
        }
    }
}
