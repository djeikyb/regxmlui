using System.Text.RegularExpressions;
using ObservableCollections;
using R3;
using RegXml;
using UlReg;

namespace SearchBench;

public class FilterRawBools : IMvm
{
    private readonly ObservableList<RegisterEntry> _entries;
    private readonly ISynchronizedView<RegisterEntry, RegisterEntry> _entriesView;

    public FilterRawBools(MultiRegisterRegXmlService register)
    {
        _entries = new ObservableList<RegisterEntry>(register.All());

        _entriesView = _entries.CreateView(x => x);
        Entries = _entriesView.ToNotifyCollectionChanged();

        SearchTerm = new BindableReactiveProperty<string?>(string.Empty);
        SearchUl0 = new();
        SearchUl4 = new();
        SearchUl8 = new();
        SearchUl12 = new();
    }

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
        if (u4 is { Length: > 0 }) octetsMatch = octetsMatch && re.Ul._s_oct4.StartsWith(u4, StringComparison.OrdinalIgnoreCase);
        if (u8 is { Length: > 0 }) octetsMatch = octetsMatch && re.Ul._s_oct8.StartsWith(u8, StringComparison.OrdinalIgnoreCase);
        if (u12 is { Length: > 0 }) octetsMatch = octetsMatch && re.Ul._s_oct12.StartsWith(u12, StringComparison.OrdinalIgnoreCase);

        bool termMatches;
        if (term is not { Length: > 2 })
        {
            termMatches = true;
        }
        else
        {
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

    public BindableReactiveProperty<string?> SearchTerm { get; }
    public BindableReactiveProperty<string?> SearchUl0 { get; }
    public BindableReactiveProperty<string?> SearchUl4 { get; }
    public BindableReactiveProperty<string?> SearchUl8 { get; }
    public BindableReactiveProperty<string?> SearchUl12 { get; }
    public NotifyCollectionChangedSynchronizedViewList<RegisterEntry> Entries { get; set; }
}

