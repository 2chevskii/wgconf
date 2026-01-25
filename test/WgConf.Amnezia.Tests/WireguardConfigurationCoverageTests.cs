namespace WgConf.Amnezia.Tests;

public class WireguardConfigurationCoverageTests
{
    private const string ConfigText = """
        [Interface]
        PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
        ListenPort = 51820
        Address = 10.0.0.1/24

        [Peer]
        PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
        AllowedIPs = 10.0.0.2/32
        """;

    [Fact]
    public void Parse_And_TryParse_Work()
    {
        var parsed = WireguardConfiguration.Parse(ConfigText);
        var success = WireguardConfiguration.TryParse(ConfigText, out var config, out var errors);

        Assert.NotNull(parsed);
        Assert.True(success);
        Assert.NotNull(config);
        Assert.Empty(errors);
    }

    [Fact]
    public void Load_And_Save_Work()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, ConfigText);
            var loaded = WireguardConfiguration.Load(tempFile);

            loaded.Save(tempFile);
            var reloaded = WireguardConfiguration.Load(tempFile);

            Assert.Equal(loaded.ListenPort, reloaded.ListenPort);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsync_And_SaveAsync_Work()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, ConfigText);
            var loaded = await WireguardConfiguration.LoadAsync(tempFile);

            await loaded.SaveAsync(tempFile);
            var reloaded = await WireguardConfiguration.LoadAsync(tempFile);

            Assert.Equal(loaded.ListenPort, reloaded.ListenPort);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
