namespace WgConf.Amnezia.Tests;

public class WireguardConfigurationReaderCoverageTests
{
    private const string PrivateKey = "YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=";
    private const string PublicKey = "xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=";

    [Fact]
    public async Task DisposeAsync_CallsAsyncDispose()
    {
        var textReader = new TrackingTextReader();
        var reader = new WireguardConfigurationReader(textReader);

        await reader.DisposeAsync();

        Assert.True(textReader.DisposeAsyncCalled);
    }

    [Fact]
    public void Read_InvalidConfiguration_ThrowsException()
    {
        var config = """
            [Interface]
            PrivateKey = invalid
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        Assert.Throws<WireguardConfigurationException>(() => reader.Read());
    }

    [Fact]
    public void TryRead_WithStructuralErrors_ReturnsErrors()
    {
        var config = $$"""
            OrphanKey = value
            [Unknown]
            Key = Value
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 51820
            Address = 10.0.0.1/24
            BadInterfaceProp = value
            BadLineWithoutEquals
            [Peer]
            PublicKey = {{PublicKey}}
            AllowedIPs = 10.0.0.2/32
            UnknownPeerProp = value
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out var result, out var errors);

        Assert.False(success);
        Assert.Null(result);
        Assert.Contains(errors, e => e.Message.Contains("Property found outside"));
        Assert.Contains(errors, e => e.Message.Contains("Unknown section"));
        Assert.Contains(errors, e => e.Message.Contains("Unknown property 'BadInterfaceProp'"));
        Assert.Contains(errors, e => e.Message.Contains("Invalid property format"));
        Assert.Contains(errors, e => e.Message.Contains("Unknown property 'UnknownPeerProp'"));
    }

    [Fact]
    public void TryRead_WithMultiplePeers_AddsPeers()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = {{PublicKey}}
            AllowedIPs = 10.0.0.2/32

            [Peer]
            PublicKey = {{PublicKey}}
            AllowedIPs = 10.0.0.3/32
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out var result, out var errors);

        Assert.True(success);
        Assert.Empty(errors);
        Assert.NotNull(result);
        Assert.Equal(2, result!.Peers.Count);
    }

    [Fact]
    public void TryRead_WithInterfaceScripts_SetsProperties()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 51820
            Address = 10.0.0.1/24
            PreUp = pre-up
            PostUp = post-up
            PreDown = pre-down
            PostDown = post-down
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out var result, out var errors);

        Assert.True(success);
        Assert.Empty(errors);
        Assert.NotNull(result);
        Assert.Equal("pre-up", result!.PreUp);
        Assert.Equal("post-up", result.PostUp);
        Assert.Equal("pre-down", result.PreDown);
        Assert.Equal("post-down", result.PostDown);
    }

    [Fact]
    public void TryRead_MissingInterfaceProperties_ReturnsErrors()
    {
        var config = """
            [Interface]
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out var result, out var errors);

        Assert.False(success);
        Assert.Null(result);
        Assert.Contains(errors, e => e.Message.Contains("PrivateKey"));
        Assert.Contains(errors, e => e.Message.Contains("ListenPort"));
        Assert.Contains(errors, e => e.Message.Contains("Address"));
    }

    [Fact]
    public void TryRead_InvalidPrivateKeyFormat_ReturnsError()
    {
        var config = $$"""
            [Interface]
            PrivateKey = invalid
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Message.Contains("Invalid PrivateKey format"));
    }

    [Fact]
    public void TryRead_InvalidPrivateKeyLength_ReturnsError()
    {
        var config = $$"""
            [Interface]
            PrivateKey = AA==
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Message.Contains("Invalid PrivateKey length"));
    }

    [Fact]
    public void TryRead_InvalidListenPortFormat_ReturnsError()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = abc
            Address = 10.0.0.1/24
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Message.Contains("Invalid ListenPort format"));
    }

    [Fact]
    public void TryRead_InvalidListenPortRange_ReturnsError()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 70000
            Address = 10.0.0.1/24
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Message.Contains("Invalid ListenPort value"));
    }

    [Fact]
    public void TryRead_InvalidAddress_ReturnsError()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 51820
            Address = 10.0.0.1
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Message.Contains("Invalid Address format"));
    }

    [Fact]
    public void TryRead_MissingPeerProperties_ReturnsErrors()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Message.Contains("PublicKey"));
        Assert.Contains(errors, e => e.Message.Contains("AllowedIPs"));
    }

    [Fact]
    public void TryRead_InvalidPublicKeyFormat_ReturnsError()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = invalid
            AllowedIPs = 10.0.0.2/32
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Message.Contains("Invalid PublicKey format"));
    }

    [Fact]
    public void TryRead_InvalidPublicKeyLength_ReturnsError()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = AA==
            AllowedIPs = 10.0.0.2/32
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Message.Contains("Invalid PublicKey length"));
    }

    [Fact]
    public void TryRead_InvalidAllowedIps_ReturnsErrors()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = {{PublicKey}}
            AllowedIPs = 10.0.0.2/33, , invalid
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Message.Contains("Invalid CIDR in AllowedIPs"));
    }

    [Fact]
    public void TryRead_InvalidEndpointAndKeepalive_ReturnsErrors()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = {{PublicKey}}
            AllowedIPs = 10.0.0.2/32
            Endpoint = invalid
            PresharedKey = invalid
            PersistedKeepalive = abc
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Message.Contains("Invalid Endpoint format"));
        Assert.Contains(errors, e => e.Message.Contains("Invalid PresharedKey format"));
        Assert.Contains(errors, e => e.Message.Contains("Invalid PersistedKeepalive format"));
    }

    [Fact]
    public void TryRead_InvalidPresharedKeyLength_ReturnsError()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = {{PublicKey}}
            AllowedIPs = 10.0.0.2/32
            PresharedKey = AA==
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out _, out var errors);

        Assert.False(success);
        Assert.Contains(errors, e => e.Message.Contains("Invalid PresharedKey length"));
    }

    [Fact]
    public void TryRead_WhenBuildConfigurationThrows_ReturnsUnexpectedError()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        using var reader = new ThrowingWireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out var result, out var errors);

        Assert.False(success);
        Assert.Null(result);
        Assert.Contains(errors, e => e.Message.Contains("Unexpected error during parsing"));
    }

    [Fact]
    public void BuildPeer_WhenUnexpectedException_ReturnsNull()
    {
        var reader = new ExposedWireguardConfigurationReader(new StringReader(string.Empty));
        var peerProps = new Dictionary<string, string>
        {
            ["PublicKey"] = PublicKey,
            ["AllowedIPs"] = null!,
        };
        var errors = new List<ParseError>();

        var peer = reader.CallBuildPeer(peerProps, errors);

        Assert.Null(peer);
    }

    private sealed class TrackingTextReader : TextReader, IAsyncDisposable
    {
        public bool DisposeAsyncCalled { get; private set; }

        public override string? ReadLine() => null;

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            DisposeAsyncCalled = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ThrowingWireguardConfigurationReader(TextReader textReader)
        : WireguardConfigurationReader(textReader)
    {
        protected override WireguardConfiguration BuildConfiguration(
            Dictionary<string, string> interfaceProps,
            List<Dictionary<string, string>> peers,
            List<string> allLines,
            List<ParseError> errors
        )
        {
            throw new InvalidOperationException("boom");
        }
    }

    private sealed class ExposedWireguardConfigurationReader(TextReader textReader)
        : WireguardConfigurationReader(textReader)
    {
        public WireguardPeerConfiguration? CallBuildPeer(
            Dictionary<string, string> peerProps,
            List<ParseError> errors
        )
        {
            return base.BuildPeer(peerProps, new List<string>(), errors);
        }
    }
}
