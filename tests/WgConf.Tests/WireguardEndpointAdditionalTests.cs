namespace WgConf.Tests;

public class WireguardEndpointAdditionalTests
{
    [Fact]
    public void ImplicitOperator_FromReadOnlySpan_Parses()
    {
        ReadOnlySpan<char> input = "example.com:51820".AsSpan();
        WireguardEndpoint endpoint = input;

        Assert.Equal("example.com", endpoint.Host);
        Assert.Equal(51820, endpoint.Port);
    }

    [Fact]
    public void TryParse_InvalidIpv6InBrackets_ReturnsFalse()
    {
        var success = WireguardEndpoint.TryParse("[not-ipv6]:51820", out var endpoint, out var ex);

        Assert.False(success);
        Assert.Equal(default, endpoint);
        Assert.NotNull(ex);
        Assert.Contains("Invalid IPv6 address", ex!.Message);
    }
}
