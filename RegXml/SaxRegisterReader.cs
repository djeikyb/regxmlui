using System.Xml;
using CommunityToolkit.HighPerformance.Buffers;
using TurboXml;

namespace RegXml;

public enum RegisterName
{
    Elements, Essence, Groups, Labels, Types,
}

public struct SaxEntry
{
    public string? Register { get; set; }
    public Ul? Ul { get; set; }
    public string? Symbol { get; set; }
    public string? DefiningDocument { get; set; }
}

public struct SaxRegisterReader : IXmlReadHandler
{
    public RegisterEntry[] Entries;
    private int i;
    private readonly string _register;
    private int _depth = 0;

    private SaxEntry _current;

    // private ReadOnlySpan<char> _currentElementValue;
    private string _currentElementValue = string.Empty;
    private bool _currentTagIsRegister = false;

    private bool _readTagText = false;

    public SaxRegisterReader(RegisterName register)
    {
        _register = Enum.GetName(register) ?? throw new Exception("Enum name failed.");
        Entries = register switch
        {
            RegisterName.Elements => new RegisterEntry[3729],
            RegisterName.Essence => new RegisterEntry[78],
            RegisterName.Groups => new RegisterEntry[599],
            RegisterName.Labels => new RegisterEntry[3885],
            RegisterName.Types => new RegisterEntry[607],
            _ => throw new RegisterLoadException($"Unknown register: {register}")
        };
    }

    public int Count => i;

    public void OnBeginTag(ReadOnlySpan<char> name, int line, int column)
    {
        switch (name)
        {
            case "Entry":
            {
                _current = new SaxEntry { Register = _register };
                break;
            }
            // The register tag's text is constant within a register.
            // Don't bother reading it.
            case "Symbol":
            case "UL":
            case "DefiningDocument":
                _readTagText = true;
                break;
            default:
                _readTagText = false;
                break;
        }

        _depth++;
    }

    public void OnEndTag(ReadOnlySpan<char> name, int line, int column)
    {
        if (_depth == 4)
        {
            if (name is "UL") _current.Ul = Ul.FromUrn(_currentElementValue);
            if (name is "Symbol") _current.Symbol = _currentElementValue;
            if (name is "DefiningDocument") _current.DefiningDocument = _currentElementValue;
        }
        else if (name is "Entry") Entries[i++] = new RegisterEntry
        {
            // unit tests prove not null, safe to bang
            Register = _current.Register!,
            Symbol = _current.Symbol!,
            Ul = _current.Ul!,
            DefiningDocument = _current.DefiningDocument,
        };

        _depth--;
    }

    public void OnText(ReadOnlySpan<char> text, int line, int column)
    {
        if (!_readTagText) return;
        if (_depth != 4) return;

        _currentElementValue = StringPool.Shared.GetOrAdd(text);
    }

    public void OnXmlDeclaration(ReadOnlySpan<char> version, ReadOnlySpan<char> encoding, ReadOnlySpan<char> standalone,
        int line, int column)
    {
    }

    public void OnEndTagEmpty()
    {
    }

    public void OnAttribute(ReadOnlySpan<char> name, ReadOnlySpan<char> value, int nameLine, int nameColumn,
        int valueLine, int valueColumn)
    {
    }

    public void OnComment(ReadOnlySpan<char> comment, int line, int column)
    {
    }

    public void OnCData(ReadOnlySpan<char> cdata, int line, int column)
    {
    }

    public void OnError(string message, int line, int column)
    {
        throw new XmlException(message, null, line + 1, column + 1);
    }
}
