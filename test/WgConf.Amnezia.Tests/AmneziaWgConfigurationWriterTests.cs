using System.Text;
using WgConf.Amnezia;
using Xunit;

namespace WgConf.Amnezia.Tests;

public class AmneziaWgConfigurationWriterTests
{
    [Fact]
    public void Write_MinimalConfig_OutputsStandardProperties()
    {
        var config = new AmneziaWgConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
        };

        var output = WriteToString(config);

        Assert.Contains("[Interface]", output);
        Assert.Contains("PrivateKey = YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag=", output);
        Assert.Contains("ListenPort = 51820", output);
        Assert.Contains("Address = 10.0.0.1/24", output);
        Assert.DoesNotContain("Jc", output);
        Assert.DoesNotContain("I1", output);
        Assert.DoesNotContain("H1", output);
    }

    [Fact]
    public void Write_WithIntegerProperties_OutputsCorrectly()
    {
        var config = new AmneziaWgConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
            Jc = 5,
            Jmin = 20,
            Jmax = 1000,
            S1 = 10,
        };

        var output = WriteToString(config);

        Assert.Contains("Jc = 5", output);
        Assert.Contains("Jmin = 20", output);
        Assert.Contains("Jmax = 1000", output);
        Assert.Contains("S1 = 10", output);
    }

    [Fact]
    public void Write_WithStringProperties_OutputsCorrectly()
    {
        var config = new AmneziaWgConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
            I1 = "value1",
            I2 = "value2",
            I3 = "value3",
        };

        var output = WriteToString(config);

        Assert.Contains("I1 = value1", output);
        Assert.Contains("I2 = value2", output);
        Assert.Contains("I3 = value3", output);
    }

    [Fact]
    public void Write_WithRangeProperties_OutputsCorrectly()
    {
        var config = new AmneziaWgConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
            H1 = new IntegerRange { Start = 25, End = 30 },
            H2 = new IntegerRange { Start = 100, End = 200 },
        };

        var output = WriteToString(config);

        Assert.Contains("H1 = 25-30", output);
        Assert.Contains("H2 = 100-200", output);
    }

    [Fact]
    public void Write_CompleteConfig_OutputsAllProperties()
    {
        var config = new AmneziaWgConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
            Jc = 5,
            Jmin = 20,
            Jmax = 1000,
            S1 = 10,
            S2 = 20,
            S3 = 30,
            S4 = 40,
            J1 = 1,
            J2 = 2,
            J3 = 3,
            Itime = 100,
            I1 = "val1",
            I2 = "val2",
            I3 = "val3",
            I4 = "val4",
            I5 = "val5",
            H1 = new IntegerRange { Start = 25, End = 30 },
            H2 = new IntegerRange { Start = 100, End = 200 },
            H3 = new IntegerRange { Start = 5, End = 10 },
            H4 = new IntegerRange { Start = 1000, End = 2000 },
        };

        var output = WriteToString(config);

        Assert.Contains("Jc = 5", output);
        Assert.Contains("Jmin = 20", output);
        Assert.Contains("I1 = val1", output);
        Assert.Contains("H1 = 25-30", output);
    }

    [Fact]
    public void Write_NullOptionalProperties_OmitsFromOutput()
    {
        var config = new AmneziaWgConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
            Jc = 5,
            // Leave Jmin, I1, H1 as null
        };

        var output = WriteToString(config);

        Assert.Contains("Jc = 5", output);
        Assert.DoesNotContain("Jmin", output);
        Assert.DoesNotContain("I1", output);
        Assert.DoesNotContain("H1", output);
    }

    [Fact]
    public void Write_WithPeer_OutputsPeerSection()
    {
        var config = new AmneziaWgConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
            Jc = 5,
        };
        config.Peers.Add(
            new WireguardPeerConfiguration
            {
                PublicKey = Convert.FromBase64String(
                    "xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg="
                ),
                AllowedIPs = [CIDR.Parse("10.0.0.2/32")],
            }
        );

        var output = WriteToString(config);

        Assert.Contains("Jc = 5", output);
        Assert.Contains("[Peer]", output);
        Assert.Contains("PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=", output);
    }

    [Fact]
    public void RoundTrip_WithAmneziaProperties()
    {
        var original = new AmneziaWgConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
            Jc = 5,
            Jmin = 20,
            I1 = "test",
            H1 = new IntegerRange { Start = 25, End = 30 },
        };
        original.Peers.Add(
            new WireguardPeerConfiguration
            {
                PublicKey = Convert.FromBase64String(
                    "xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg="
                ),
                AllowedIPs = [CIDR.Parse("10.0.0.2/32")],
            }
        );

        var text = WriteToString(original);
        using var reader = new AmneziaWgConfigurationReader(new StringReader(text));
        var parsed = reader.Read();

        Assert.Equal(original.ListenPort, parsed.ListenPort);
        Assert.Equal(original.Jc, parsed.Jc);
        Assert.Equal(original.Jmin, parsed.Jmin);
        Assert.Equal(original.I1, parsed.I1);
        Assert.Equal(original.H1!.Value.Start, parsed.H1!.Value.Start);
        Assert.Equal(original.H1!.Value.End, parsed.H1!.Value.End);
        Assert.Single(parsed.Peers);
    }

    [Fact]
    public void PropertyOrdering_WireguardPropertiesBeforeAmnezia()
    {
        var config = new AmneziaWgConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
            Jc = 5,
        };

        var output = WriteToString(config);

        var privateKeyIndex = output.IndexOf("PrivateKey");
        var jcIndex = output.IndexOf("Jc");

        Assert.True(
            privateKeyIndex < jcIndex,
            "WireGuard properties should appear before AmneziaWG properties"
        );
    }

    private static string WriteToString(AmneziaWgConfiguration config)
    {
        using var stringWriter = new StringWriter();
        using (var writer = new AmneziaWgConfigurationWriter(stringWriter))
        {
            writer.Write(config);
        }
        return stringWriter.ToString();
    }
}
