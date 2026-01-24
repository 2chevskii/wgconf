# WgConf

[![CI/CD](https://github.com/2chevskii/wgconf/actions/workflows/ci.yml/badge.svg)](https://github.com/2chevskii/wgconf/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/WgConf.svg)](https://www.nuget.org/packages/WgConf/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/WgConf.svg)](https://www.nuget.org/packages/WgConf/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET library for reading and writing **WireGuard** and **AmneziaWG** configuration files with strongly-typed models and comprehensive error handling.

## Features

- **Strongly-typed models** for WireGuard configurations (Interface and Peer sections)
- **Reader/Writer pattern** for parsing and serializing configuration files
- **Comprehensive error handling** with detailed validation and error accumulation
- **Value types** for IP addresses (CIDR), endpoints (WireguardEndpoint)
- **Dual API** - `Read()`/`TryRead()` patterns for both throwing and non-throwing scenarios
- **Case-insensitive** property parsing
- **Comment support** - Strips both full-line and inline comments during parsing
- **Async/await support** - All I/O operations have async counterparts
- **AmneziaWG support** - Optional extension package for AmneziaWG obfuscation parameters

## Installation

### WgConf (Standard WireGuard)

```bash
dotnet add package WgConf
```

### WgConf.Amnezia (AmneziaWG Extension)

```bash
dotnet add package WgConf.Amnezia
```

## Quick Start

### Reading a WireGuard Configuration

```csharp
using WgConf;

// Parse from string
var config = WireguardConfiguration.Parse(configText);

// Load from file
var config = WireguardConfiguration.Load("wg0.conf");

// Or use async
var config = await WireguardConfiguration.LoadAsync("wg0.conf");

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

### Writing a WireGuard Configuration

```csharp
using WgConf;

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

// Save to file
config.Save("wg0.conf");

// Or use async
await config.SaveAsync("wg0.conf");
```

### Working with AmneziaWG

```csharp
using WgConf.Amnezia;

var config = new AmneziaWgConfiguration
{
    PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
    ListenPort = 51820,
    Address = CIDR.Parse("10.0.0.1/24"),

    // AmneziaWG-specific obfuscation parameters
    Jc = 5,
    Jmin = 20,
    Jmax = 1000,
    S1 = 10,
    I1 = "custom_value",
    H1 = IntegerRange.Parse("25-30"),
};

config.Save("awg0.conf");
```

## Configuration Model

### WireguardConfiguration (Interface)

- `PrivateKey` - Base64-encoded private key (32 bytes)
- `ListenPort` - UDP port (1-65535)
- `Address` - Interface IP address and subnet (CIDR)
- `PreUp`, `PostUp`, `PreDown`, `PostDown` - Hook commands
- `Peers` - Collection of peer configurations

### WireguardPeerConfiguration

- `PublicKey` - Base64-encoded public key (32 bytes)
- `AllowedIPs` - Array of allowed IP ranges (CIDR[])
- `Endpoint` - Optional peer endpoint (host:port)
- `PresharedKey` - Optional pre-shared key for additional security
- `PersistentKeepalive` - Optional keepalive interval in seconds

### AmneziaWgConfiguration (Extends WireguardConfiguration)

Adds 20 optional obfuscation parameters:

- **Integers** (11): Jc, Jmin, Jmax, S1, S2, S3, S4, J1, J2, J3, Itime
- **Strings** (5): I1, I2, I3, I4, I5
- **Integer Ranges** (4): H1, H2, H3, H4 (format: "start-end")

## Testing

The library includes comprehensive test coverage (117 tests):

- WgConf.Tests: 81 tests
- WgConf.Amnezia.Tests: 36 tests

```bash
dotnet test
```

## Requirements

- .NET 8.0 or later

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Related Projects

- [WireGuard](https://www.wireguard.com/) - Fast, modern, secure VPN tunnel
- [AmneziaWG](https://github.com/amnezia-vpn/amneziawg) - WireGuard with obfuscation support
