namespace WgConf;

public class WireguardPeerConfiguration
{
    public required CIDR[] AllowedIPs { get; set; }
    public Uri? Endpoint { get; set; }

    public required byte[] PublicKey { get; set; }
    public byte[]? PresharedKey { get; set; }

    public int? PersistedKeepalive { get; set; }
}
