using System.IO.Compression;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using TurboXml;

namespace RegXml;

public class Registers
{
    public const string EmbeddedRegxmlZipName = "RegXml.regxml.zip";

    public static Stream Open()
    {
        var zip = Assembly.GetExecutingAssembly().GetManifestResourceStream(EmbeddedRegxmlZipName);
        if (zip == null) throw new Exception("Smpte metadata registers were not found embedded.");
        return zip;
    }

    public static Registers FromXPath()
    {
        Register? essence = null;
        Register? types = null;
        Register? labels = null;
        Register? groups = null;
        Register? elements = null;
        using var z = new ZipArchive(Open());

        foreach (var entry in z.Entries)
        {
            switch (entry.Name)
            {
                case "Essence.xml":
                    using (var s = entry.Open())
                    {
                        var doc = XDocument.Load(s);
                        essence = new Register(DeserializeAll(doc).ToList());
                    }

                    break;
                case "Types.xml":
                    using (var s = entry.Open())
                    {
                        var doc = XDocument.Load(s);
                        types = new Register(DeserializeAll(doc).ToList());
                    }

                    break;
                case "Labels.xml":
                    using (var s = entry.Open())
                    {
                        var doc = XDocument.Load(s);
                        labels = new Register(DeserializeAll(doc).ToList());
                    }

                    break;
                case "Groups.xml":
                    using (var s = entry.Open())
                    {
                        var doc = XDocument.Load(s);
                        groups = new Register(DeserializeAll(doc).ToList());
                    }

                    break;
                case "Elements.xml":
                    using (var s = entry.Open())
                    {
                        var doc = XDocument.Load(s);
                        elements = new Register(DeserializeAll(doc).ToList());
                    }

                    break;
            }
        }

        if (essence == null) throw new RegisterLoadException("essence");
        if (types == null) throw new RegisterLoadException("types");
        if (labels == null) throw new RegisterLoadException("labels");
        if (groups == null) throw new RegisterLoadException("groups");
        if (elements == null) throw new RegisterLoadException("elements");

        return new Registers
        {
            Essence = essence,
            Types = types,
            Labels = labels,
            Groups = groups,
            Elements = elements
        };
    }

    public static Registers FromSax()
    {
        RegisterEntry[]? essence = null;
        RegisterEntry[]? types = null;
        RegisterEntry[]? labels = null;
        RegisterEntry[]? groups = null;
        RegisterEntry[]? elements = null;
        using var z = new ZipArchive(Open());

        foreach (var entry in z.Entries)
        {
            switch (entry.Name)
            {
                case "Essence.xml":
                {
                    var sax = new SaxRegisterReader(RegisterName.Essence);
                    using var entryStream = entry.Open();
                    XmlParser.Parse(entryStream, ref sax, new XmlParserOptions(Encoding.UTF8));
                    essence = sax.Entries;
                    break;
                }
                case "Types.xml":
                {
                    var sax = new SaxRegisterReader(RegisterName.Types);
                    using var entryStream = entry.Open();
                    XmlParser.Parse(entryStream, ref sax, new XmlParserOptions(Encoding.UTF8));
                    types = sax.Entries;
                    break;
                }
                case "Labels.xml":
                {
                    var sax = new SaxRegisterReader(RegisterName.Labels);
                    using var entryStream = entry.Open();
                    XmlParser.Parse(entryStream, ref sax, new XmlParserOptions(Encoding.UTF8));
                    labels = sax.Entries;
                    break;
                }
                case "Groups.xml":
                {
                    var sax = new SaxRegisterReader(RegisterName.Groups);
                    using var entryStream = entry.Open();
                    XmlParser.Parse(entryStream, ref sax, new XmlParserOptions(Encoding.UTF8));
                    groups = sax.Entries;
                    break;
                }
                case "Elements.xml":
                {
                    var sax = new SaxRegisterReader(RegisterName.Elements);
                    using var entryStream = entry.Open();
                    XmlParser.Parse(entryStream, ref sax, new XmlParserOptions(Encoding.UTF8));
                    elements = sax.Entries;
                    break;
                }
            }
        }

        if (essence == null) throw new RegisterLoadException("essence");
        if (types == null) throw new RegisterLoadException("types");
        if (labels == null) throw new RegisterLoadException("labels");
        if (groups == null) throw new RegisterLoadException("groups");
        if (elements == null) throw new RegisterLoadException("elements");

        return new Registers
        {
            Essence = new Register(essence),
            Types = new Register(types),
            Labels = new Register(labels),
            Groups = new Register(groups),
            Elements = new Register(elements)
        };
    }

