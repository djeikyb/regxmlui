using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using R3;
using RegXml;

namespace UlReg.Views;

public class Shim : MainViewModel
{
    public Shim(IApplicationService appService) : base(appService)
    {
        Source = new();
        Source.Value = new FlatTreeDataGridSource<RegisterEntry>(EntriesView)
        {
            Columns =
            {
                new TextColumn<RegisterEntry, string>("Register", x => x.Register, new GridLength(0, GridUnitType.Auto)),
                new TextColumn<RegisterEntry, string>("Symbol", x => x.Symbol, new GridLength(1, GridUnitType.Star)),
                new TextColumn<RegisterEntry, string>("Document", x => x.DefiningDocument, new GridLength(.5, GridUnitType.Star)),
                new TextColumn<RegisterEntry, string>("UL", x => x.Ul.ToOctets()),
            },
        };
        Source.Value.RowSelection!.SingleSelect = false;

        SelectedRowsCount = new(0);

        SelectedRowsCount = Source.Value.RowSelection
            .ObservePropertyChanged(x => x.Count)
            .ToBindableReactiveProperty();

        MaterialOpacity = new(1);
        TintOpacity = new(1);
    }

    public BindableReactiveProperty<double> MaterialOpacity { get; }
    public BindableReactiveProperty<double> TintOpacity { get; }

    public BindableReactiveProperty<FlatTreeDataGridSource<RegisterEntry>?> Source { get; }
    public BindableReactiveProperty<int> SelectedRowsCount { get; }
}
