using System.Diagnostics;

namespace UlRegBiz.Model.Xml;

public class Ul
{
    private readonly ReadOnlyMemory<byte> _value;

    public Ul(ReadOnlyMemory<byte> value)
    {
        Debug.Assert(value.Length == 16, "UL are always sixteen bytes.");
        _value = value;
    }

    public ReadOnlyMemory<byte> Bytes => _value;

    public static Ul FromUrn(string urn)
    {
        const int expectedLength = 48;
        if (urn.Length != expectedLength)
            throw new UlException(
                $"Invalid ul. Wrong number of chars. Urn must start with \"urn:smpte:ul:\". Got {urn.Length}, expected {expectedLength}."
            );

        // 0..13 should be "urn:smpte:ul:", but is it worth checking?
        urn = urn.Substring(13);

        var octets = new byte[16];
        for (int idxUrn = 0, idxOctets = 0; idxUrn < urn.Length;)
        {
            if (idxOctets > 0 && idxOctets % 4 == 0) idxUrn += 1;

            // I chose to be delimiter agnostic, and instead allow any
            // single char delimiter every eight chars.
            //
            // Could have instead written:
            //
            // if (urn[i] == '.') i += 1;

            var hi = C2B(urn[idxUrn++]);
            var lo = C2B(urn[idxUrn++]);

            // If you're wondering how tf this works, try running these
            // in the debugger:
            //
            // Convert.ToString(hi, toBase: 2).PadLeft(8, '0')
            // Convert.ToString(hi<<4, toBase: 2).PadLeft(8, '0')
            // Convert.ToString(lo, toBase: 2).PadLeft(8, '0')

            byte b = (byte)(hi << 4 | lo);
            octets[idxOctets++] = b;
        }

        return new Ul(octets);
    }

    internal static byte C2B(char c) => c switch
    {
        '0' => 0, '1' => 1, '2' => 2, '3' => 3,
        '4' => 4, '5' => 5, '6' => 6, '7' => 7,
        '8' => 8, '9' => 9, 'a' => 10, 'A' => 10,
        'b' => 11, 'B' => 11,
        'c' => 12, 'C' => 12,
        'd' => 13, 'D' => 13,
        'e' => 14, 'E' => 14,
        'f' => 15, 'F' => 15,
        _ => throw new ArgumentOutOfRangeException(nameof(c), c, "Not a hexadecimal character.")
    };


    public string ToUrn()
    {
        var s = _value.Span;
        var p1 = Convert.ToHexString(s.Slice(0, 4));
        var p2 = Convert.ToHexString(s.Slice(4, 4));
        var p3 = Convert.ToHexString(s.Slice(8, 4));
        var p4 = Convert.ToHexString(s.Slice(12, 4));
        return $"urn:smpte:ul:{p1}.{p2}.{p3}.{p4}";
    }

    public string ToOctets()
    {
        var s = _value.Span;
        var p1 = Convert.ToHexString(s.Slice(0, 4));
        var p2 = Convert.ToHexString(s.Slice(4, 4));
        var p3 = Convert.ToHexString(s.Slice(8, 4));
        var p4 = Convert.ToHexString(s.Slice(12, 4));
        return $"{p1}.{p2}.{p3}.{p4}";
    }
}

public class UlException : Exception
{
    public UlException(string msg) : base(msg)
    {
    }
}
