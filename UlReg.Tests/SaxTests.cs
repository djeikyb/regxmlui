using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using RegXml;
using TurboXml;
using Xunit.Abstractions;
using MemoryStream = System.IO.MemoryStream;

namespace UlReg.Tests;

public class SaxTests(ITestOutputHelper Console)
{
    [Fact]
    public void CountEntriesPerRegister()
    {
        Stopwatch sax = new();
        Stopwatch xpath = new();
        using var zip = new ZipArchive(Registers.Open());
        foreach (string r in (string[]) ["Elements", "Essence", "Groups", "Labels", "Types"])
        {
            var found = zip.Entries.FirstOrDefault(x => x.Name.Equals($"{r}.xml"));
            Assert.NotNull(found);
            var entryCounter = new EntryCounter();
            double xcount;
            var ms = new MemoryStream();
            using (var z = found.Open())
            {
                z.CopyTo(ms);

                ms.Position = 0;
                var foo = new EntryCounter();
                XmlParser.Parse(ms, ref foo, new XmlParserOptions(Encoding.UTF8));
                ms.Position = 0;
                XmlParser.Parse(ms, ref foo, new XmlParserOptions(Encoding.UTF8));
                ms.Position = 0;
                XmlParser.Parse(ms, ref foo, new XmlParserOptions(Encoding.UTF8));
                sax.Start();
                ms.Position = 0;
                XmlParser.Parse(ms, ref entryCounter, new XmlParserOptions(Encoding.UTF8));
                sax.Stop();

                const string xpathTotal = "count(//*[local-name() = 'Entries']/*[local-name() = 'Entry'])";
                ms.Position = 0;
                var doc = XDocument.Load(ms);
                _ = (double)doc.XPathEvaluate(xpathTotal);
                ms.Position = 0;
                _ = (double)doc.XPathEvaluate(xpathTotal);
                ms.Position = 0;
                _ = (double)doc.XPathEvaluate(xpathTotal);
                xpath.Start();
                ms.Position = 0;
                xcount = (double)doc.XPathEvaluate(xpathTotal);
                xpath.Stop();
            }

            Console.WriteLine(r.PadLeft(8) + ": " + entryCounter.Count);
            Console.WriteLine(r.PadLeft(8) + ": " + xcount);
        }

        Console.WriteLine($"  sax in: {sax.ElapsedMilliseconds}");
        Console.WriteLine($"xpath in: {xpath.ElapsedMilliseconds}");
    }
}
