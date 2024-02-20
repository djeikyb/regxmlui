using CommunityToolkit.Mvvm.ComponentModel;

namespace UlReg.ViewModels;

public partial class MainViewModel : ObservableObject
{

    [ObservableProperty]
    private bool _topmost;
}
