namespace WgConf.Tests;

public class WireguardEndpointTests
{
    [Theory]
    [InlineData("example.com:51820", "example.com", 51820)]
    [InlineData("vpn.example.com:12345", "vpn.example.com", 12345)]
    [InlineData("192.168.1.1:51820", "192.168.1.1", 51820)]
    [InlineData("10.0.0.1:1", "10.0.0.1", 1)]
    [InlineData("host:65535", "host", 65535)]
    public void Parse_ValidEndpoint_ReturnsCorrectValues(
        string input,
        string expectedHost,
        int expectedPort
    )
    {
        var endpoint = WireguardEndpoint.Parse(input);

        Assert.Equal(expectedHost, endpoint.Host);
        Assert.Equal(expectedPort, endpoint.Port);
    }

    [Theory]
    [InlineData("[::1]:51820", "::1", 51820)]
    [InlineData("[2001:db8::1]:12345", "2001:db8::1", 12345)]
    [InlineData("[fe80::1]:51820", "fe80::1", 51820)]
    public void Parse_IPv6Endpoint_ReturnsCorrectValues(
        string input,
        string expectedHost,
        int expectedPort
    )
    {
        var endpoint = WireguardEndpoint.Parse(input);

        Assert.Equal(expectedHost, endpoint.Host);
        Assert.Equal(expectedPort, endpoint.Port);
    }

    [Theory]
    [InlineData("example.com")]
    [InlineData("")]
    [InlineData(":51820")]
    public void Parse_InvalidFormat_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => WireguardEndpoint.Parse(input));
    }

    [Theory]
    [InlineData("example.com:0")]
    [InlineData("example.com:65536")]
    [InlineData("example.com:-1")]
    public void Parse_InvalidPort_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => WireguardEndpoint.Parse(input));
    }

    [Theory]
    [InlineData("example.com:abc")]
    [InlineData("example.com:")]
    [InlineData("example.com:1.5")]
    public void Parse_NonNumericPort_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => WireguardEndpoint.Parse(input));
    }

    [Fact]
    public void TryParse_ValidEndpoint_ReturnsTrue()
    {
        var success = WireguardEndpoint.TryParse(
            "example.com:51820",
            out var endpoint,
            out var exception
        );

        Assert.True(success);
        Assert.Null(exception);
        Assert.Equal("example.com", endpoint.Host);
        Assert.Equal(51820, endpoint.Port);
    }

    [Fact]
    public void TryParse_InvalidEndpoint_ReturnsFalse()
    {
        var success = WireguardEndpoint.TryParse("invalid", out var endpoint, out var exception);

        Assert.False(success);
        Assert.NotNull(exception);
        Assert.Equal(default, endpoint);
    }

    [Theory]
    [InlineData("example.com", 51820, "example.com:51820")]
    [InlineData("192.168.1.1", 12345, "192.168.1.1:12345")]
    public void ToString_IPv4OrHostname_ReturnsCorrectFormat(string host, int port, string expected)
    {
        var endpoint = new WireguardEndpoint { Host = host, Port = port };
        Assert.Equal(expected, endpoint.ToString());
    }

    [Fact]
    public void ToString_IPv6Address_ReturnsBracketedFormat()
    {
        var endpoint = new WireguardEndpoint { Host = "::1", Port = 51820 };
        Assert.Equal("[::1]:51820", endpoint.ToString());
    }

    [Fact]
    public void Parse_RoundTrip_PreservesValue()
    {
        var original = "vpn.example.com:51820";
        var endpoint = WireguardEndpoint.Parse(original);
        var result = endpoint.ToString();

        Assert.Equal(original, result);
    }

    [Fact]
    public void Parse_IPv6RoundTrip_PreservesValue()
    {
        var original = "[2001:db8::1]:51820";
        var endpoint = WireguardEndpoint.Parse(original);
        var result = endpoint.ToString();

        Assert.Equal(original, result);
    }
}
