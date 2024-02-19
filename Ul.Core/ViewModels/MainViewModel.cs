using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Ul.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        ChangeThemeCommand = new RelayCommand(ChangeThemeAction);
    }
}
