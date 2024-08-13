using UlRegBiz.Model.Xml;

namespace UlReg.ViewModels;

public class RegisterEntryViewModel(RegisterEntry re)
{
    public string? Ul { get; } = re.Ul.ToOctets();
    public string? Register { get; } = re.Register;
    public string? DefiningDocument { get; } = re.DefiningDocument;
    public string? Symbol { get; } = re.Symbol;
}
