using System.Diagnostics;
using System.Text.RegularExpressions;
using ObservableCollections;
using R3;
using RegXml;
using UlReg.Model.Services;
using UlRegBiz.Model.Services;
using UlRegBiz.Services;

namespace UlReg.ViewModels;

public partial class MainViewModel : IDisposable
{
    private readonly IRegisterService _register;
    private readonly ObservableList<RegisterEntryViewModel> _entries;

    public MainViewModel(IApplicationService appService)
    {
        Topmost = new(false);

        var registers = Registers.FromEmbedded();
        _register = new MultiRegisterRegXmlService(
            new RegXmlService(registers.Elements.Xml),
            new RegXmlService(registers.Essence.Xml),
            new RegXmlService(registers.Groups.Xml),
            new RegXmlService(registers.Labels.Xml),
            new RegXmlService(registers.Types.Xml)
        );

        _entries = new ObservableList<RegisterEntryViewModel>(
            _register.All().Select(re => new RegisterEntryViewModel(re))
        );
        EntriesView = _entries.CreateView(x => x).ToNotifyCollectionChanged();

        SearchTerm = new BindableReactiveProperty<string?>(string.Empty);
        SearchUl0 = new();
        SearchUl4 = new();
        SearchUl8 = new();
        SearchUl12 = new();

        Observable
            .Merge(SearchTerm, SearchUl0, SearchUl4, SearchUl8, SearchUl12)
            .Debounce(TimeSpan.FromMilliseconds(200))
            .Subscribe(_ => RefreshTable());

        SelectedRowIndex = new();
        CopyCommand = new();
        CopyCommand.Subscribe(re =>
        {
            Console.WriteLine($"🍝 copyCOPYcopy");
            if (re.Ul is null) return;
            appService.SetClipboardText(re.Ul);
        });
        OpenDefiningDocument = new();
        OpenDefiningDocument.Subscribe(re =>
        {
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
                using var _ = Process.Start(new ProcessStartInfo("open", files[0]) { UseShellExecute = true });
            }
            else
            {
                using var _ = Process.Start(new ProcessStartInfo(location) { UseShellExecute = true });
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
    public NotifyCollectionChangedSynchronizedViewList<RegisterEntryViewModel> EntriesView { get; }
    public BindableReactiveProperty<int> SelectedRowIndex { get; }
    public ReactiveCommand<RegisterEntryViewModel> CopyCommand { get; }
    public ReactiveCommand<RegisterEntryViewModel> OpenDefiningDocument { get; }

    internal void RefreshTable()
    {
        _entries.Clear();

        if (SearchUl0.Value is not { Length: > 0 }
            && SearchUl4.Value is not { Length: > 0 }
            && SearchUl8.Value is not { Length: > 0 }
            && SearchUl12.Value is not { Length: > 0 }
            && SearchTerm.Value is not { Length: > 2 })
        {
            _entries.AddRange(_register.All().Select(re => new RegisterEntryViewModel(re)));
        }
        else
        {
            _entries.AddRange(
                _register.Search(
                        SearchTerm.Value,
                        SearchUl0.Value, SearchUl4.Value,
                        SearchUl8.Value, SearchUl12.Value)
                    .Select(re => new RegisterEntryViewModel(re)));
        }
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
    }
}
