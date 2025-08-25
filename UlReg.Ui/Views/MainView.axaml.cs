using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using RegXml;

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

        MyTreeDataGrid.KeyDown += (sender, e) =>
        {
            // only pay attention to platform copy key-bindings
            var hotkeys = TopLevel.GetTopLevel(this)?.PlatformSettings?.HotkeyConfiguration;
            if (hotkeys is null || !hotkeys.Copy.Any(g => g.Matches(e))) return;

            // handle copy
            Console.WriteLine($"⚡️ {nameof(MyTreeDataGrid)} copy!");
            var tdg = (TreeDataGrid)sender!;
            if (tdg.RowSelection is not { SelectedItem: { } row }) return;
            if (row is not RegisterEntry re) return;
            var vm = (MainViewModel?)DataContext;
            if (vm == null) return;
            vm.CopyCommand.Execute(re);
            e.Handled = true;
        };

        MyTreeDataGrid.DoubleTapped += (sender, e) =>
        {
            var tdg = (TreeDataGrid)sender!;
            var found = tdg.GetInputElementsAt(e.GetPosition(tdg));
            var cells = (TreeDataGridCellsPresenter?)found.FirstOrDefault(ie => ie is TreeDataGridCellsPresenter);

            // ignore if it wasn't a row that wasn't tapped
            // iunno. maybe it was a column divider?
            if (cells is null) return;

            Console.WriteLine($"⚡️ double-tapped a row!");
            var vm = (MainViewModel)tdg.DataContext!;
            vm.OpenDefiningDocument.Execute((RegisterEntry)cells.DataContext!);
        };
    }
}
