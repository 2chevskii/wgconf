using WgConf.Amnezia;
using Xunit;

namespace WgConf.Amnezia.Tests;

public class MagicHeaderTests
{
    [Fact]
    public void Parse_ValidRange_ReturnsCorrectValues()
    {
        var range = MagicHeader.Parse("25-30");

        Assert.Equal(25ul, range.Start);
        Assert.Equal(30ul, range.End);
    }

    [Fact]
    public void Parse_SingleValue_ReturnsCorrectValues()
    {
        var range = MagicHeader.Parse("25");

        Assert.Equal(25ul, range.Start);
        Assert.Null(range.End);
    }

    [Fact]
    public void Parse_SingleNegativeValue_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => MagicHeader.Parse("-10"));
    }

    [Fact]
    public void Parse_SingleValueWithWhitespace_Parses()
    {
        var range = MagicHeader.Parse("  42  ");

        Assert.Equal(42ul, range.Start);
        Assert.Null(range.End);
    }

    [Fact]
    public void Parse_SameStartEnd_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MagicHeader.Parse("5-5"));
    }

    [Fact]
    public void Parse_EndLessThanStart_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MagicHeader.Parse("30-25"));
    }

    [Fact]
    public void Parse_NegativeNumbers_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => MagicHeader.Parse("-10-5"));
    }

    [Fact]
    public void Parse_LargeNumbers_Parses()
    {
        var range = MagicHeader.Parse("1000000-2000000");

        Assert.Equal(1000000ul, range.Start);
        Assert.Equal(2000000ul, range.End);
    }

    [Fact]
    public void Parse_WithWhitespace_Parses()
    {
        var range = MagicHeader.Parse(" 10 - 20 ");

        Assert.Equal(10ul, range.Start);
        Assert.Equal(20ul, range.End);
    }

    [Fact]
    public void Parse_InvalidSingleValue_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => MagicHeader.Parse("abc"));
    }

    [Fact]
    public void Parse_InvalidStartNumber_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => MagicHeader.Parse("abc-20"));
    }

    [Fact]
    public void Parse_InvalidEndNumber_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => MagicHeader.Parse("10-xyz"));
    }

    [Fact]
    public void Parse_EmptyString_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => MagicHeader.Parse(""));
    }

    [Fact]
    public void TryParse_ValidRange_ReturnsTrue()
    {
        var success = MagicHeader.TryParse("25-30", out var range);

        Assert.True(success);
        Assert.Equal(25ul, range.Start);
        Assert.Equal(30ul, range.End);
    }

    [Fact]
    public void TryParse_ValidSingleValue_ReturnsTrue()
    {
        var success = MagicHeader.TryParse("42", out var range);

        Assert.True(success);
        Assert.Equal(42ul, range.Start);
        Assert.Null(range.End);
    }

    [Fact]
    public void TryParse_InvalidFormat_ReturnsFalse()
    {
        var success = MagicHeader.TryParse("invalid", out var range);

        Assert.False(success);
        Assert.Equal(default, range);
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        MagicHeader range = [25ul, 30ul];

        Assert.Equal("25-30", range.ToString());
    }

    [Fact]
    public void ToString_SingleValue_ReturnsCorrectFormat()
    {
        MagicHeader range = 25ul;

        Assert.Equal("25", range.ToString());
    }

    [Fact]
    public void ToString_SingleValue_FromConstructor_ReturnsCorrectFormat()
    {
        var range = new MagicHeader(10ul);

        Assert.Equal("10", range.ToString());
    }

    [Fact]
    public void RoundTrip_ParseAndToString()
    {
        var original = "100-200";
        var range = MagicHeader.Parse(original);
        var result = range.ToString();

        Assert.Equal(original, result);
    }

    [Fact]
    public void RoundTrip_SingleValue()
    {
        var original = "100";
        var range = MagicHeader.Parse(original);
        var result = range.ToString();

        Assert.Equal(original, result);
    }
}
