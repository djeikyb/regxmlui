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
                Console.WriteLine($"⚡️ double-clicked a doc (column index 2)!");

                var vm = (MainViewModel?)DataContext;
                if (vm == null) return;

                var tb = (TextBlock?)e.Cell.Content;
                var maybeDoc = tb?.Text;
                if (maybeDoc is null) return;

                vm.OpenDefiningDocument.Execute(maybeDoc);
            });
    }

}
