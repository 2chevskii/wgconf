using System.Net;

namespace WgConf.Tests;

public class CIDRTryParseTests
{
    [Theory]
    [InlineData("192.168.1.0", "CIDR notation must contain")]
    [InlineData("not-an-ip/24", "Invalid IP address")]
    [InlineData("192.168.1.0/abc", "Invalid prefix length")]
    [InlineData("192.168.1.0/33", "Prefix length must be between 0 and 32")]
    public void TryParse_InvalidInput_ReturnsFalseWithException(
        string input,
        string expectedMessageFragment
    )
    {
        var success = CIDR.TryParse(input, out var cidr, out var exception);

        Assert.False(success);
        Assert.Equal(default, cidr);
        Assert.NotNull(exception);
        Assert.Contains(expectedMessageFragment, exception!.Message);
    }

    [Fact]
    public void TryParse_ValidInput_ReturnsTrue()
    {
        var success = CIDR.TryParse("10.0.0.1/24", out var cidr, out var exception);

        Assert.True(success);
        Assert.Null(exception);
        Assert.Equal(IPAddress.Parse("10.0.0.1"), cidr.Address);
        Assert.Equal(24, cidr.PrefixLength);
    }

    [Fact]
    public void ImplicitOperator_FromReadOnlySpan_Parses()
    {
        ReadOnlySpan<char> input = "10.0.0.1/24".AsSpan();
        CIDR cidr = input;

        Assert.Equal(IPAddress.Parse("10.0.0.1"), cidr.Address);
        Assert.Equal(24, cidr.PrefixLength);
    }
}
