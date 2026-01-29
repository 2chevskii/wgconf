# WgConf

[![CI/CD](https://github.com/2chevskii/wgconf/actions/workflows/ci.yml/badge.svg)](https://github.com/2chevskii/wgconf/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/WgConf.svg)](https://www.nuget.org/packages/WgConf/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/WgConf.svg)](https://www.nuget.org/packages/WgConf/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET library for reading and writing WireGuard configuration files with strongly typed models, strict validation, and detailed error reporting. The WgConf.Amnezia package extends the core model with AmneziaWG obfuscation parameters while keeping the same API shape.

## Packages

- `WgConf` - Core WireGuard configuration reader and writer.
- `WgConf.Amnezia` - Extension package that adds AmneziaWG properties to the `[Interface]` section.

## Features

- Strongly typed models for `[Interface]` and `[Peer]` sections.
- Reader and writer classes with sync and async APIs.
- Validation that collects multiple errors at once.
- Case insensitive property parsing and `#` comment stripping.
- Value types for CIDR addresses and WireGuard endpoints.
- Optional AmneziaWG extension with header range parsing.

## Installation

```bash
dotnet add package WgConf
```

For AmneziaWG support:

```bash
dotnet add package WgConf.Amnezia
```

## Quick Start

### Read a configuration

```csharp
using WgConf;

var config = WireguardConfiguration.Parse(text);
```

```csharp
using WgConf;

var config = WireguardConfiguration.Load("wg0.conf");
```

```csharp
using WgConf;

var config = await WireguardConfiguration.LoadAsync("wg0.conf");
```

### Write a configuration

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
    Endpoint = WireguardEndpoint.Parse("vpn.example.com:51820"),
    PersistedKeepalive = 25,
});

config.Save("wg0.conf");
```

### AmneziaWG

```csharp
using WgConf.Amnezia;

var config = new AmneziaWgConfiguration
{
    PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
    ListenPort = 51820,
    Address = "10.0.0.1/24",

    Jc = 5,
    Jmin = 20,
    Jmax = 1000,
    I1 = "custom",
    H1 = "25-30",
};

config.Save("awg0.conf");
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

### AmneziaWgConfiguration

Adds optional obfuscation parameters in `[Interface]`:

- Integers: `Jc`, `Jmin`, `Jmax`, `S1`, `S2`, `S3`, `S4`, `J1`, `J2`, `J3`, `Itime`
- Strings: `I1`, `I2`, `I3`, `I4`, `I5`
- Header ranges: `H1`, `H2`, `H3`, `H4` (single value like `25` or range like `25-30`)

## Error Handling

`Read()` and `Parse()` throw `WireguardConfigurationException` when errors are present. Use `TryParse` or `TryRead` to collect all errors without throwing.

```csharp
if (!WireguardConfiguration.TryParse(text, out var config, out var errors))
{
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
```

## Documentation

The documentation site is authored in the `docs` folder (Vitepress v2).

```bash
cd docs
npm run dev
```

## Testing

```bash
dotnet test
```

Coverage reports (OpenCover XML) are produced under `TestResults/<ProjectName>/coverage.opencover.xml`.

## Requirements

- .NET 8.0 or later

## License

MIT License - see [LICENSE](LICENSE) for details.

## Related Projects

- [WireGuard](https://www.wireguard.com/) - Fast, modern, secure VPN tunnel
- [AmneziaWG](https://github.com/amnezia-vpn/amneziawg) - WireGuard with obfuscation support


