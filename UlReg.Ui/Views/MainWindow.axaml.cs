using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using R3;

namespace UlReg.Views;

public partial class MainWindow : Window
{
    private double _defaultFontSize;

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

        _defaultFontSize = FontSize;

        KeyBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.OemPlus, KeyModifiers.Meta),
            Command = new ReactiveCommand<Unit>(_ => FontSize += 0.5),
        });

        KeyBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.OemMinus, KeyModifiers.Meta),
            Command = new ReactiveCommand<Unit>(_ => FontSize -= 0.5),
        });

        KeyBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.D0, KeyModifiers.Meta),
            Command = new ReactiveCommand<Unit>(_ => FontSize = _defaultFontSize),
        });
    }
}
