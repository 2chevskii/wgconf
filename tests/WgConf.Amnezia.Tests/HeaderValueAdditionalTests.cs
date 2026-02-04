using WgConf.Amnezia;

namespace WgConf.Amnezia.Tests;

public class HeaderValueAdditionalTests
{
    [Fact]
    public void Create_FromRange_ReturnsExpectedValues()
    {
        MagicHeader value = MagicHeader.Create([10ul, 20ul]);

        Assert.Equal(10ul, value.Start);
        Assert.Equal(20ul, value.End);
    }

    [Fact]
    public void ImplicitOperator_FromTuple_ReturnsExpectedValues()
    {
        MagicHeader value = (5ul, 15ul);

        Assert.Equal(5ul, value.Start);
        Assert.Equal(15ul, value.End);
    }

    [Fact]
    public void Parse_WithMultipleDashes_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => MagicHeader.Parse("1-2-3"));
    }

    [Fact]
    public void GetEnumerator_ReturnsRangeValues()
    {
        MagicHeader value = new MagicHeader(3ul, 4ul);
        var items = new List<ulong>();

        foreach (var item in value)
        {
            items.Add(item);
        }

        Assert.Equal(new[] { 3ul, 4ul }, items);
    }

    [Fact]
    public void GetEnumerator_ReturnsSingleValue()
    {
        MagicHeader value = new MagicHeader(7ul);
        var items = new List<ulong>();

        foreach (var item in value)
        {
            items.Add(item);
        }

        Assert.Equal(new[] { 7ul }, items);
    }
}
