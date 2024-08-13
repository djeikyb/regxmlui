using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using R3;

namespace UlReg.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif

        KeyBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.A, KeyModifiers.Control | KeyModifiers.Shift),
            Command = new ReactiveCommand<Unit>(_ =>
            {
                if (Application.Current is App application)
                {
                    application.ToggleAcrylicBlur();
                }
            })
        });

    }
}
