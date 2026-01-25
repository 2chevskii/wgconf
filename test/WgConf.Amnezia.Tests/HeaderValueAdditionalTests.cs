using WgConf.Amnezia;

namespace WgConf.Amnezia.Tests;

public class HeaderValueAdditionalTests
{
    [Fact]
    public void Create_FromRange_ReturnsExpectedValues()
    {
        HeaderValue value = HeaderValue.Create([10ul, 20ul]);

        Assert.Equal(10ul, value.Start);
        Assert.Equal(20ul, value.End);
    }

    [Fact]
    public void ImplicitOperator_FromTuple_ReturnsExpectedValues()
    {
        HeaderValue value = (5ul, 15ul);

        Assert.Equal(5ul, value.Start);
        Assert.Equal(15ul, value.End);
    }

    [Fact]
    public void Parse_WithMultipleDashes_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => HeaderValue.Parse("1-2-3"));
    }

    [Fact]
    public void GetEnumerator_ReturnsRangeValues()
    {
        HeaderValue value = new HeaderValue(3ul, 4ul);
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
        HeaderValue value = new HeaderValue(7ul);
        var items = new List<ulong>();

        foreach (var item in value)
        {
            items.Add(item);
        }

        Assert.Equal(new[] { 7ul }, items);
    }
}
