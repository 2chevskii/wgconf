namespace WgConf;

public sealed class WireguardPeerConfiguration
{
    public List<CIDR> AllowedIPs { get; set; } = [];
    public WireguardEndpoint? Endpoint { get; set; }

    public required byte[] PublicKey { get; set; }
    public byte[]? PresharedKey { get; set; }

    public int? PersistedKeepalive { get; set; }
}
