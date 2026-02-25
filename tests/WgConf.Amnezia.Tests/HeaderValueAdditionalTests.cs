using FluentAssertions;

namespace WgConf.Amnezia.Tests;

public class HeaderValueAdditionalTests
{
    [Fact]
    public void Create_FromRange_ReturnsExpectedValues()
    {
        MagicHeader value = MagicHeader.Create([10u, 20u]);

        Assert.Equal(10u, value.Start);
        Assert.Equal(20u, value.End);
    }

    [Fact]
    public void ImplicitOperator_FromTuple_ReturnsExpectedValues()
    {
        MagicHeader value = (5u, 15u);

        Assert.Equal(5u, value.Start);
        Assert.Equal(15u, value.End);
    }

    [Fact]
    public void Parse_WithMultipleDashes_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => MagicHeader.Parse("1-2-3"));
    }

    [Fact]
    public void GetEnumerator_ReturnsRangeValues()
    {
        MagicHeader magicHeader = new MagicHeader(3u, 4u);
        var items = new List<ulong>();

        foreach (var item in magicHeader)
        {
            items.Add(item);
        }

        items.Should().BeEquivalentTo([3u, 4u]);
    }

    [Fact]
    public void GetEnumerator_ReturnsSingleValue()
    {
        MagicHeader magicHeader = new MagicHeader(7u);
        var items = new List<ulong>();

        foreach (var item in magicHeader)
        {
            items.Add(item);
        }

        items.Should().BeEquivalentTo([7u]);
    }
}
