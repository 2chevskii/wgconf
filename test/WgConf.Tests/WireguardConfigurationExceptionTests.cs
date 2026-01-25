namespace WgConf.Tests;

public class WireguardConfigurationExceptionTests
{
    [Fact]
    public void BuildMessage_NoErrors_UsesDefaultMessage()
    {
        var exception = new WireguardConfigurationException(Array.Empty<ParseError>());

        Assert.Equal("Configuration parsing failed", exception.Message);
    }

    [Fact]
    public void BuildMessage_MultipleErrors_FormatsEachError()
    {
        var errors = new[]
        {
            new ParseError(1, "First error"),
            new ParseError(2, "Second error"),
        };

        var exception = new WireguardConfigurationException(errors);

        Assert.Contains("2 errors", exception.Message);
        Assert.Contains("Error 1", exception.Message);
        Assert.Contains("Error 2", exception.Message);
    }
}
