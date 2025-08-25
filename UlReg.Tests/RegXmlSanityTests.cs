using System.IO.Compression;
using System.Text;
using RegXml;
using TurboXml;

namespace UlReg.Tests;

public class RegXmlSanityTests
{
    private static readonly IReadOnlyList<SaxEntry> _entries;

    static RegXmlSanityTests()
    {
        var list = new List<SaxEntry>();

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

    // Would it actually be better to use xpath for this?
    //
    // public bool EntryPropertyAlwaysExists(string property)
    // {
    //     const string xpathTotal = "count(//*[local-name() = 'Entries']/*[local-name() = 'Entry'])";
    //     var totalElements = (double)_doc.XPathEvaluate(xpathTotal);
    //     string xpathForProperty =
    //         $"count(//*[local-name() = 'Entries']/*[local-name() = 'Entry']/*[local-name() = '{property}'])";
    //     var elementsWithProperty = (double)_doc.XPathEvaluate(xpathForProperty);
    //     return Convert.ToInt64(totalElements) == Convert.ToInt64(elementsWithProperty);
    // }

    [Fact]
    public void RegAllTheIsters()
    {
        var found = _entries.SingleOrDefault(x => Ul.FromUrn("urn:smpte:ul:060e2b34.027f0101.0d010101.01014e00").Equals(x.Ul));
        Assert.NotEqual(default, found);
        Assert.Equal("AuxiliaryDescriptor", found.Symbol);
    }

    [Fact]
    public void SymbolAlwaysExists()
    {
        var e = _entries.FirstOrDefault(x => x.Symbol is null);
        Assert.Equal(default, e);
    }

    [Fact]
    public void RegisterAlwaysExists()
    {
        var e = _entries.FirstOrDefault(x => x.Register is null);
        Assert.Equal(default, e);
    }

    [Fact]
    public void UlAlwaysExists()
    {
        var e = _entries.FirstOrDefault(x => x.Ul is null);
        Assert.Equal(default, e);
    }

    [Fact]
    public void DefinitionDoesNotAlwaysExist()
    {
        var e = _entries.FirstOrDefault(x => x.DefiningDocument is null);
        Assert.NotEqual(default, e);
    }
}
