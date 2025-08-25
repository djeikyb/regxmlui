using RegXml;

namespace UlReg.Tests;

public class UlTests
{
    [Fact]
    public void UlEquals()
    {
        var a = Ul.FromUrn("urn:smpte:ul:060E2B34.01020101.00000000.00000000");
        var b = Ul.FromUrn("urn:smpte:ul:060e2b34.01020101.00000000.00000000");
        Assert.True(a.Equals(b));
    }
}
