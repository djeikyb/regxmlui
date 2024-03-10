using System.Xml.Linq;
using System.Xml.XPath;
using UlRegBiz.Model.Services;
using UlRegBiz.Model.Xml;

namespace UlRegBiz.Services;

public class RegXmlService : IRegisterService
{
    private readonly Lazy<IReadOnlyCollection<RegisterEntry>> _singleton;
    private readonly XDocument _doc;

    public RegXmlService(XDocument doc)
    {
        _doc = doc;
        _singleton = new(() => DeserializeAll().ToList());
    }

    public IEnumerable<RegisterEntry> All() => _singleton.Value;

    private IEnumerable<RegisterEntry> DeserializeAll()
    {
        var xpath = $"//*[local-name() = 'Entry']/*[local-name() = 'UL']/..";
        var found = _doc.XPathSelectElements(xpath);
        return found.Select(
            el =>
            {
                var urn = el.Descendants().First(x => "UL".Equals(x.Name.LocalName)).Value;
                var ul = Ul.FromUrn(urn);
                var symbol = el.Descendants().First(x => "Symbol".Equals(x.Name.LocalName)).Value;
                var register = el.Descendants().First(x => "Register".Equals(x.Name.LocalName)).Value;
                var defDoc = el.Descendants().FirstOrDefault(x => "DefiningDocument".Equals(x.Name.LocalName))?.Value;
                return new RegisterEntry()
                {
                    Register = register, Ul = ul, Symbol = symbol, DefiningDocument = defDoc,
                };
            }
        );
    }

    public bool EntryPropertyAlwaysExists(string property)
    {
        const string xpathTotal = "count(//*[local-name() = 'Entries']/*[local-name() = 'Entry'])";
        var totalElements = (double)_doc.XPathEvaluate(xpathTotal);
        string xpathForProperty =
            $"count(//*[local-name() = 'Entries']/*[local-name() = 'Entry']/*[local-name() = '{property}'])";
        var elementsWithProperty = (double)_doc.XPathEvaluate(xpathForProperty);
        return Convert.ToInt64(totalElements) == Convert.ToInt64(elementsWithProperty);
    }

    public IEnumerable<RegisterEntry> Search(
        string? term,
        string? u0 = null,
        string? u4 = null,
        string? u8 = null,
        string? u12 = null
    )
    {
        var q = _singleton.Value.AsQueryable();
        if (u0 is { Length: > 0 }) q = q.Where(re => OctetStartsWith(0, re.Ul, u0));
        if (u4 is { Length: > 0 }) q = q.Where(re => OctetStartsWith(4, re.Ul, u4));
        if (u8 is { Length: > 0 }) q = q.Where(re => OctetStartsWith(8, re.Ul, u8));
        if (u12 is { Length: > 0 }) q = q.Where(re => OctetStartsWith(12, re.Ul, u12));
        return q.ToList();
    }

    public static bool OctetStartsWith(int octet, Ul ul, string search)
    {
        return Convert.ToHexString(ul.Bytes.Span.Slice(octet, 4))
            .StartsWith(search, StringComparison.InvariantCultureIgnoreCase);
    }
}
