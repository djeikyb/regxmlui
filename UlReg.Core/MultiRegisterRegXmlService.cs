using RegXml;

namespace UlReg;

public class MultiRegisterRegXmlService : IRegister
{
    private readonly List<IRegister> _services;

    public MultiRegisterRegXmlService(params IRegister[] services)
    {
        _services = services.ToList();
    }

    public IEnumerable<RegisterEntry> All()
    {
        var all = new List<RegisterEntry>();
        foreach (var service in _services)
        {
            all.AddRange(service.All());
        }

        return all;
    }

    public IEnumerable<RegisterEntry> Search(string? term, string? u0 = null, string? u4 = null, string? u8 = null, string? u12 = null)
    {
        var found = new List<RegisterEntry>();
        foreach (var service in _services)
        {
            found.AddRange(service.Search(term, u0, u4, u8, u12));
        }

        return found;
    }
}
