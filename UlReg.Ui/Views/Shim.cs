using R3;

namespace UlReg.Views;

public class Shim : MainViewModel
{
    public Shim(IApplicationService appService) : base(appService)
    {
        MaterialOpacity = new(1);
        TintOpacity = new(1);
    }

    public BindableReactiveProperty<double> MaterialOpacity { get; }
    public BindableReactiveProperty<double> TintOpacity { get; }
}
