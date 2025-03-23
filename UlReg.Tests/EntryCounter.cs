using System.Xml;
using TurboXml;

namespace UlReg.Tests;

public struct EntryCounter : IXmlReadHandler
{
    public int Count;

    public void OnBeginTag(ReadOnlySpan<char> name, int line, int column)
    {
        if (name is "Entry") Count++;
    }

    public void OnXmlDeclaration(ReadOnlySpan<char> version, ReadOnlySpan<char> encoding, ReadOnlySpan<char> standalone, int line, int column)
    {
    }

    public void OnEndTagEmpty()
    {
    }

    public void OnEndTag(ReadOnlySpan<char> name, int line, int column)
    {
    }

    public void OnAttribute(ReadOnlySpan<char> name, ReadOnlySpan<char> value, int nameLine, int nameColumn, int valueLine, int valueColumn)
    {
    }

    public void OnText(ReadOnlySpan<char> text, int line, int column)
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
