using System.Text;

namespace WgConf.Amnezia.Tests;

public class WireguardConfigurationWriterCoverageTests
{
    [Fact]
    public async Task Write_WithOptionalProperties_FormatsOutput()
    {
        var config = new WireguardConfiguration
        {
            PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
            ListenPort = 51820,
            Address = CIDR.Parse("10.0.0.1/24"),
            PreUp = "pre-up",
            PostUp = "post-up",
            PreDown = "pre-down",
            PostDown = "post-down",
        };
        config.Peers.Add(
            new WireguardPeerConfiguration
            {
                PublicKey = Convert.FromBase64String(
                    "xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg="
                ),
                AllowedIPs = [CIDR.Parse("10.0.0.2/32"), CIDR.Parse("10.0.0.3/32")],
                PresharedKey = Convert.FromBase64String(
                    "FpCyhws9cxwWoV4xELtfJvjJN+zQVRPISllRWgeopVE="
                ),
                Endpoint = WireguardEndpoint.Parse("example.com:51820"),
                PersistedKeepalive = 25,
            }
        );

        var builder = new StringBuilder();
        var writer = new WireguardConfigurationWriter(new StringWriter(builder));
        writer.Write(config);
        await writer.DisposeAsync();

        var output = builder.ToString();
        Assert.Contains("PreUp = pre-up", output);
        Assert.Contains("PostUp = post-up", output);
        Assert.Contains("PreDown = pre-down", output);
        Assert.Contains("PostDown = post-down", output);
        Assert.Contains("AllowedIPs = 10.0.0.2/32, 10.0.0.3/32", output);
        Assert.Contains("PresharedKey = FpCyhws9cxwWoV4xELtfJvjJN+zQVRPISllRWgeopVE=", output);
        Assert.Contains("Endpoint = example.com:51820", output);
        Assert.Contains("PersistedKeepalive = 25", output);
    }
}
