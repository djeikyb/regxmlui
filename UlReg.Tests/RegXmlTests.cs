using System.IO.Compression;
using System.Reflection;
using System.Xml.XPath;
using FluentAssertions;
using RegXml;
using Xunit.Abstractions;

namespace UlReg.Tests;

public class RegXmlTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RegXmlTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void WhatIsEvenBeenEmbedden()
    {
        var ass = Assembly.GetAssembly(typeof(Registers));
        Assert.NotNull(ass);
        foreach (var name in ass.GetManifestResourceNames())
        {
            _testOutputHelper.WriteLine(name);
        }
    }

    [Fact]
    public void ZipIsEmbedded()
    {
        Assert.NotNull(Registers.Open());
    }

    [Fact]
    public void WhatsInsideTheBox()
    {
        var s = Registers.Open();
        var z = new ZipArchive(s);
        foreach (var entry in z.Entries)
        {
            _testOutputHelper.WriteLine(entry.FullName);
        }
    }

    [Fact]
    public void RegAllTheIsters()
    {
        var registers = Registers.FromEmbedded();
        Assert.NotNull(registers.Groups);
        var xpath = $"//*[local-name() = 'UL' and text() = 'urn:smpte:ul:060e2b34.027f0101.0d010101.01014e00']/..";
        var el = registers.Groups.Xml.XPathSelectElement(xpath);
        Assert.NotNull(el);
        var symbol = el.Descendants().First(x => "Symbol".Equals(x.Name.LocalName)).Value;
        Assert.Equal("AuxiliaryDescriptor", symbol);
    }

    [Fact]
    public void SymbolAlwaysExists()
    {
        bool alwaysExists = true;
        var reg = Registers.FromEmbedded();
        foreach (var r in reg.All)
            if (!r.EntryPropertyAlwaysExists("Symbol"))
                alwaysExists = false;
        alwaysExists.Should().BeTrue();
    }

    [Fact]
    public void RegisterAlwaysExists()
    {
        bool alwaysExists = true;
        var reg = Registers.FromEmbedded();
        foreach (var r in reg.All)
            if (!r.EntryPropertyAlwaysExists("Register"))
                alwaysExists = false;
        alwaysExists.Should().BeTrue();
    }

    [Fact]
    public void UlAlwaysExists()
    {
        bool alwaysExists = true;
        var reg = Registers.FromEmbedded();
        foreach (var r in reg.All)
            if (!r.EntryPropertyAlwaysExists("UL"))
                alwaysExists = false;
        alwaysExists.Should().BeTrue();
    }

    [Fact]
    public void DefinitionDoesNotAlwaysExist()
    {
        bool alwaysExists = true;
        var reg = Registers.FromEmbedded();
        foreach (var r in reg.All)
            if (!r.EntryPropertyAlwaysExists("Definition"))
                alwaysExists = false;
        alwaysExists.Should().BeFalse();
    }
}
