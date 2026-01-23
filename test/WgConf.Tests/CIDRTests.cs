using System.Net;
using System.Net.Sockets;

namespace WgConf.Tests;

public class CIDRTests
{
    [Theory]
    [InlineData("192.168.1.0/24", "192.168.1.0", 24)]
    [InlineData("10.0.0.1/32", "10.0.0.1", 32)]
    [InlineData("172.16.0.0/16", "172.16.0.0", 16)]
    [InlineData("0.0.0.0/0", "0.0.0.0", 0)]
    public void Parse_ValidIPv4CIDR_ReturnsCorrectCIDR(
        string input,
        string expectedAddress,
        int expectedPrefix
    )
    {
        var cidr = CIDR.Parse(input);

        Assert.Equal(IPAddress.Parse(expectedAddress), cidr.Address);
        Assert.Equal(expectedPrefix, cidr.PrefixLength);
    }

    [Theory]
    [InlineData("fd00::1/64", "fd00::1", 64)]
    [InlineData("2001:db8::/32", "2001:db8::", 32)]
    [InlineData("fe80::1/128", "fe80::1", 128)]
    [InlineData("::/0", "::", 0)]
    public void Parse_ValidIPv6CIDR_ReturnsCorrectCIDR(
        string input,
        string expectedAddress,
        int expectedPrefix
    )
    {
        var cidr = CIDR.Parse(input);

        Assert.Equal(IPAddress.Parse(expectedAddress), cidr.Address);
        Assert.Equal(expectedPrefix, cidr.PrefixLength);
    }

    [Theory]
    [InlineData("192.168.1.0")]
    [InlineData("not-an-ip/24")]
    [InlineData("")]
    public void Parse_InvalidFormat_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => CIDR.Parse(input));
    }

    [Theory]
    [InlineData("192.168.1.0/33")]
    [InlineData("10.0.0.1/-1")]
    [InlineData("172.16.0.0/256")]
    public void Parse_InvalidIPv4PrefixLength_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => CIDR.Parse(input));
    }

    [Theory]
    [InlineData("fd00::1/129")]
    [InlineData("2001:db8::/-1")]
    [InlineData("fe80::1/256")]
    public void Parse_InvalidIPv6PrefixLength_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => CIDR.Parse(input));
    }

    [Theory]
    [InlineData("192.168.1.0/abc")]
    [InlineData("10.0.0.1/")]
    [InlineData("172.16.0.0/1.5")]
    public void Parse_NonNumericPrefixLength_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => CIDR.Parse(input));
    }

    [Fact]
    public void Parse_ReadOnlySpanOverload_WorksCorrectly()
    {
        ReadOnlySpan<char> span = "10.0.0.1/24".AsSpan();
        var cidr = CIDR.Parse(span);

        Assert.Equal(IPAddress.Parse("10.0.0.1"), cidr.Address);
        Assert.Equal(24, cidr.PrefixLength);
    }

    [Theory]
    [InlineData("192.168.1.0/24", "192.168.1.0/24")]
    [InlineData("10.0.0.1/32", "10.0.0.1/32")]
    [InlineData("fd00::1/64", "fd00::1/64")]
    [InlineData("2001:db8::/32", "2001:db8::/32")]
    public void ToString_ReturnsCorrectFormat(string input, string expected)
    {
        var cidr = CIDR.Parse(input);
        Assert.Equal(expected, cidr.ToString());
    }

    [Fact]
    public void ToString_IPv6_UsesShorthandNotation()
    {
        var cidr = new CIDR
        {
            Address = IPAddress.Parse("2001:0db8:0000:0000:0000:0000:0000:0001"),
            PrefixLength = 64,
        };

        Assert.Equal("2001:db8::1/64", cidr.ToString());
    }

    [Fact]
    public void Parse_AndToString_RoundTrip()
    {
        var original = "192.168.1.0/24";
        var cidr = CIDR.Parse(original);
        var result = cidr.ToString();

        Assert.Equal(original, result);
    }
}
