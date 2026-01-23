# WgConf

A .NET library for reading and writing WireGuard configuration files with strongly-typed models and comprehensive error handling.

## Features

- **Strongly-typed models** for WireGuard configurations
- **Reader/Writer pattern** with comprehensive validation
- **Value types** for CIDR notation and WireGuard endpoints
- **Dual API** - Both throwing (`Read()`) and non-throwing (`TryRead()`) methods
- **Async/await support** for all I/O operations
- **Case-insensitive** property parsing
- **Comment support** - Strips both full-line and inline `#` comments

## Installation

```bash
dotnet add package WgConf
```

## Quick Start

### Reading a Configuration

```csharp
using WgConf;

// Parse from string
var config = WireguardConfiguration.Parse(configText);

// Load from file
var config = WireguardConfiguration.Load("wg0.conf");

// With error handling
if (WireguardConfiguration.TryParse(configText, out var config, out var errors))
{
    Console.WriteLine($"Loaded {config.Peers.Count} peers");
}
else
{
    foreach (var error in errors)
        Console.WriteLine($"Line {error.LineNumber}: {error.Message}");
}
```

### Writing a Configuration

```csharp
var config = new WireguardConfiguration
{
    PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
    ListenPort = 51820,
    Address = CIDR.Parse("10.0.0.1/24"),
};

config.Peers.Add(new WireguardPeerConfiguration
{
    PublicKey = Convert.FromBase64String("xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg="),
    AllowedIPs = new[] { CIDR.Parse("10.0.0.2/32") },
    Endpoint = WireguardEndpoint.Parse("example.com:51820"),
});

config.Save("wg0.conf");
```

## Configuration Model

### WireguardConfiguration (Interface Section)

- `PrivateKey` - Base64-encoded private key (32 bytes, required)
- `ListenPort` - UDP port (1-65535, optional)
- `Address` - Interface IP address and subnet (CIDR, optional)
- `PreUp`, `PostUp`, `PreDown`, `PostDown` - Hook commands (optional)
- `Peers` - Collection of peer configurations

### WireguardPeerConfiguration (Peer Section)

- `PublicKey` - Base64-encoded public key (32 bytes, required)
- `AllowedIPs` - Array of allowed IP ranges (CIDR[], required)
- `Endpoint` - Peer endpoint in `host:port` format (optional)
- `PresharedKey` - Base64-encoded pre-shared key for additional security (optional)
- `PersistentKeepalive` - Keepalive interval in seconds (optional)

## Value Types

### CIDR

Represents an IP address with prefix length (e.g., `10.0.0.1/24`):

```csharp
var cidr = CIDR.Parse("10.0.0.1/24");
Console.WriteLine(cidr.ToString()); // 10.0.0.1/24
```

### WireguardEndpoint

Represents a WireGuard endpoint in `host:port` format with IPv6 support:

```csharp
var endpoint = WireguardEndpoint.Parse("example.com:51820");
var ipv6 = WireguardEndpoint.Parse("[::1]:51820");
```

## Error Handling

The library provides detailed error information with line numbers and context:

```csharp
if (!WireguardConfiguration.TryParse(text, out var config, out var errors))
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Line {error.LineNumber}: {error.Message}");
        Console.WriteLine($"  Property: {error.PropertyName}");
        Console.WriteLine($"  Section: {error.Section}");
        Console.WriteLine($"  Context: {error.LineText}");
    }
}
```

## Requirements

- .NET 8.0 or later

## Related Packages

- **WgConf.Amnezia** - Extension package for AmneziaWG obfuscation parameters

## License

MIT License - see [LICENSE](https://github.com/dvchevskii/wgconf/blob/master/LICENSE) for details.

## Links

- [GitHub Repository](https://github.com/dvchevskii/wgconf)
- [Documentation](https://github.com/dvchevskii/wgconf#readme)
- [WireGuard Official Site](https://www.wireguard.com/)