    public required Register Essence { get; init; }
    public required Register Types { get; init; }
    public required Register Labels { get; init; }
    public required Register Groups { get; init; }
    public required Register Elements { get; init; }

    public IEnumerable<Register> All => [Elements, Essence, Groups, Labels, Types];

    private static IEnumerable<RegisterEntry> DeserializeAll(XDocument doc)
    {
        var xpath = $"//*[local-name() = 'Entry']/*[local-name() = 'UL']/..";
        var found = doc.XPathSelectElements(xpath);
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
}

public class Register : IRegister
{
    private readonly IReadOnlyCollection<RegisterEntry> _entries;

    /// You own the stream and should close it.
    public Register(IReadOnlyCollection<RegisterEntry> entries)
    {
        _entries = entries;
    }

    public Register(IEnumerable<SaxEntry> saxEntries) : this(saxEntries.Select(x =>
        new RegisterEntry
        {
            // unit tests prove not null, safe to bang
            Register = x.Register!,
            Symbol = x.Symbol!,
            Ul = x.Ul!,
            DefiningDocument = x.DefiningDocument,
        }).ToArray())
    {
    }

    public IReadOnlyCollection<RegisterEntry> All() => _entries;

    public IEnumerable<RegisterEntry> Search(
        string? term,
        string? u0 = null,
        string? u4 = null,
        string? u8 = null,
        string? u12 = null
    )
    {
        var q = _entries.AsQueryable();
        if (u0 is { Length: > 0 }) q = q.Where(re => re.Ul._s_oct0.StartsWith(u0));
        if (u4 is { Length: > 0 }) q = q.Where(re => re.Ul._s_oct4.StartsWith(u4));
        if (u8 is { Length: > 0 }) q = q.Where(re => re.Ul._s_oct8.StartsWith(u8));
        if (u12 is { Length: > 0 }) q = q.Where(re => re.Ul._s_oct12.StartsWith(u12));
        if (term is { Length: > 2 })
        {
            var t = Regex.Replace(term, "-|:| ", ".");
            Expression<Func<RegisterEntry, bool>> chain = re =>
                re.Symbol.StartsWith(term, StringComparison.InvariantCultureIgnoreCase);
            if (term is { Length: <= 8 })
            {
                Expression<Func<RegisterEntry, bool>> p0 = re => re.Ul._s_oct0.StartsWith(t);
                Expression<Func<RegisterEntry, bool>> p4 = re => re.Ul._s_oct4.StartsWith(t);
                Expression<Func<RegisterEntry, bool>> p8 = re => re.Ul._s_oct8.StartsWith(t);
                Expression<Func<RegisterEntry, bool>> p12 = re => re.Ul._s_oct12.StartsWith(t);
                chain = chain.Or(p0).Or(p4).Or(p8).Or(p12);
            }

            if (term is { Length: > 4 and <= 35 })
            {
                chain = chain.Or(re => re.Ul.ToOctets().Contains(t, StringComparison.InvariantCultureIgnoreCase));
            }

            if (term is { Length: >= 3 })
            {
                chain = chain.Or(re => re.Symbol.Contains(term, StringComparison.InvariantCultureIgnoreCase));
            }

            if (term is { Length: >= 3 })
            {
                chain = chain.Or(re => re.DefiningDocument != null && re.DefiningDocument.Contains(term, StringComparison.InvariantCultureIgnoreCase));
            }

            q = q.Where(chain);
        }


        return q.ToList();
    }
}
