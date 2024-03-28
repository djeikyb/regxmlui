using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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
    }
}
