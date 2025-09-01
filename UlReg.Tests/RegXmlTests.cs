using System.IO.Compression;
using System.Reflection;
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
}
