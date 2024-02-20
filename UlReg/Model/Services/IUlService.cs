using UlRegBiz.Model.Xml;

namespace UlRegBiz.Model.Services;

public interface IRegisterService
{
    Task<RegisterEntry?> Find(Ul ul);
    Task<RegisterEntry?> FindByLastHalf(Ul ul);
    Task<bool> EntryPropertyAlwaysExists(string property);
}
