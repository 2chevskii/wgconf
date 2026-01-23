namespace WgConf.Tests;

public class WireguardConfigurationReaderTests
{
    [Fact]
    public void Read_MinimalConfiguration_ParsesCorrectly()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        var config = ReadConfiguration(input);

        Assert.Equal(
            Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
            config.PrivateKey
        );
        Assert.Equal(51820, config.ListenPort);
        Assert.Equal("10.0.0.1", config.Address.Address.ToString());
        Assert.Equal(24, config.Address.PrefixLength);
        Assert.Empty(config.Peers);
    }

    [Fact]
    public void Read_CompleteInterfaceConfiguration_ParsesAllProperties()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            PreUp = echo 'Starting WireGuard'
            PostUp = iptables -A FORWARD -i wg0 -j ACCEPT
            PreDown = echo 'Stopping WireGuard'
            PostDown = iptables -D FORWARD -i wg0 -j ACCEPT
            """;

        var config = ReadConfiguration(input);

        Assert.Equal("echo 'Starting WireGuard'", config.PreUp);
        Assert.Equal("iptables -A FORWARD -i wg0 -j ACCEPT", config.PostUp);
        Assert.Equal("echo 'Stopping WireGuard'", config.PreDown);
        Assert.Equal("iptables -D FORWARD -i wg0 -j ACCEPT", config.PostDown);
    }

    [Fact]
    public void Read_ConfigurationWithPeer_ParsesCorrectly()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = 10.0.0.2/32
            """;

        var config = ReadConfiguration(input);

        Assert.Single(config.Peers);
        var peer = config.Peers[0];
        Assert.Equal(
            Convert.FromBase64String("xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg="),
            peer.PublicKey
        );
        Assert.Single(peer.AllowedIPs);
        Assert.Equal("10.0.0.2/32", peer.AllowedIPs[0].ToString());
    }

    [Fact]
    public void Read_PeerWithMultipleAllowedIPs_ParsesCorrectly()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = 10.0.0.2/32, 10.0.0.3/32, 192.168.1.0/24
            """;

        var config = ReadConfiguration(input);

        Assert.Single(config.Peers);
        var peer = config.Peers[0];
        Assert.Equal(3, peer.AllowedIPs.Length);
        Assert.Equal("10.0.0.2/32", peer.AllowedIPs[0].ToString());
        Assert.Equal("10.0.0.3/32", peer.AllowedIPs[1].ToString());
        Assert.Equal("192.168.1.0/24", peer.AllowedIPs[2].ToString());
    }

    [Fact]
    public void Read_PeerWithAllOptionalProperties_ParsesCorrectly()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = 10.0.0.2/32
            Endpoint = vpn.example.com:51820
            PresharedKey = FpCyhws9cxwWoV4xELtfJvjJN+zQVRPISllRWgeopVE=
            PersistedKeepalive = 25
            """;

        var config = ReadConfiguration(input);

        Assert.Single(config.Peers);
        var peer = config.Peers[0];
        Assert.NotNull(peer.Endpoint);
        Assert.Equal("vpn.example.com:51820", peer.Endpoint.Value.ToString());
        Assert.NotNull(peer.PresharedKey);
        Assert.Equal(
            Convert.FromBase64String("FpCyhws9cxwWoV4xELtfJvjJN+zQVRPISllRWgeopVE="),
            peer.PresharedKey
        );
        Assert.Equal(25, peer.PersistedKeepalive);
    }

    [Fact]
    public void Read_MultiplePeers_ParsesCorrectly()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = 10.0.0.2/32

            [Peer]
            PublicKey = TrMvSoP4jYQlY6RIzBgbssQqY3vxI2Pi+y71lOWWXX0=
            AllowedIPs = 10.0.0.3/32
            Endpoint = peer2.example.com:51820
            PersistedKeepalive = 30
            """;

        var config = ReadConfiguration(input);

        Assert.Equal(2, config.Peers.Count);
        Assert.Equal(
            Convert.FromBase64String("xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg="),
            config.Peers[0].PublicKey
        );
        Assert.Equal(
            Convert.FromBase64String("TrMvSoP4jYQlY6RIzBgbssQqY3vxI2Pi+y71lOWWXX0="),
            config.Peers[1].PublicKey
        );
    }

    [Fact]
    public void Read_ConfigurationWithComments_StripsComments()
    {
        var input = """
            # This is a comment
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY= # inline comment
            ListenPort = 51820
            Address = 10.0.0.1/24

            # Peer section
            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = 10.0.0.2/32 # allowed IP
            """;

        var config = ReadConfiguration(input);

        Assert.Equal(51820, config.ListenPort);
        Assert.Single(config.Peers);
    }

    [Fact]
    public void Read_CaseInsensitiveProperties_ParsesCorrectly()
    {
        var input = """
            [Interface]
            privatekey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            LISTENPORT = 51820
            address = 10.0.0.1/24

            [Peer]
            publickey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            allowedips = 10.0.0.2/32
            """;

        var config = ReadConfiguration(input);

        Assert.Equal(51820, config.ListenPort);
        Assert.Single(config.Peers);
    }

    [Fact]
    public void Read_DuplicateProperties_UsesLastValue()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            ListenPort = 51821
            Address = 10.0.0.1/24
            """;

        var config = ReadConfiguration(input);

        Assert.Equal(51821, config.ListenPort);
    }

    [Fact]
    public void Read_IPv6Address_ParsesCorrectly()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = fd00::1/64
            """;

        var config = ReadConfiguration(input);

        Assert.Equal("fd00::1", config.Address.Address.ToString());
        Assert.Equal(64, config.Address.PrefixLength);
    }

    [Fact]
    public void Read_EmptyLines_Ignored()
    {
        var input = """
            [Interface]

            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=

            ListenPort = 51820
            Address = 10.0.0.1/24

            """;

        var config = ReadConfiguration(input);

        Assert.Equal(51820, config.ListenPort);
    }

    [Fact]
    public void Read_UnknownProperty_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            InvalidProperty = SomeValue
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Single(ex.Errors);
        Assert.Contains("InvalidProperty", ex.Errors[0].Message);
    }

    [Fact]
    public void Read_UnknownSection_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [InvalidSection]
            SomeProperty = SomeValue
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Single(ex.Errors);
        Assert.Contains("InvalidSection", ex.Errors[0].Message);
    }

    [Fact]
    public void Read_PropertyOutsideSection_ThrowsWithError()
    {
        var input = """
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=

            [Interface]
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.NotEmpty(ex.Errors);
        Assert.Contains(ex.Errors, e => e.Message.Contains("outside of any section"));
    }

    [Fact]
    public void Read_InvalidPropertyFormat_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            InvalidLine without equals sign
            Address = 10.0.0.1/24
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Single(ex.Errors);
        Assert.Contains("Invalid property format", ex.Errors[0].Message);
    }

    [Fact]
    public void Read_InvalidBase64PrivateKey_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = not-valid-base64!!!
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(ex.Errors, e => e.Message.Contains("PrivateKey"));
    }

    [Fact]
    public void Read_InvalidListenPort_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = not-a-number
            Address = 10.0.0.1/24
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(ex.Errors, e => e.Message.Contains("ListenPort"));
    }

    [Fact]
    public void Read_InvalidCIDRAddress_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = invalid-cidr
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(ex.Errors, e => e.Message.Contains("Address"));
    }

    [Fact]
    public void TryRead_ValidConfiguration_ReturnsTrue()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(input));
        var success = reader.TryRead(out var config, out var errors);

        Assert.True(success);
        Assert.NotNull(config);
        Assert.Empty(errors);
    }

    [Fact]
    public void TryRead_InvalidConfiguration_ReturnsFalse()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            InvalidProperty = Value
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(input));
        var success = reader.TryRead(out var config, out var errors);

        Assert.False(success);
        Assert.Null(config);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Read_RoundTripWithWriter_PreservesConfiguration()
    {
        var original = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = 10.0.0.2/32
            """;

        var config = ReadConfiguration(original);

        using var stringWriter = new StringWriter();
        using var writer = new WireguardConfigurationWriter(stringWriter);
        writer.Write(config);
        var written = stringWriter.ToString();

        var reparsed = ReadConfiguration(written);

        Assert.Equal(config.PrivateKey, reparsed.PrivateKey);
        Assert.Equal(config.ListenPort, reparsed.ListenPort);
        Assert.Equal(config.Address.ToString(), reparsed.Address.ToString());
        Assert.Equal(config.Peers.Count, reparsed.Peers.Count);
        Assert.Equal(config.Peers[0].PublicKey, reparsed.Peers[0].PublicKey);
        Assert.Equal(
            config.Peers[0].AllowedIPs[0].ToString(),
            reparsed.Peers[0].AllowedIPs[0].ToString()
        );
    }

    private static WireguardConfiguration ReadConfiguration(string input)
    {
        using var reader = new WireguardConfigurationReader(new StringReader(input));
        return reader.Read();
    }
}
