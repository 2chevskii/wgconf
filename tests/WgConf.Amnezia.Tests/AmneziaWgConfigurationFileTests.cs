using WgConf.Amnezia;

namespace WgConf.Amnezia.Tests;

public class AmneziaWgConfigurationFileTests
{
    private const string ConfigText = """
        [Interface]
        PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
        ListenPort = 51820
        Address = 10.0.0.1/24
        Jc = 5

        [Peer]
        PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
        AllowedIPs = 10.0.0.2/32
        """;

    [Fact]
    public void Parse_MinimalConfig_ReturnsConfiguration()
    {
        var config = AmneziaWgConfiguration.Parse(ConfigText);

        Assert.NotNull(config);
        Assert.Equal(51820, config.ListenPort);
        Assert.Equal(5, config.Jc);
    }

    [Fact]
    public void TryParse_ValidConfig_ReturnsTrue()
    {
        var success = AmneziaWgConfiguration.TryParse(ConfigText, out var config, out var errors);

        Assert.True(success);
        Assert.NotNull(config);
        Assert.Empty(errors);
    }

    [Fact]
    public void Load_ValidFile_ReturnsConfiguration()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, ConfigText);

            var config = AmneziaWgConfiguration.Load(tempFile);

            Assert.NotNull(config);
            Assert.Equal(5, config.Jc);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsync_ValidFile_ReturnsConfiguration()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(
                tempFile,
                ConfigText,
                TestContext.Current.CancellationToken
            );

            var config = await AmneziaWgConfiguration.LoadAsync(
                tempFile,
                TestContext.Current.CancellationToken
            );

            Assert.NotNull(config);
            Assert.Equal(5, config.Jc);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Save_WritesFile()
    {
        var config = AmneziaWgConfiguration.Parse(ConfigText);
        var tempFile = Path.GetTempFileName();
        try
        {
            config.Save(tempFile);

            var text = File.ReadAllText(tempFile);
            Assert.Contains("Jc = 5", text);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveAsync_WritesFile()
    {
        var config = AmneziaWgConfiguration.Parse(ConfigText);
        var tempFile = Path.GetTempFileName();
        try
        {
            await config.SaveAsync(tempFile, TestContext.Current.CancellationToken);

            var text = await File.ReadAllTextAsync(tempFile, TestContext.Current.CancellationToken);
            Assert.Contains("Jc = 5", text);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
