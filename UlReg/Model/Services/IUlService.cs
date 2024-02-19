using UlReg.Model.Xml;

namespace UlReg.Model.Services;

public interface IRegisterService
{
    Task<RegisterEntry?> Find(Xml.Ul ul);
    Task<RegisterEntry?> FindByLastHalf(Xml.Ul ul);
    Task<bool> EntryPropertyAlwaysExists(string property);
}
