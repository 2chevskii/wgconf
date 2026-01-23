using WgConf.Amnezia;
using Xunit;

namespace WgConf.Amnezia.Tests;

public class IntegerRangeTests
{
    [Fact]
    public void Parse_ValidRange_ReturnsCorrectValues()
    {
        var range = IntegerRange.Parse("25-30");

        Assert.Equal(25, range.Start);
        Assert.Equal(30, range.End);
    }

    [Fact]
    public void Parse_SameStartEnd_ReturnsCorrectValues()
    {
        var range = IntegerRange.Parse("5-5");

        Assert.Equal(5, range.Start);
        Assert.Equal(5, range.End);
    }

    [Fact]
    public void Parse_EndLessThanStart_StillParses()
    {
        var range = IntegerRange.Parse("30-25");

        Assert.Equal(30, range.Start);
        Assert.Equal(25, range.End);
    }

    [Fact]
    public void Parse_NegativeNumbers_Parses()
    {
        var range = IntegerRange.Parse("-10-5");

        Assert.Equal(-10, range.Start);
        Assert.Equal(5, range.End);
    }

    [Fact]
    public void Parse_LargeNumbers_Parses()
    {
        var range = IntegerRange.Parse("1000000-2000000");

        Assert.Equal(1000000, range.Start);
        Assert.Equal(2000000, range.End);
    }

    [Fact]
    public void Parse_WithWhitespace_Parses()
    {
        var range = IntegerRange.Parse(" 10 - 20 ");

        Assert.Equal(10, range.Start);
        Assert.Equal(20, range.End);
    }

    [Fact]
    public void Parse_MissingDash_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => IntegerRange.Parse("1020"));
    }

    [Fact]
    public void Parse_InvalidStartNumber_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => IntegerRange.Parse("abc-20"));
    }

    [Fact]
    public void Parse_InvalidEndNumber_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => IntegerRange.Parse("10-xyz"));
    }

    [Fact]
    public void Parse_EmptyString_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => IntegerRange.Parse(""));
    }

    [Fact]
    public void TryParse_ValidRange_ReturnsTrue()
    {
        var success = IntegerRange.TryParse("25-30", out var range);

        Assert.True(success);
        Assert.Equal(25, range.Start);
        Assert.Equal(30, range.End);
    }

    [Fact]
    public void TryParse_InvalidFormat_ReturnsFalse()
    {
        var success = IntegerRange.TryParse("invalid", out var range);

        Assert.False(success);
        Assert.Equal(default, range);
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        var range = new IntegerRange { Start = 25, End = 30 };

        Assert.Equal("25-30", range.ToString());
    }

    [Fact]
    public void ToString_WithNegativeNumbers()
    {
        var range = new IntegerRange { Start = -10, End = 5 };

        Assert.Equal("-10-5", range.ToString());
    }

    [Fact]
    public void RoundTrip_ParseAndToString()
    {
        var original = "100-200";
        var range = IntegerRange.Parse(original);
        var result = range.ToString();

        Assert.Equal(original, result);
    }

    [Fact]
    public void RoundTrip_WithNegative()
    {
        var original = "-50-100";
        var range = IntegerRange.Parse(original);
        var result = range.ToString();

        Assert.Equal(original, result);
    }
}
