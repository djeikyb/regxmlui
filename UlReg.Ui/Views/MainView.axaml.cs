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
            if (hotkeys is null) return;

            // and also cmd+shift+c, for the black turtleneck crowd
            // ctrl+shift+c accommodates a typical linux keyboard layout
            KeyModifiers m;
            if (OperatingSystem.IsMacOS()) m = KeyModifiers.Meta;
            else m = KeyModifiers.Control;
            var chordmod = e.KeyModifiers;
            var shiftcopy = chordmod.HasFlag(m) && chordmod.HasFlag(KeyModifiers.Shift) && e.Key == Key.C;

            if (!shiftcopy && !hotkeys.Copy.Any(g => g.Matches(e))) return;

            // handle copy

            Console.WriteLine($"⚡️ {nameof(MyTreeDataGrid)} copy!");
            var tdg = (TreeDataGrid)sender!;
            if (tdg.RowSelection is not { SelectedItem: { } row }) return;
            if (row is not RegisterEntry re) return;
            var vm = (MainViewModel?)DataContext;
            if (vm == null) return;

            if (shiftcopy) vm.CopyUlWithPrefixCommand.Execute(re);
            else vm.CopyUlNoPrefixCommand.Execute(re);

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
