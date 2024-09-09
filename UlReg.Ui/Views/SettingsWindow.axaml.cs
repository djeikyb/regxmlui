using System.Runtime.Loader;
using Avalonia.Controls;
using Avalonia.Input;
using R3;

namespace UlReg.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        var hideCommand = new ReactiveCommand<Unit>();
        hideCommand.Subscribe(_ =>
        {
            Hide();
        });

        KeyBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.W, KeyModifiers.Meta),
            Command = hideCommand,
        });

        Opened += (sender, e) =>
        {
            Console.WriteLine("⚡️ opened!");
            Focusable = true;
            Focus();
        };


        Closing += (sender, e) =>
        {
            if (sender is SettingsWindow sw && e.CloseReason == WindowCloseReason.WindowClosing)
            {
                sw.Hide();
                e.Cancel = true;
            }
        };
    }
}
