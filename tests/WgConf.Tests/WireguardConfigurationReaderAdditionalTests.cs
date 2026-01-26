namespace WgConf.Tests;

public class WireguardConfigurationReaderAdditionalTests
{
    private const string PrivateKey = "YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=";
    private const string PublicKey = "xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=";

    [Fact]
    public async Task DisposeAsync_UsesAsyncDisposable()
    {
        var textReader = new TrackingTextReader();
        var reader = new WireguardConfigurationReader(textReader);

        await reader.DisposeAsync();

        Assert.True(textReader.DisposeAsyncCalled);
    }

    [Fact]
    public void TryRead_UnknownPeerProperty_AddsError()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = {{PublicKey}}
            AllowedIPs = 10.0.0.2/32
            UnknownPeerProp = value
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out var result, out var errors);

        Assert.False(success);
        Assert.Null(result);
        Assert.Contains(errors, e => e.Message.Contains("Unknown property 'UnknownPeerProp'"));
    }

    [Fact]
    public void TryRead_AllowedIPs_WithEmptyEntries_IgnoresEmpty()
    {
        var config = $$"""
            [Interface]
            PrivateKey = {{PrivateKey}}
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = {{PublicKey}}
            AllowedIPs = 10.0.0.2/32,  , 10.0.0.3/32
            """;

        using var reader = new WireguardConfigurationReader(new StringReader(config));
        var success = reader.TryRead(out var result, out var errors);

        Assert.True(success);
        Assert.Empty(errors);
        Assert.NotNull(result);
        Assert.Equal(2, result!.Peers[0].AllowedIPs.Count);
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
