using System.Collections.ObjectModel;
using R3;
using RegXml;
using UlRegBiz.Model.Services;
using UlRegBiz.Model.Xml;
using UlRegBiz.Services;

namespace UlReg.ViewModels;

public class MainViewModel : IDisposable
{
    private readonly IRegisterService _register;

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
        Entries = new ObservableCollection<RegisterEntryViewModel>(
            _register.All().Select(re => new RegisterEntryViewModel(re))
        );

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

    public ObservableCollection<RegisterEntryViewModel> Entries { get; init; }

    internal void RefreshTable()
    {
        Entries.Clear();

        if (SearchUl0.Value is not { Length: > 0 }
            && SearchUl4.Value is not { Length: > 0 }
            && SearchUl8.Value is not { Length: > 0 }
            && SearchUl12.Value is not { Length: > 0 }
            && SearchTerm.Value is not { Length: > 2 })
        {
            foreach (var re in _register.All()) Entries.Add(new RegisterEntryViewModel(re));
        }
        else
        {
            foreach (var vm in _register.Search(SearchTerm.Value, SearchUl0.Value, SearchUl4.Value, SearchUl8.Value, SearchUl12.Value)
                         .Select(re => new RegisterEntryViewModel(re)))
            {
                Entries.Add(vm);
            }
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

public class RegisterEntryViewModel
{
    private readonly RegisterEntry _re;

    public string? Ul { get; }

    public string? Register { get; }

    public string? DefiningDocument { get; }

    public string? Symbol { get; }

    public RegisterEntryViewModel()
    {
    }

    public RegisterEntryViewModel(RegisterEntry re)
    {
        _re = re;
        Ul = re.Ul.ToOctets();
        Register = re.Register;
        DefiningDocument = re.DefiningDocument;
        Symbol = re.Symbol;
    }
}
