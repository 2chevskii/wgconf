# Configuration Model

WgConf exposes two main data types: `WireguardConfiguration` for the `[Interface]` section and `WireguardPeerConfiguration` for `[Peer]` sections.

## WireguardConfiguration (Interface)

Properties are read from the `[Interface]` section and written back in the same section. The parser requires `PrivateKey`, `ListenPort`, and `Address` to be present.

| Property | Type | Required | Notes |
| --- | --- | --- | --- |
| `PrivateKey` | `byte[]` | Yes | Must decode to 32 bytes (Base64 in files). |
| `ListenPort` | `ushort` | Yes | Must be in the range 1-65535. |
| `Address` | `CIDR` | Yes | CIDR notation, IPv4 or IPv6. |
| `PreUp` | `string?` | No | Command executed before the interface is brought up. |
| `PostUp` | `string?` | No | Command executed after the interface is brought up. |
| `PreDown` | `string?` | No | Command executed before the interface is brought down. |
| `PostDown` | `string?` | No | Command executed after the interface is brought down. |
| `Peers` | `List<WireguardPeerConfiguration>` | No | Collection of peers. |

## WireguardPeerConfiguration (Peer)

Each `[Peer]` section maps to one `WireguardPeerConfiguration` instance.

| Property | Type | Required | Notes |
| --- | --- | --- | --- |
| `PublicKey` | `byte[]` | Yes | Must decode to 32 bytes (Base64 in files). |
| `AllowedIPs` | `List<CIDR>` | Yes | Comma separated list in files. |
| `Endpoint` | `WireguardEndpoint?` | No | `host:port` or `[ipv6]:port`. |
| `PresharedKey` | `byte[]?` | No | Must decode to 32 bytes if set. |
| `PersistedKeepalive` | `int?` | No | Parsed from `PersistedKeepalive`. |

### Keepalive spelling

The supported property name is `PersistedKeepalive` (note the spelling). The reader and writer both use this name, and `PersistentKeepalive` is not recognized.

## Implicit conversions

Some value types allow implicit conversions from strings:

```csharp
var config = new WireguardConfiguration
{
    PrivateKey = Convert.FromBase64String("..."),
    ListenPort = 51820,
    Address = "10.0.0.1/24",
};

var peer = new WireguardPeerConfiguration
{
    PublicKey = Convert.FromBase64String("..."),
    AllowedIPs = { "10.0.0.2/32" },
    Endpoint = "vpn.example.com:51820",
};
```

These use implicit conversions defined by `CIDR` and `WireguardEndpoint`.
