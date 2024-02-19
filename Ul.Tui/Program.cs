using System;
using Terminal.Gui;

namespace Ul.Tui;

internal static class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Application.Init ();
        var menu = new MenuBar (new MenuBarItem [] {
            new MenuBarItem ("_File", new MenuItem [] {
                new MenuItem ("_Quit", "", () => {
                    Application.RequestStop ();
                })
            }),
        });

        var button = new Button ("_Hello") {
            X = 0,
            Y = Pos.Bottom (menu),
            Width = Dim.Fill (),
            Height = Dim.Fill () - 1
        };
        button.Clicked += (_,__) => {
            MessageBox.Query (50, 5, "Hi", "Hello World! This is a message box", "Ok");
        };

// Add both menu and win in a single call
        Application.Top.Add (menu, button);
        Application.Run ();
        Application.Shutdown ();
    }
}
