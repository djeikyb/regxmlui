using ObservableCollections;
using R3;
using RegXml;
using UlRegBiz.Model.Services;
using UlRegBiz.Services;

namespace UlReg.ViewModels;

public class MainViewModel : IDisposable
{
    private readonly IRegisterService _register;
    private readonly ObservableList<RegisterEntryViewModel> _entries;

    public MainViewModel()
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
    }

    public BindableReactiveProperty<string?> SearchTerm { get; }
    public BindableReactiveProperty<bool> Topmost { get; }
    public BindableReactiveProperty<string?> SearchUl0 { get; }
    public BindableReactiveProperty<string?> SearchUl4 { get; }
    public BindableReactiveProperty<string?> SearchUl8 { get; }
    public BindableReactiveProperty<string?> SearchUl12 { get; }
    public INotifyCollectionChangedSynchronizedView<RegisterEntryViewModel> EntriesView { get; }

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
