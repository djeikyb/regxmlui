using CommunityToolkit.HighPerformance.Buffers;

namespace RegXml;

public class Ul
{
    private readonly ReadOnlyMemory<byte> _value;
    private readonly string _octets;
    public readonly string _s_oct0;
    public readonly string _s_oct4;
    public readonly string _s_oct8;
    public readonly string _s_oct12;

    public Ul(ReadOnlySpan<char> urn)
    {
        const int expectedLength = 48;
        if (urn.Length != expectedLength)
            throw new UlException(
                $"Invalid ul. Wrong number of chars. Urn must start with \"urn:smpte:ul:\". Got {urn.Length}, expected {expectedLength}."
            );

        // 0..13 should be "urn:smpte:ul:", but is it worth checking?
        urn = urn.Slice(13);

        _s_oct0 = StringPool.Shared.GetOrAdd(urn.Slice(0, 8));
        _s_oct4 = StringPool.Shared.GetOrAdd(urn.Slice(9, 8)); // urn[8] is separator
        _s_oct8 = StringPool.Shared.GetOrAdd(urn.Slice(18, 8)); // urn[18] is separator
        _s_oct12 = StringPool.Shared.GetOrAdd(urn.Slice(27, 8)); // 35 + 13 == 48, expected length!

        var s_octets = new string(urn.Slice(0, 35));

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

        _octets = s_octets;
        _value = octets;
    }

    public ReadOnlyMemory<byte> Bytes => _value;

    public static Ul FromUrn(ReadOnlySpan<char> urn)
    {
        return new Ul(urn);
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
        return _octets;
        // if (_octets is not null) return _octets;
        // var s = _value.Span;
        // var p1 = Convert.ToHexString(s.Slice(0, 4));
        // var p2 = Convert.ToHexString(s.Slice(4, 4));
        // var p3 = Convert.ToHexString(s.Slice(8, 4));
        // var p4 = Convert.ToHexString(s.Slice(12, 4));
        // _octets = $"{p1}.{p2}.{p3}.{p4}";
        // return _octets;
    }

    public bool Equals(Ul? other) => other != null && _value.Span.SequenceEqual(other._value.Span);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Ul)obj);
    }

    public override int GetHashCode() => _value.GetHashCode();
}

public class UlException : Exception
{
    public UlException(string msg) : base(msg)
    {
    }
}
