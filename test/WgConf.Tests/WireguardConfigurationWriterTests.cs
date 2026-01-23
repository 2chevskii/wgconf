using System.Net;

namespace WgConf.Tests;

public class WireguardConfigurationWriterTests
{
    [Fact]
    public void Write_MinimalInterfaceConfiguration_WritesCorrectFormat()
    {
        var config = new WireguardConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
        };

        var output = WriteConfiguration(config);

        var expected = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24


            """;

        Assert.Equal(expected, output);
    }

    [Fact]
    public void Write_InterfaceWithAllOptionalProperties_WritesAllProperties()
    {
        var config = new WireguardConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
            PreUp = "echo 'Starting WireGuard'",
            PostUp = "iptables -A FORWARD -i wg0 -j ACCEPT",
            PreDown = "echo 'Stopping WireGuard'",
            PostDown = "iptables -D FORWARD -i wg0 -j ACCEPT",
        };

        var output = WriteConfiguration(config);

        var expected = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            PreUp = echo 'Starting WireGuard'
            PostUp = iptables -A FORWARD -i wg0 -j ACCEPT
            PreDown = echo 'Stopping WireGuard'
            PostDown = iptables -D FORWARD -i wg0 -j ACCEPT


            """;

        Assert.Equal(expected, output);
    }

    [Fact]
    public void Write_InterfaceWithSomeOptionalProperties_WritesOnlySpecifiedProperties()
    {
        var config = new WireguardConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
            PostUp = "iptables -A FORWARD -i wg0 -j ACCEPT",
        };

        var output = WriteConfiguration(config);

        var expected = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            PostUp = iptables -A FORWARD -i wg0 -j ACCEPT


            """;

        Assert.Equal(expected, output);
    }

    [Fact]
    public void Write_MinimalPeerConfiguration_WritesCorrectFormat()
    {
        var config = new WireguardConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
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

        var output = WriteConfiguration(config);

        var expected = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            AllowedIPs = 10.0.0.2/32
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=


            """;

        Assert.Equal(expected, output);
    }

    [Fact]
    public void Write_PeerWithMultipleAllowedIPs_WritesCommaSeparatedList()
    {
        var config = new WireguardConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
        };

        config.Peers.Add(
            new WireguardPeerConfiguration
            {
                PublicKey = Convert.FromBase64String(
                    "xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg="
                ),
                AllowedIPs =
                [
                    CIDR.Parse("10.0.0.2/32"),
                    CIDR.Parse("10.0.0.3/32"),
                    CIDR.Parse("192.168.1.0/24"),
                ],
            }
        );

        var output = WriteConfiguration(config);

        Assert.Contains("AllowedIPs = 10.0.0.2/32, 10.0.0.3/32, 192.168.1.0/24", output);
    }

    [Fact]
    public void Write_PeerWithAllOptionalProperties_WritesAllProperties()
    {
        var config = new WireguardConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
        };

        config.Peers.Add(
            new WireguardPeerConfiguration
            {
                PublicKey = Convert.FromBase64String(
                    "xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg="
                ),
                AllowedIPs = [CIDR.Parse("10.0.0.2/32")],
                Endpoint = WireguardEndpoint.Parse("vpn.example.com:51820"),
                PresharedKey = Convert.FromBase64String(
                    "FpCyhws9cxwWoV4xELtfJvjJN+zQVRPISllRWgeopVE="
                ),
                PersistedKeepalive = 25,
            }
        );

        var output = WriteConfiguration(config);

        var expected = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            AllowedIPs = 10.0.0.2/32
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            PresharedKey = FpCyhws9cxwWoV4xELtfJvjJN+zQVRPISllRWgeopVE=
            Endpoint = vpn.example.com:51820
            PersistedKeepalive = 25


            """;

        Assert.Equal(expected, output);
    }

    [Fact]
    public void Write_MultiplePeers_WritesSeparatePeerSections()
    {
        var config = new WireguardConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
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

        config.Peers.Add(
            new WireguardPeerConfiguration
            {
                PublicKey = Convert.FromBase64String(
                    "TrMvSoP4jYQlY6RIzBgbssQqY3vxI2Pi+y71lOWWXX0="
                ),
                AllowedIPs = [CIDR.Parse("10.0.0.3/32")],
                Endpoint = WireguardEndpoint.Parse("peer2.example.com:51820"),
                PersistedKeepalive = 30,
            }
        );

        var output = WriteConfiguration(config);

        var expected = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            AllowedIPs = 10.0.0.2/32
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=

            [Peer]
            AllowedIPs = 10.0.0.3/32
            PublicKey = TrMvSoP4jYQlY6RIzBgbssQqY3vxI2Pi+y71lOWWXX0=
            Endpoint = peer2.example.com:51820
            PersistedKeepalive = 30


            """;

        Assert.Equal(expected, output);
    }

    [Fact]
    public void Write_IPv6Address_WritesCorrectFormat()
    {
        var config = new WireguardConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
            ListenPort = 51820,
            Address = CIDR.Parse("fd00::1/64"),
        };

        var output = WriteConfiguration(config);

        Assert.Contains("Address = fd00::1/64", output);
    }

    [Fact]
    public void Write_EmptyPeersList_WritesOnlyInterface()
    {
        var config = new WireguardConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
        };

        var output = WriteConfiguration(config);

        Assert.DoesNotContain("[Peer]", output);
        Assert.Contains("[Interface]", output);
    }

    private static string WriteConfiguration(WireguardConfiguration config)
    {
        using var stringWriter = new StringWriter();
        using var writer = new WireguardConfigurationWriter(stringWriter);
        writer.Write(config);
        return stringWriter.ToString();
    }
}
