using System.IO.Compression;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RegXml;

public class RegisterLoadException : Exception
{
    public RegisterLoadException(string register) : base($"Missing register: {register}")
    {
    }
}

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

public class Register
{
    public readonly XDocument Xml;

    /// You own the stream and should close it.
    public Register(Stream stream)
    {
        Xml = XDocument.Load(stream);
    }

    public bool EntryPropertyAlwaysExists(string property)
    {
        const string xpathTotal = "count(//*[local-name() = 'Entries']/*[local-name() = 'Entry'])";
        var totalElements = (double)Xml.XPathEvaluate(xpathTotal);
        string xpathForProperty =
            $"count(//*[local-name() = 'Entries']/*[local-name() = 'Entry']/*[local-name() = '{property}'])";
        var elementsWithProperty = (double)Xml.XPathEvaluate(xpathForProperty);
        return Convert.ToInt64(totalElements) == Convert.ToInt64(elementsWithProperty);
    }
}
