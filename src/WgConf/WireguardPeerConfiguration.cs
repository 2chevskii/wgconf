namespace WgConf;

/// <summary>
/// Represents a WireGuard peer configuration.
/// </summary>
public sealed class WireguardPeerConfiguration
{
    /// <summary>
    /// Gets or sets the list of allowed IP ranges for the peer.
    /// </summary>
    public List<CIDR> AllowedIPs { get; set; } = [];

    /// <summary>
    /// Gets or sets the endpoint for the peer.
    /// </summary>
    public WireguardEndpoint? Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the public key for the peer.
    /// </summary>
    public required byte[] PublicKey { get; set; }

    /// <summary>
    /// Gets or sets the optional preshared key for the peer.
    /// </summary>
    public byte[]? PresharedKey { get; set; }

    /// <summary>
    /// Gets or sets the optional persistent keepalive interval in seconds.
    /// </summary>
    public int? PersistedKeepalive { get; set; }
}
