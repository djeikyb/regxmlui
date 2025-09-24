using System.Diagnostics;
using System.Text.RegularExpressions;
using ObservableCollections;
using R3;
using RegXml;

namespace UlReg;

public partial class MainViewModel : IDisposable
{
    private readonly IRegister _register;
    private readonly ObservableList<RegisterEntry> _entries;
    private ISynchronizedView<RegisterEntry, RegisterEntry> _entriesView;

    public MainViewModel(IApplicationService appService, TimeProvider? tp = null, Registers? registers = null)
    {
        tp ??= ObservableSystem.DefaultTimeProvider;

        Topmost = new(false);

        // var registers = Registers.FromXPath();
        registers ??= Registers.FromSax();
        _register = new MultiRegisterRegXmlService(
            registers.Elements,
            registers.Essence,
            registers.Groups,
            registers.Labels,
            registers.Types
        );

        _entries = new ObservableList<RegisterEntry>(_register.All());
        _entriesView = _entries.CreateView(x => x);
        EntriesView = _entriesView.ToNotifyCollectionChanged();

        SearchTerm = new BindableReactiveProperty<string?>(string.Empty);
        SearchUl0 = new();
        SearchUl4 = new();
        SearchUl8 = new();
        SearchUl12 = new();

        Observable
            .Merge(SearchTerm, SearchUl0, SearchUl4, SearchUl8, SearchUl12)
            .Debounce(TimeSpan.FromMilliseconds(200), tp)
            .Subscribe(_ => RefreshTable());

        SelectedRowsCount = new(0);
        SelectedRow = new();

        CopyUlNoPrefixCommand = new();
        CopyUlNoPrefixCommand.Subscribe(_ =>
        {
            Console.WriteLine($"🍝 copyCOPYcopy");

            var re = SelectedRow.Value;
            if (re is null) return;

            appService.SetClipboardText(re.Ul.ToOctets());
        });

        CopyUlWithPrefixCommand = new();
        CopyUlWithPrefixCommand.Subscribe(_ =>
        {
            Console.WriteLine($"🍝 copyCOPYcopy");

            var re = SelectedRow.Value;
            if (re is null) return;

            appService.SetClipboardText(re.Ul.ToUrn());
        });

        CopySymbol = new(_ =>
        {
            var re = SelectedRow.Value;
            if (re is null) return;
            var s = re.Symbol;
            appService.SetClipboardText(s);
        });

        CopyDocument = new ReactiveCommand(_ =>
        {
            var re = SelectedRow.Value;
            if (re is null) return;
            if (re.DefiningDocument != null)
                appService.SetClipboardText(re.DefiningDocument);
        });

        OpenDefiningDocument = new();
        OpenDefiningDocument.Subscribe(_ =>
        {
            var re = SelectedRow.Value;
            if (re is null) return;

            if (re.DefiningDocument is not { } defDoc) return;
            var parsed = ParseDefiningDocumentField(defDoc);
            if (parsed is null) return;
            var (cat, num) = parsed.Value;
            Console.WriteLine($"📂 will try to open: {cat} {num}");

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
                    if (pdf.Contains("-am") && !defDoc.Contains("amendment")) return false;
                    return true;
                })
                .GroupBy(
                    k =>
                    {
                        var fn = Path.GetFileName(k);
                        fn = Regex.Replace(fn, @"^Version_", string.Empty);
                        fn = Regex.Replace(fn, @"^st0", "st");
                        fn = Regex.Replace(fn, @"-20\d\d.pdf", "-20xx.pdf"); // TOneverDO century bug
                        return fn;
                    },
                    v => v)
                .Select(g => g.OrderBy(x => Regex.Match(x, @"-20\d\d.pdf$").Value).Last())
                .Select(x => $"file://{x}")
                .ToList();

            if (files.Count == 1)
            {
                using var __ = Process.Start(new ProcessStartInfo("open", files[0]) { UseShellExecute = true });
            }
            else
            {
                using var __ = Process.Start(new ProcessStartInfo(location) { UseShellExecute = true });
            }

            Console.WriteLine($"\tfound:\n{string.Join('\n', files.Select(x => $"\t\t{x}"))}");
        });
    }

    public BindableReactiveProperty<string?> SearchTerm { get; }
    public BindableReactiveProperty<bool> Topmost { get; }
    public BindableReactiveProperty<string?> SearchUl0 { get; }
    public BindableReactiveProperty<string?> SearchUl4 { get; }
    public BindableReactiveProperty<string?> SearchUl8 { get; }
    public BindableReactiveProperty<string?> SearchUl12 { get; }
    public NotifyCollectionChangedSynchronizedViewList<RegisterEntry> EntriesView { get; }
    public BindableReactiveProperty<int> SelectedRowsCount { get; }
    public BindableReactiveProperty<RegisterEntry?> SelectedRow { get; }
    public ReactiveCommand<Unit> CopyUlNoPrefixCommand { get; }
    public ReactiveCommand<Unit> CopyUlWithPrefixCommand { get; }
    public ReactiveCommand<Unit> CopySymbol { get; }
    public ReactiveCommand<Unit> CopyDocument { get; }
    public ReactiveCommand<Unit> OpenDefiningDocument { get; }

    public void RefreshTable()
    {
        if (SearchUl0.Value is not { Length: > 0 }
            && SearchUl4.Value is not { Length: > 0 }
            && SearchUl8.Value is not { Length: > 0 }
            && SearchUl12.Value is not { Length: > 0 }
            && SearchTerm.Value is not { Length: > 2 })
        {
            _entriesView.ResetFilter();
        }
        else
        {
            _entriesView.AttachFilter(re =>
            {
                var st = SearchTerm.Value;
                var u0 = SearchUl0.Value;
                var u4 = SearchUl4.Value;
                var u8 = SearchUl8.Value;
                var u12 = SearchUl12.Value;
                return Filter(re, st, u0, u4, u8, u12);
            });
        }
    }

    public static bool Filter(
        RegisterEntry re,
        string? term,
        string? u0 = null,
        string? u4 = null,
        string? u8 = null,
        string? u12 = null
    )
    {
        var octetsMatch = true;
        if (u0 is { Length: > 0 }) octetsMatch = octetsMatch && re.Ul._s_oct0.StartsWith(u0, StringComparison.OrdinalIgnoreCase);
        if (u4 is { Length: > 0 })
        {
            if (u4.StartsWith("0253"))
                u4 = "027f" + u4.Substring(4);
            octetsMatch = octetsMatch && re.Ul._s_oct4.StartsWith(u4, StringComparison.OrdinalIgnoreCase);
        }

        if (u8 is { Length: > 0 }) octetsMatch = octetsMatch && re.Ul._s_oct8.StartsWith(u8, StringComparison.OrdinalIgnoreCase);
        if (u12 is { Length: > 0 }) octetsMatch = octetsMatch && re.Ul._s_oct12.StartsWith(u12, StringComparison.OrdinalIgnoreCase);

        bool termMatches;
        if (term is not { Length: > 2 })
        {
            termMatches = true;
        }
        else
        {
            if (term.StartsWith("urn:smpte:ul:", StringComparison.OrdinalIgnoreCase))
                term = term.Substring(13);
            else if (term.StartsWith("060e2b34.0253", StringComparison.OrdinalIgnoreCase))
                term = "060e2b34.027f" + term.Substring(13);

            var t = Regex.Replace(term, "-|:| ", ".");
            termMatches = re.Symbol.StartsWith(term, StringComparison.InvariantCultureIgnoreCase);

            if (term is { Length: <= 8 })
            {
                termMatches = termMatches || re.Ul._s_oct0.StartsWith(t);
                termMatches = termMatches || re.Ul._s_oct4.StartsWith(t);
                termMatches = termMatches || re.Ul._s_oct8.StartsWith(t);
                termMatches = termMatches || re.Ul._s_oct12.StartsWith(t);
            }

            if (term is { Length: > 4 and <= 35 })
            {
                termMatches = termMatches || re.Ul.ToOctets().Contains(t, StringComparison.InvariantCultureIgnoreCase);
            }

            if (term is { Length: >= 3 })
            {
                termMatches = termMatches || re.Symbol.Contains(term, StringComparison.InvariantCultureIgnoreCase);
            }

            if (term is { Length: >= 3 })
            {
                termMatches = termMatches ||
                              re.DefiningDocument != null &&
                              re.DefiningDocument.Contains(term, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        return octetsMatch && termMatches;
    }

    private (string cat, string num)? ParseDefiningDocumentField(string defdoc)
    {
        if ("377M-1".Equals(defdoc)) return ("ST", "377-1");
        if ("SMPTE 377M".Equals(defdoc)) return ("ST", "377-1");
        if ("SMPTE 377M-1".Equals(defdoc)) return ("ST", "377-1");
        var m = RegexDefiningDocParserBasic().Match(defdoc);

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
    private static partial Regex RegexDefiningDocParserBasic();

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

    public void Dispose()
    {
        SearchTerm.Dispose();
        Topmost.Dispose();
        SearchUl0.Dispose();
        SearchUl4.Dispose();
        SearchUl8.Dispose();
        SearchUl12.Dispose();
        SelectedRowsCount.Dispose();
    }
}
