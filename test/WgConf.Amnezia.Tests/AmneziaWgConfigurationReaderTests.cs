using WgConf.Amnezia;
using Xunit;

namespace WgConf.Amnezia.Tests;

public class AmneziaWgConfigurationReaderTests
{
    [Fact]
    public void Read_MinimalConfig_ParsesStandardWireguardProperties()
    {
        var config =
            @"[Interface]
PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=
ListenPort = 51820
Address = 10.0.0.1/24

[Peer]
PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
AllowedIPs = 10.0.0.2/32";

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        var result = reader.Read();

        Assert.NotNull(result);
        Assert.Equal(51820, result.ListenPort);
        Assert.Single(result.Peers);
        Assert.Null(result.Jc);
        Assert.Null(result.Jmin);
        Assert.Null(result.I1);
        Assert.Null(result.H1);
    }

    [Fact]
    public void Read_WithIntegerProperties_ParsesCorrectly()
    {
        var config =
            @"[Interface]
PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=
ListenPort = 51820
Address = 10.0.0.1/24
Jc = 5
Jmin = 20
Jmax = 1000
S1 = 10
S2 = 20
S3 = 30
S4 = 40
J1 = 1
J2 = 2
J3 = 3
Itime = 100

[Peer]
PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
AllowedIPs = 10.0.0.2/32";

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        var result = reader.Read();

        Assert.Equal(5, result.Jc);
        Assert.Equal(20, result.Jmin);
        Assert.Equal(1000, result.Jmax);
        Assert.Equal(10, result.S1);
        Assert.Equal(20, result.S2);
        Assert.Equal(30, result.S3);
        Assert.Equal(40, result.S4);
        Assert.Equal(1, result.J1);
        Assert.Equal(2, result.J2);
        Assert.Equal(3, result.J3);
        Assert.Equal(100, result.Itime);
    }

    [Fact]
    public void Read_WithStringProperties_ParsesCorrectly()
    {
        var config =
            @"[Interface]
PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=
ListenPort = 51820
Address = 10.0.0.1/24
I1 = value1
I2 = value2
I3 = value3
I4 = value4
I5 = value5

[Peer]
PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
AllowedIPs = 10.0.0.2/32";

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        var result = reader.Read();

        Assert.Equal("value1", result.I1);
        Assert.Equal("value2", result.I2);
        Assert.Equal("value3", result.I3);
        Assert.Equal("value4", result.I4);
        Assert.Equal("value5", result.I5);
    }

    [Fact]
    public void Read_WithRangeProperties_ParsesCorrectly()
    {
        var config =
            @"[Interface]
PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=
ListenPort = 51820
Address = 10.0.0.1/24
H1 = 25-30
H2 = 100-200
H3 = 5-10
H4 = 1000-2000

[Peer]
PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
AllowedIPs = 10.0.0.2/32";

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        var result = reader.Read();

        Assert.NotNull(result.H1);
        Assert.Equal(25, result.H1.Value.Start);
        Assert.Equal(30, result.H1.Value.End);

        Assert.NotNull(result.H2);
        Assert.Equal(100, result.H2.Value.Start);
        Assert.Equal(200, result.H2.Value.End);

        Assert.NotNull(result.H3);
        Assert.Equal(5, result.H3.Value.Start);
        Assert.Equal(10, result.H3.Value.End);

        Assert.NotNull(result.H4);
        Assert.Equal(1000, result.H4.Value.Start);
        Assert.Equal(2000, result.H4.Value.End);
    }

    [Fact]
    public void Read_WithSingleValueRangeProperties_ParsesCorrectly()
    {
        var config =
            @"[Interface]
PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=
ListenPort = 51820
Address = 10.0.0.1/24
H1 = 25
H2 = 100
H3 = 5
H4 = 1000

[Peer]
PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
AllowedIPs = 10.0.0.2/32";

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        var result = reader.Read();

        Assert.NotNull(result.H1);
        Assert.Equal(25, result.H1.Value.Start);
        Assert.Null(result.H1.Value.End);

        Assert.NotNull(result.H2);
        Assert.Equal(100, result.H2.Value.Start);
        Assert.Null(result.H2.Value.End);

        Assert.NotNull(result.H3);
        Assert.Equal(5, result.H3.Value.Start);
        Assert.Null(result.H3.Value.End);

        Assert.NotNull(result.H4);
        Assert.Equal(1000, result.H4.Value.Start);
        Assert.Null(result.H4.Value.End);
    }

