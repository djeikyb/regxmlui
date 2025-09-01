using System.IO.Compression;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

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

    public static Registers FromEmbedded()
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
                    essence = new Register(entry.Open());
                    break;
                case "Types.xml":
                    types = new Register(entry.Open());
                    break;
                case "Labels.xml":
                    labels = new Register(entry.Open());
                    break;
                case "Groups.xml":
                    groups = new Register(entry.Open());
                    break;
                case "Elements.xml":
                    elements = new Register(entry.Open());
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

    public required Register Essence { get; init; }
    public required Register Types { get; init; }
    public required Register Labels { get; init; }
    public required Register Groups { get; init; }
    public required Register Elements { get; init; }

    public IEnumerable<Register> All => [Elements, Essence, Groups, Labels, Types];
}

public class Register : IRegister
{
    private readonly IReadOnlyCollection<RegisterEntry> _entries;
    internal readonly XDocument _doc;

    /// You own the stream and should close it.
    public Register(Stream stream)
    {
        _doc = XDocument.Load(stream);
        _entries = DeserializeAll(_doc).ToList();
    }

    public IEnumerable<RegisterEntry> All() => _entries;

    public IEnumerable<RegisterEntry> Search(
        string? term,
        string? u0 = null,
        string? u4 = null,
        string? u8 = null,
        string? u12 = null
    )
    {
        var q = _entries.AsQueryable();
        if (u0 is { Length: > 0 }) q = q.Where(re => OctetStartsWith(0, re.Ul, u0));
        if (u4 is { Length: > 0 }) q = q.Where(re => OctetStartsWith(4, re.Ul, u4));
        if (u8 is { Length: > 0 }) q = q.Where(re => OctetStartsWith(8, re.Ul, u8));
        if (u12 is { Length: > 0 }) q = q.Where(re => OctetStartsWith(12, re.Ul, u12));
        if (term is { Length: > 2 })
        {
            var t = Regex.Replace(term, "-|:| ", ".");
            Expression<Func<RegisterEntry, bool>> chain = re =>
                re.Symbol.StartsWith(term, StringComparison.InvariantCultureIgnoreCase);
            if (term is { Length: <= 8 })
            {
                Expression<Func<RegisterEntry, bool>> p0 = re => OctetStartsWith(0, re.Ul, t);
                Expression<Func<RegisterEntry, bool>> p4 = re => OctetStartsWith(4, re.Ul, t);
                Expression<Func<RegisterEntry, bool>> p8 = re => OctetStartsWith(8, re.Ul, t);
                Expression<Func<RegisterEntry, bool>> p12 = re => OctetStartsWith(12, re.Ul, t);
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

    public IEnumerable<RegisterEntry> Search2(
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

    public static bool OctetStartsWith(int octet, Ul ul, string search)
    {
        return Convert.ToHexString(ul.Bytes.Span.Slice(octet, 4))
            .StartsWith(search, StringComparison.InvariantCultureIgnoreCase);
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

    private IEnumerable<RegisterEntry> DeserializeAll(XDocument doc)
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
