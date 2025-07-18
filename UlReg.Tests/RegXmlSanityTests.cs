using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using RegXml;
using TurboXml;

namespace UlReg.Tests;

public class RegXmlSanityTests
{
    private static readonly IReadOnlyList<RegisterEntry> _entries;

    static RegXmlSanityTests()
    {
        var list = new List<RegisterEntry>();

        using var zip = new ZipArchive(Registers.Open());
        foreach (string r in (string[])["Elements", "Essence", "Groups", "Labels", "Types"])
        {
            var found = zip.Entries.FirstOrDefault(x => x.Name.Equals($"{r}.xml"));
            Assert.NotNull(found);
            var regReader = new SaxRegisterReader(r switch
            {
                "Elements" => RegisterName.Elements,
                "Essence" => RegisterName.Essence,
                "Groups" => RegisterName.Groups,
                "Labels" => RegisterName.Labels,
                "Types" => RegisterName.Types,
                _ => throw new ArgumentOutOfRangeException()
            });
            var ms = new MemoryStream();
            using (var z = found.Open())
            {
                z.CopyTo(ms);

                ms.Position = 0;
                XmlParser.Parse(ms, ref regReader, new XmlParserOptions(Encoding.UTF8));
                list.AddRange(regReader.Entries);
            }
        }

        _entries = list;
    }

    [Fact]
    public void RegAllTheIsters()
    {
        var found = _entries.SingleOrDefault(x => Ul.FromUrn("urn:smpte:ul:060e2b34.027f0101.0d010101.01014e00").Equals(x.Ul));
        Assert.NotEqual(default, found);
        Assert.Equal("AuxiliaryDescriptor", found.Symbol);
    }

    [Fact]
    public void GuaranteeSomeEntryPropertiesExist()
    {
        using var zip = new ZipArchive(Registers.Open());
        foreach (string r in (string[])["Elements", "Essence", "Groups", "Labels", "Types"])
        {
            var found = zip.Entries.FirstOrDefault(x => x.Name.Equals($"{r}.xml"));
            Assert.NotNull(found);
            var ms = new MemoryStream();
            using (var z = found.Open())
            {
                z.CopyTo(ms);

                ms.Position = 0;
                var doc = XDocument.Load(ms);

                Assert.True(EntryPropertyAlwaysExists(doc, "Symbol"));
                Assert.True(EntryPropertyAlwaysExists(doc, "Register"));
                Assert.True(EntryPropertyAlwaysExists(doc, "UL"));
            }
        }
    }

    private static bool EntryPropertyAlwaysExists(XDocument doc, string property)
    {
        const string xpathTotal = "count(//*[local-name() = 'Entries']/*[local-name() = 'Entry'])";
        var totalElements = (double)doc.XPathEvaluate(xpathTotal);
        string xpathForProperty =
            $"count(//*[local-name() = 'Entries']/*[local-name() = 'Entry']/*[local-name() = '{property}'])";
        var elementsWithProperty = (double)doc.XPathEvaluate(xpathForProperty);
        return Convert.ToInt64(totalElements) == Convert.ToInt64(elementsWithProperty);
    }

    [Fact]
    public void DefinitionDoesNotAlwaysExist()
    {
        var e = _entries.FirstOrDefault(x => x.DefiningDocument is null);
        Assert.NotEqual(default, e);
    }
}
