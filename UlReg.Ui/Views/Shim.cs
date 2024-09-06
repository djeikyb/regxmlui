using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using R3;
using UlReg.Model.Services;
using UlReg.ViewModels;

namespace UlReg.Views;

public class Shim : MainViewModel
{
    public Shim(IApplicationService appService) : base(appService)
    {
        Source = new();
        Source.Value = new FlatTreeDataGridSource<RegisterEntryViewModel>(EntriesView)
        {
            Columns =
            {
                new TextColumn<RegisterEntryViewModel, string>("Register", x => x.Register, new GridLength(0, GridUnitType.Auto)),
                new TextColumn<RegisterEntryViewModel, string>("Symbol", x => x.Symbol, new GridLength(1, GridUnitType.Star)),
                new TextColumn<RegisterEntryViewModel, string>("Document", x => x.DefiningDocument, new GridLength(.5, GridUnitType.Star)),
                new TextColumn<RegisterEntryViewModel, string>("UL", x => x.Ul),
            },
        };
        Source.Value.RowSelection!.SingleSelect = false;

        SelectedRowsCount = new(0);

        SelectedRowsCount = Source.Value.RowSelection
            .ObservePropertyChanged(x => x.Count)
            .ToBindableReactiveProperty();
    }


    public BindableReactiveProperty<FlatTreeDataGridSource<RegisterEntryViewModel>?> Source { get; }
    public BindableReactiveProperty<int> SelectedRowsCount { get; }
}
