using System.Diagnostics;

namespace UlReg.Model.Xml;

public class Ul
{
    private readonly ReadOnlyMemory<byte> _value;

    public Ul(ReadOnlyMemory<byte> value)
    {
        Debug.Assert(value.Length == 16, "UL are always sixteen bytes.");
        _value = value;
    }

    public string ToUrn()
    {
        var s = _value.Span;
        var p1 = Convert.ToHexString(s.Slice(0, 4));
        var p2 = Convert.ToHexString(s.Slice(4, 4));
        var p3 = Convert.ToHexString(s.Slice(8, 4));
        var p4 = Convert.ToHexString(s.Slice(12, 4));
        return $"urn:smpte:ul:{p1}.{p2}.{p3}.{p4}";
    }
}
