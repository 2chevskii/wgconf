using WgConf.Amnezia;

namespace WgConf.Amnezia.Tests;

public class AmneziaWgConfigurationReaderInvalidPropertiesTests
{
    [Fact]
    public void TryRead_InvalidExtendedProperties_ReturnsErrors()
    {
        var config = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            Jmax = bad
            S1 = bad
            S2 = bad
            S3 = bad
            S4 = bad
            J1 = bad
            J2 = bad
            J3 = bad
            Itime = bad
            H2 = bad
            H3 = bad
            H4 = bad

            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = 10.0.0.2/32
            """;

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out var result, out var errors);

        Assert.False(success);
        Assert.Null(result);
        Assert.Contains(errors, e => e.Message.Contains("Jmax"));
        Assert.Contains(errors, e => e.Message.Contains("S1"));
        Assert.Contains(errors, e => e.Message.Contains("S2"));
        Assert.Contains(errors, e => e.Message.Contains("S3"));
        Assert.Contains(errors, e => e.Message.Contains("S4"));
        Assert.Contains(errors, e => e.Message.Contains("J1"));
        Assert.Contains(errors, e => e.Message.Contains("J2"));
        Assert.Contains(errors, e => e.Message.Contains("J3"));
        Assert.Contains(errors, e => e.Message.Contains("Itime"));
        Assert.Contains(errors, e => e.Message.Contains("H2"));
        Assert.Contains(errors, e => e.Message.Contains("H3"));
        Assert.Contains(errors, e => e.Message.Contains("H4"));
    }
}