    [Fact]
    public void Read_WithMixedRangeAndSingleValues_ParsesCorrectly()
    {
        var config =
            @"[Interface]
PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=
ListenPort = 51820
Address = 10.0.0.1/24
H1 = 25
H2 = 100-200
H3 = 5
H4 = 1000-2000

[Peer]
PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
AllowedIPs = 10.0.0.2/32";

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        var result = reader.Read();

        Assert.NotNull(result.H1);
        Assert.Equal(25, result.H1.Value.Start);
        Assert.Null(result.H1.Value.End);

        Assert.NotNull(result.H2);
        Assert.Equal(100, result.H2.Value.Start);
        Assert.Equal(200, result.H2.Value.End);

        Assert.NotNull(result.H3);
        Assert.Equal(5, result.H3.Value.Start);
        Assert.Null(result.H3.Value.End);

        Assert.NotNull(result.H4);
        Assert.Equal(1000, result.H4.Value.Start);
        Assert.Equal(2000, result.H4.Value.End);
    }

    [Fact]
    public void Read_CompleteConfig_ParsesAllProperties()
    {
        var config =
            @"[Interface]
PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=
ListenPort = 51820
Address = 10.0.0.1/24
Jc = 5
Jmin = 20
Jmax = 1000
S1 = 10
S2 = 20
S3 = 30
S4 = 40
J1 = 1
J2 = 2
J3 = 3
Itime = 100
I1 = val1
I2 = val2
I3 = val3
I4 = val4
I5 = val5
H1 = 25-30
H2 = 100-200
H3 = 5-10
H4 = 1000-2000

[Peer]
PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
AllowedIPs = 10.0.0.2/32";

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        var result = reader.Read();

        Assert.Equal(5, result.Jc);
        Assert.Equal(20, result.Jmin);
        Assert.Equal("val1", result.I1);
        Assert.NotNull(result.H1);
        Assert.Equal(25, result.H1.Value.Start);
    }

    [Fact]
    public void Read_CaseInsensitiveProperties_ParsesCorrectly()
    {
        var config =
            @"[Interface]
PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=
ListenPort = 51820
Address = 10.0.0.1/24
jc = 5
JMIN = 20
JMax = 1000
i1 = test
H1 = 10-20

[Peer]
PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
AllowedIPs = 10.0.0.2/32";

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        var result = reader.Read();

        Assert.Equal(5, result.Jc);
        Assert.Equal(20, result.Jmin);
        Assert.Equal(1000, result.Jmax);
        Assert.Equal("test", result.I1);
        Assert.NotNull(result.H1);
    }

    [Fact]
    public void TryRead_InvalidIntegerValue_ReturnsErrors()
    {
        var config =
            @"[Interface]
PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=
ListenPort = 51820
Address = 10.0.0.1/24
Jc = invalid

[Peer]
PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
AllowedIPs = 10.0.0.2/32";

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out var result, out var errors);

        Assert.False(success);
        Assert.Null(result);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Message.Contains("Jc"));
    }

    [Fact]
    public void TryRead_InvalidRangeFormat_ReturnsErrors()
    {
        var config =
            @"[Interface]
PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=
ListenPort = 51820
Address = 10.0.0.1/24
H1 = invalid

[Peer]
PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
AllowedIPs = 10.0.0.2/32";

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out var result, out var errors);

        Assert.False(success);
        Assert.Null(result);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Message.Contains("H1"));
    }

    [Fact]
    public void Read_UnknownAmneziaProperty_ThrowsException()
    {
        var config =
            @"[Interface]
PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=
ListenPort = 51820
Address = 10.0.0.1/24
UnknownProp = value

[Peer]
PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
AllowedIPs = 10.0.0.2/32";

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        Assert.Throws<WireguardConfigurationException>(() => reader.Read());
    }

    [Fact]
    public async Task ReadAsync_WithAmneziaProperties_ParsesCorrectly()
    {
        var config =
            @"[Interface]
PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=
ListenPort = 51820
Address = 10.0.0.1/24
Jc = 5

[Peer]
PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
AllowedIPs = 10.0.0.2/32";

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        var result = await reader.ReadAsync();

        Assert.Equal(5, result.Jc);
    }

    [Fact]
    public async Task TryReadAsync_WithErrors_ReturnsErrors()
    {
        var config =
            @"[Interface]
PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=
ListenPort = 51820
Address = 10.0.0.1/24
Jmin = abc

[Peer]
PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
AllowedIPs = 10.0.0.2/32";

        using var reader = new AmneziaWgConfigurationReader(new StringReader(config));
        var (result, errors) = await reader.TryReadAsync();

        Assert.Null(result);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Message.Contains("Jmin"));
    }
}
