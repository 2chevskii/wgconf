namespace WgConf;

public class WireguardConfiguration
{
    public required byte[] PrivateKey { get; set; }
    public required int ListenPort { get; set; }
    public required CIDR Address { get; set; }

    public string? PreUp { get; set; }
    public string? PostUp { get; set; }
    public string? PreDown { get; set; }
    public string? PostDown { get; set; }

    public List<WireguardPeerConfiguration> Peers { get; } = [];
}
