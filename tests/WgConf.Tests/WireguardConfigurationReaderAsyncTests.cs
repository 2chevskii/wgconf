namespace WgConf.Tests;

public class WireguardConfigurationReaderAsyncTests
{
    [Fact]
    public async Task ReadAsync_ValidConfiguration_ReturnsConfiguration()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = 10.0.0.2/32
            """;

        await using var reader = new WireguardConfigurationReader(new StringReader(input));
        var config = await reader.ReadAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(config);
        Assert.Equal(51820, config.ListenPort);
        Assert.Single(config.Peers);
    }

    [Fact]
    public async Task ReadAsync_InvalidConfiguration_ThrowsException()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            InvalidProperty = Value
            """;

        await using var reader = new WireguardConfigurationReader(new StringReader(input));

        await Assert.ThrowsAsync<WireguardConfigurationException>(async () =>
            await reader.ReadAsync(TestContext.Current.CancellationToken)
        );
    }

    [Fact]
    public async Task TryReadAsync_ValidConfiguration_ReturnsConfiguration()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        await using var reader = new WireguardConfigurationReader(new StringReader(input));
        var (config, errors) = await reader.TryReadAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(config);
        Assert.Empty(errors);
        Assert.Equal(51820, config.ListenPort);
    }

    [Fact]
    public async Task TryReadAsync_InvalidConfiguration_ReturnsNullWithErrors()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            InvalidProperty = Value
            """;

        await using var reader = new WireguardConfigurationReader(new StringReader(input));
        var (config, errors) = await reader.TryReadAsync(TestContext.Current.CancellationToken);

        Assert.Null(config);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public async Task DisposeAsync_CalledMultipleTimes_DoesNotThrow()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        var reader = new WireguardConfigurationReader(new StringReader(input));
        await reader.DisposeAsync();
        await reader.DisposeAsync(); // Should not throw
    }

    [Fact]
    public async Task ReadAsync_WithCancellationToken_RespectsCancellation()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        await using var reader = new WireguardConfigurationReader(new StringReader(input));
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await reader.ReadAsync(cts.Token)
        );
    }

    [Fact]
    public async Task TryReadAsync_WithCancellationToken_RespectsCancellation()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        await using var reader = new WireguardConfigurationReader(new StringReader(input));
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await reader.TryReadAsync(cts.Token)
        );
    }
}
