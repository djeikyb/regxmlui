using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RegXml;
using UlRegBiz.Model.Services;
using UlRegBiz.Model.Xml;
using UlRegBiz.Services;

namespace UlReg.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _topmost;

    [ObservableProperty]
    private string? _searchTerm;

    [ObservableProperty]
    private string? _searchUl0;

    [ObservableProperty]
    private string? _searchUl4;

    [ObservableProperty]
    private string? _searchUl8;

    [ObservableProperty]
    private string? _searchUl12;

    private readonly IRegisterService _register;

    public MainViewModel()
    {
        Topmost = false;
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
    }

    public ObservableCollection<RegisterEntryViewModel> Entries { get; init; }

    partial void OnSearchTermChanged(string? _) => RefreshTable();
    partial void OnSearchUl0Changed(string? _) => RefreshTable();
    partial void OnSearchUl4Changed(string? _) => RefreshTable();
    partial void OnSearchUl8Changed(string? _) => RefreshTable();
    partial void OnSearchUl12Changed(string? _) => RefreshTable();

    private void RefreshTable()
    {
        Entries.Clear();

        if (SearchUl0 is not { Length: > 0 }
            && SearchUl4 is not { Length: > 0 }
            && SearchUl8 is not { Length: > 0 }
            && SearchUl12 is not { Length: > 0 }
            && SearchTerm is not { Length: > 2 })
        {
            foreach (var re in _register.All()) Entries.Add(new RegisterEntryViewModel(re));
        }
        else
        {
            foreach (var vm in _register.Search(SearchTerm, SearchUl0, SearchUl4, SearchUl8, SearchUl12)
                         .Select(re => new RegisterEntryViewModel(re)))
            {
                Entries.Add(vm);
            }
        }
    }
}

public partial class RegisterEntryViewModel : ObservableObject
{
    private readonly RegisterEntry _re;

    [ObservableProperty]
    private string? _ul;

    [ObservableProperty]
    private string? _register;

    [ObservableProperty]
    private string? _definingDocument;

    [ObservableProperty]
    private string? _symbol;

    public RegisterEntryViewModel()
    {
    }

    public RegisterEntryViewModel(RegisterEntry re)
    {
        _re = re;
        _ul = re.Ul.ToOctets();
        _register = re.Register;
        _definingDocument = re.DefiningDocument;
        _symbol = re.Symbol;
    }
}
