using System.Collections;
using System.Xml.Linq;
using System.Xml.XPath;
using UlRegBiz.Model.Services;
using UlRegBiz.Model.Xml;

namespace UlRegBiz.Services;

public class RegXmlService : IRegisterService
{
    private readonly XDocument _doc;

    public RegXmlService(XDocument doc)
    {
        _doc = doc;
    }

    public Task<RegisterEntry?> Find(Ul ul)
    {
        var xpath = $"//*[local-name() = 'UL' and text() = '{ul.ToUrn()}']/..";
        var el = _doc.XPathSelectElement(xpath);
        if (el == null) return Task.FromResult<RegisterEntry?>(null);
        var symbol = el.Descendants().First(x => "Symbol".Equals(x.Name.LocalName)).Value;
        var register = el.Descendants().First(x => "Register".Equals(x.Name.LocalName)).Value;
        var defDoc = el.Descendants().FirstOrDefault(x => "DefiningDocument".Equals(x.Name.LocalName))?.Value;
        return Task.FromResult<RegisterEntry?>(
            new RegisterEntry
            {
                Register = register, Ul = ul, Symbol = symbol, DefiningDocument = defDoc,
            }
        );
    }

    public Task<RegisterEntry?> FindByLastHalf(Ul ul)
    {
        var half = ul.ToUrn()[^17..];

        var xpath = $"//*[local-name() = 'UL' and contains(text(), '{half}')]/..";
        var el = _doc.XPathSelectElement(xpath);
        if (el == null) return Task.FromResult<RegisterEntry?>(null);
        var symbol = el.Descendants().First(x => "Symbol".Equals(x.Name.LocalName)).Value;
        var register = el.Descendants().First(x => "Register".Equals(x.Name.LocalName)).Value;
        var defDoc = el.Descendants().FirstOrDefault(x => "DefiningDocument".Equals(x.Name.LocalName))?.Value;
        return Task.FromResult<RegisterEntry?>(
            new RegisterEntry
            {
                Register = register, Ul = ul, Symbol = symbol, DefiningDocument = defDoc,
            }
        );
    }

    public IEnumerable<string> AllUls()
    {
        var found = (IEnumerable)_doc.XPathEvaluate(
            "//*[local-name() = 'Entries']/*[local-name() = 'Entry']/*[local-name() = 'UL']/text()"
        );
        return found.Cast<XText>().Select(x => x.Value);
    }

    public Task<bool> EntryPropertyAlwaysExists(string property)
    {
        const string xpathTotal = "count(//*[local-name() = 'Entries']/*[local-name() = 'Entry'])";
        var totalElements = (double)_doc.XPathEvaluate(xpathTotal);
        string xpathForProperty =
            $"count(//*[local-name() = 'Entries']/*[local-name() = 'Entry']/*[local-name() = '{property}'])";
        var elementsWithProperty = (double)_doc.XPathEvaluate(xpathForProperty);
        return Task.FromResult(Convert.ToInt64(totalElements) == Convert.ToInt64(elementsWithProperty));
    }
}
