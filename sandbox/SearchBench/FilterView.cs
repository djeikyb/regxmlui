using ObservableCollections;
using R3;
using RegXml;
using UlReg;

namespace SearchBench;

public class FilterView : IMvm
{
    private readonly MultiRegisterRegXmlService _register;
    private readonly ObservableList<RegisterEntry> _entries;
    private readonly ISynchronizedView<RegisterEntry, RegisterEntry> _entriesView;

    public FilterView(MultiRegisterRegXmlService register)
    {
        _register = register;
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
            var found = _register.Search(
                SearchTerm.Value,
                SearchUl0.Value, SearchUl4.Value,
                SearchUl8.Value, SearchUl12.Value);
            var d = found.ToDictionary(x => x.Ul.ToOctets());
            if (d.Count == 0) _entriesView.AttachFilter(_ => false);
            else _entriesView.AttachFilter((re) => d.ContainsKey(re.Ul.ToOctets()));
        }
    }

    public BindableReactiveProperty<string?> SearchTerm { get; }
    public BindableReactiveProperty<string?> SearchUl0 { get; }
    public BindableReactiveProperty<string?> SearchUl4 { get; }
    public BindableReactiveProperty<string?> SearchUl8 { get; }
    public BindableReactiveProperty<string?> SearchUl12 { get; }
    public NotifyCollectionChangedSynchronizedViewList<RegisterEntry> Entries { get; set; }
}
