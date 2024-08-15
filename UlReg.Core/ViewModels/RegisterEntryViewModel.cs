using UlRegBiz.Model.Xml;

namespace UlReg.ViewModels;

public class RegisterEntryViewModel(RegisterEntry re)
{
    public string? Ul { get; } = re.Ul.ToOctets();
    public string Register => re.Register;
    public string? DefiningDocument => re.DefiningDocument;
    public string Symbol => re.Symbol;
}
