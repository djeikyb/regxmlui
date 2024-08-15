using System.Diagnostics;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Input;
using R3;
using UlReg.ViewModels;

namespace UlReg.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        // this.DataContextChanged += (sender, args) =>
        // {
        //     var view = (MainView)sender!;
        //     var main = (MainViewModel)view.DataContext!;
        //     var vm = main.TryMeVm;
        //     var binding = new Binding { Source = vm.SearchTerm, Path = nameof(vm.SearchTerm.Value) };
        //     InputSearchTerm.Bind(TextBlock.TextProperty, binding);
        // };

        Observable
            .FromEvent<EventHandler<KeyEventArgs>, KeyEventArgs>(
                h => (sender, e) => h(e),
                e => DataGrid.KeyDown += e,
                e => DataGrid.KeyDown -= e)
            .Where(e =>
            {
                var hotkeys = TopLevel.GetTopLevel(this)?.PlatformSettings?.HotkeyConfiguration;
                return hotkeys is not null && hotkeys.Copy.Any(g => g.Matches(e));
            })
            .Subscribe(e =>
            {
                Console.WriteLine($"⚡️ {nameof(DataGrid.KeyDown)}");
                var vm = (MainViewModel?)DataContext;
                if (vm == null) return;
                vm.CopyCommand.Execute(new Unit());
                e.Handled = true;
            });


        Observable
            .FromEvent<EventHandler<DataGridCellPointerPressedEventArgs>, DataGridCellPointerPressedEventArgs>(
                h => (sender, e) => h(e),
                e => DataGrid.CellPointerPressed += e,
                e => DataGrid.CellPointerPressed -= e)
            .Where(e => e.Column.DisplayIndex == 2)
            .Where(e => e.PointerPressedEventArgs.ClickCount == 2)
            .Subscribe(e =>
            {
                var tb = (TextBlock?)e.Cell.Content;
                var maybeDoc = tb?.Text;
                if (maybeDoc is null) return;

                var parsed = ParseDefiningDocumentField(maybeDoc);
                if (parsed is null) return;
                var (cat, num) = parsed.Value;
                Console.WriteLine($"⚡️ double-clicked ({e.PointerPressedEventArgs.ClickCount}) a doc! {cat} {num}");

                var location = $"/Users/jacob/www/smpte/pub.smpte.org/pub.smpte.org/doc/{cat}{num}";
                var files = Directory.EnumerateFiles(location, "*.pdf",
                        new EnumerationOptions
                        {
                            RecurseSubdirectories = true,
                            MatchCasing = MatchCasing.CaseInsensitive,
                            MatchType = MatchType.Simple,
                        })
                    .Where(pdf =>
                    {
                        // if the regxml defining document says "amendment"
                        // then go ahead and include amendment pdfs
                        // but otherwise
                        // they're just cluttering up the result space
                        // so exclude 'em
                        if (pdf.Contains("-am") && !maybeDoc.Contains("amendment")) return false;
                        return true;
                    })
                    .GroupBy(
                        k =>
                        {
                            var fn = Path.GetFileName(k);
                            fn = Regex.Replace(fn, @"^st0", "st");
                            fn = Regex.Replace(fn, @"-20\d\d.pdf", "-20xx.pdf"); // TOneverDO century bug
                            return fn;
                        },
                        v => v)
                    .Select(g => g.Last())
                    .Select(x => $"file://{x}")
                    .ToList();

                if (files.Count == 1)
                {
                    using var _ = Process.Start(new ProcessStartInfo("open", files[0]) { UseShellExecute = true });
                }
                else
                {
                    using var _ = Process.Start(new ProcessStartInfo(location) { UseShellExecute = true });
                }

                Console.WriteLine($"\tfound:\n{string.Join('\n', files.Select(x => $"\t\t{x}"))}");

                // if (maybeDoc != null && maybeDoc.StartsWith("SMPTE"))
                // {
                //     var r = new Regex(".*SMPTE ?(?:number.*) ?.*$",
                //         RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                //     var location = "/Users/jacob/www/smpte/pub.smpte.org/pub.smpte.org/doc";
                //     Directory.EnumerateFiles(location, "**.pdf", SearchOption.AllDirectories);
                // }
            });
    }

    private (string cat, string num)? ParseDefiningDocumentField(string defdoc)
    {
        if ("377M-1".Equals(defdoc)) return ("ST", "377-1");
        if ("SMPTE 377M".Equals(defdoc)) return ("ST", "377-1");
        if ("SMPTE 377M-1".Equals(defdoc)) return ("ST", "377-1");
        var m = SmpteDocNameBasic().Match(defdoc);

        if (m.Groups["cat"].Captures.Count > 1)
        {
            Console.WriteLine("⛔️ found more than one cat");
            return null;
        }

        if (m.Groups["num"].Captures.Count > 1)
        {
            Console.WriteLine("⛔️ found more than one spec number");
            return null;
        }

        if (m.Groups["cat"].Captures.Count == 0)
        {
            Console.WriteLine("⛔️ missing cat");
            return null;
        }

        if (m.Groups["num"].Captures.Count == 0)
        {
            Console.WriteLine("⛔️ missing spec number");
            return null;
        }

        string cat = m.Groups["cat"].Value switch
        {
            "SMPTE" => "ST",
            { } s => s,
        };

        string num = m.Groups["num"].Value switch
        {
            { } s when s.Contains('.') => s.Replace('.', '-'),
            { } s => s,
        };

        return (cat, num);
    }

    [GeneratedRegex(
        pattern:
        @"^.*(?<cat>SMPTE|ST|RDD|RP) ?(?<num>\d+(-|\.)?\d*).*$",
        options: RegexOptions.IgnoreCase,
        cultureName: "en-US"
    )]
    private static partial Regex SmpteDocNameBasic();

    [GeneratedRegex(
        pattern:
        @"(?<whole>SMPTE (?<cat>ST|RP|RDD) (?<num>\d+-?\d*).*)",
        options: RegexOptions.IgnoreCase,
        cultureName: "en-US"
    )]
    private static partial Regex SmpteDocNames1();


    // starts with smpte, maybe has a category
    // like
    // - SMPTE ST2067-2
    // - SMPTE RP2089
    // - SMPTE 430-6 D-Cinema Auditorium Security Messages
    // - SMPTE 429-6
    [GeneratedRegex(
        pattern:
        @"(?<whole>SMPTE (?<cat>ST|RP|RDD)?(?<num>\d+[a-zA-Z]*-?\d*).*)",
        options: RegexOptions.IgnoreCase,
        cultureName: "en-US"
    )]
    private static partial Regex SmpteDocNames2();


    // eg | engineering guideline
    // rdd | registered disclosure document
    // rp | recommended practice
    // ov | overview
    // st | standard
    //
    //  59 eg
    //  17 ov
    //  52 rdd
    // 231 rp
    // 526 st
    //
    // so the common forms should be (extra text could be at start or end):
    //
    // SMPTE 123
    // SMPTE 123.4
    // SMPTE 123-4
    //
    // SMPTE (RDD|RP|ST) 123
    // SMPTE (RDD|RP|ST) 123.4
    // SMPTE (RDD|RP|ST) 123-4
    //
    // so maybe
    //
    // ^.*(?<cat>SMPTE|ST|RDD|RP) ?(?<num>\d+.*?)$
    //
    // .*SMPTE ST \d+.*? ?
    // .*SMPTE RDD \d+.*? ?
}
