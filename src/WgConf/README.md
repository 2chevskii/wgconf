# WgConf

A .NET library for reading and writing WireGuard configuration files with strongly typed models and comprehensive error handling.

## Features

- Strongly typed models for `[Interface]` and `[Peer]` sections
- Reader/writer pattern with validation and error accumulation
- Value types for CIDR notation and WireGuard endpoints
- Sync and async APIs for reading and writing
- Case insensitive property parsing and `#` comment stripping

## Installation

```bash
dotnet add package WgConf
```

## Quick Start

### Reading a configuration

```csharp
using WgConf;

var config = WireguardConfiguration.Parse(configText);
```

```csharp
using WgConf;

var config = WireguardConfiguration.Load("wg0.conf");
```

```csharp
using WgConf;

if (WireguardConfiguration.TryParse(configText, out var config, out var errors))
{
    Console.WriteLine($"Loaded {config.Peers.Count} peers");
}
else
{
    foreach (var error in errors)
        Console.WriteLine(error);
}
```

### Writing a configuration

```csharp
using WgConf;

var config = new WireguardConfiguration
{
    PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
    ListenPort = 51820,
    Address = CIDR.Parse("10.0.0.1/24"),
};

config.Peers.Add(new WireguardPeerConfiguration
{
    PublicKey = Convert.FromBase64String("xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg="),
    AllowedIPs = [CIDR.Parse("10.0.0.2/32")],
    Endpoint = WireguardEndpoint.Parse("example.com:51820"),
    PersistedKeepalive = 25,
});

config.Save("wg0.conf");
```

## Configuration Model

### WireguardConfiguration (Interface)

- `PrivateKey` - Base64 encoded private key (32 bytes, required)
- `ListenPort` - UDP port (1-65535, required)
- `Address` - Interface IP address and subnet (CIDR, required)
- `PreUp`, `PostUp`, `PreDown`, `PostDown` - Hook commands (optional)
- `Peers` - Collection of peer configurations

### WireguardPeerConfiguration (Peer)

- `PublicKey` - Base64 encoded public key (32 bytes, required)
- `AllowedIPs` - Array of allowed IP ranges (CIDR[], required)
- `Endpoint` - Optional peer endpoint (`host:port` or `[ipv6]:port`)
- `PresharedKey` - Optional preshared key (Base64, 32 bytes)
- `PersistedKeepalive` - Optional keepalive interval in seconds

## Value Types

### CIDR

Represents an IP address with prefix length (for example `10.0.0.1/24`).

```csharp
var cidr = CIDR.Parse("10.0.0.1/24");
Console.WriteLine(cidr.ToString());
```

### WireguardEndpoint

Represents a WireGuard endpoint in `host:port` format with IPv6 support.

```csharp
var endpoint = WireguardEndpoint.Parse("example.com:51820");
var ipv6 = WireguardEndpoint.Parse("[::1]:51820");
```

## Requirements

- .NET 8.0 or later

## Related Packages

- `WgConf.Amnezia` - Extension package for AmneziaWG obfuscation parameters

## License

MIT License - see [LICENSE](https://github.com/dvchevskii/wgconf/blob/master/LICENSE) for details.

## Links

- [GitHub Repository](https://github.com/dvchevskii/wgconf)
- [WireGuard Official Site](https://www.wireguard.com/)


