using Avalonia.Controls;
using Avalonia.Input;
using R3;

namespace UlReg.Views;

public partial class MainWindow : Window
{
    private readonly double _defaultFontSize;
    private readonly SettingsWindow _sw;

    public MainWindow()
    {
        InitializeComponent();

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

        _sw = new SettingsWindow();
        // _sw.DataContext = DataContext;
        var reactiveCommand = new ReactiveCommand<Unit>();
        reactiveCommand.Subscribe(_ =>
        {
            _sw.DataContext = this.DataContext;
            _sw.Show(this);
        });
        KeyBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.OemComma, KeyModifiers.Meta),
            Command = reactiveCommand,
        });
    }
}
