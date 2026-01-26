using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace WgConf.Amnezia.Tests;

public class WgConfBaseCoverageTests
{
    [Fact]
    public void Cidr_TryParse_InvalidInput_ReturnsFalse()
    {
        var success = CIDR.TryParse("10.0.0.1/33", out var cidr, out var ex);

        Assert.False(success);
        Assert.Equal(default, cidr);
        Assert.NotNull(ex);
        Assert.Contains("Prefix length must be between", ex!.Message);
    }

    [Theory]
    [InlineData("10.0.0.1", "CIDR notation must contain")]
    [InlineData("not-an-ip/24", "Invalid IP address")]
    [InlineData("10.0.0.1/abc", "Invalid prefix length")]
    public void Cidr_TryParse_InvalidFormats_ReturnFalse(string input, string messageFragment)
    {
        var success = CIDR.TryParse(input, out var cidr, out var ex);

        Assert.False(success);
        Assert.Equal(default, cidr);
        Assert.NotNull(ex);
        Assert.Contains(messageFragment, ex!.Message);
    }

    [Fact]
    public void Cidr_ImplicitOperator_FromReadOnlySpan_Parses()
    {
        ReadOnlySpan<char> input = "10.0.0.1/24".AsSpan();
        CIDR cidr = input;

        Assert.Equal(IPAddress.Parse("10.0.0.1"), cidr.Address);
        Assert.Equal(24, cidr.PrefixLength);
    }

    [Fact]
    public void WireguardEndpoint_ImplicitOperator_And_InvalidIpv6_ReturnsFalse()
    {
        ReadOnlySpan<char> input = "host:51820".AsSpan();
        WireguardEndpoint endpoint = input;

        Assert.Equal("host", endpoint.Host);
        Assert.Equal(51820, endpoint.Port);

        var success = WireguardEndpoint.TryParse("[invalid-ipv6]:51820", out var _, out var ex);
        Assert.False(success);
        Assert.NotNull(ex);
    }

    [Theory]
    [InlineData("example.com", "Endpoint must be in format")]
    [InlineData(":51820", "Endpoint host cannot be empty")]
    [InlineData("example.com:abc", "Invalid port number")]
    [InlineData("example.com:0", "Port must be between")]
    public void WireguardEndpoint_TryParse_InvalidInputs_ReturnFalse(
        string input,
        string messageFragment
    )
    {
        var success = WireguardEndpoint.TryParse(input, out var endpoint, out var ex);

        Assert.False(success);
        Assert.Equal(default, endpoint);
        Assert.NotNull(ex);
        Assert.Contains(messageFragment, ex!.Message);
    }

    [Fact]
    public void WireguardEndpoint_ToString_FormatsIpv6()
    {
        var endpoint = new WireguardEndpoint { Host = "::1", Port = 51820 };

        Assert.Equal("[::1]:51820", endpoint.ToString());
    }

    [Fact]
    public void AddressFamilyExtensions_Unsupported_Throws()
    {
        var ex = Assert.Throws<TargetInvocationException>(() =>
            InvokeExtension("FriendlyName", (AddressFamily)999)
        );
        Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
    }

    [Fact]
    public void AddressFamilyExtensions_MaxPrefixLength_ReturnsValues()
    {
        Assert.Equal(32, (int)InvokeExtension("MaxPrefixLength", AddressFamily.InterNetwork)!);
        Assert.Equal(128, (int)InvokeExtension("MaxPrefixLength", AddressFamily.InterNetworkV6)!);
    }

    [Fact]
    public void ParseError_ToString_WithContext_IncludesMarkers()
    {
        var error = new ParseError(
            2,
            "Bad value",
            PropertyName: "Key",
            Section: "Interface",
            LineText: "Key = value",
            SurroundingLines: new[] { "line1", "Key = value", "line3" }
        );

        var text = error.ToString();

        Assert.Contains("Line 2", text);
        Assert.Contains("Context:", text);
        Assert.Contains(">>>", text);
    }

    [Fact]
    public void WireguardConfigurationException_NoErrors_UsesDefaultMessage()
    {
        var exception = new WireguardConfigurationException(Array.Empty<ParseError>());

        Assert.Equal("Configuration parsing failed", exception.Message);
    }

    [Fact]
    public void WireguardConfigurationException_MultipleErrors_FormatsMessage()
    {
        var errors = new[] { new ParseError(1, "First"), new ParseError(2, "Second") };

        var exception = new WireguardConfigurationException(errors);

        Assert.Contains("Error 1", exception.Message);
        Assert.Contains("Error 2", exception.Message);
    }

    private static object? InvokeExtension(string nameFragment, AddressFamily family)
    {
        var type = typeof(CIDR).Assembly.GetType("WgConf.Extensions.AddressFamilyExtensions");
        Assert.NotNull(type);

        var method = type!
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(m =>
                (
                    m.Name.Equals(nameFragment, StringComparison.Ordinal)
                    || m.Name.Equals($"get_{nameFragment}", StringComparison.Ordinal)
                )
                && m.GetParameters().Length == 1
            );

        return method.Invoke(null, new object[] { family });
    }
}
