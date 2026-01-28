# WgConf

WgConf is a .NET library for reading and writing WireGuard configuration files using strongly typed models, strict validation, and detailed error reporting. The WgConf.Amnezia package extends the core model with AmneziaWG obfuscation parameters while keeping the same API shape.

## Packages

- `WgConf` - Core WireGuard configuration reader and writer.
- `WgConf.Amnezia` - Extension package that adds AmneziaWG properties to the `[Interface]` section.

## Why WgConf

- Strongly typed model for `[Interface]` and `[Peer]` sections.
- Reader and writer classes with sync and async APIs.
- Validation that collects multiple errors at once.
- Case insensitive property parsing and `#` comment stripping.
- Value types for CIDR addresses and WireGuard endpoints.

## Quick Start

Install the core package:

```bash
dotnet add package WgConf
```

Parse a configuration string and save it back to disk:

```csharp
using WgConf;

var config = WireguardConfiguration.Parse(text);
config.Save("wg0.conf");
```

Handle errors without throwing:

```csharp
using WgConf;

if (WireguardConfiguration.TryParse(text, out var config, out var errors))
{
    Console.WriteLine($"Peers: {config.Peers.Count}");
}
else
{
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
```

## Sample WireGuard Configuration

```ini
[Interface]
PrivateKey = <base64 private key>
ListenPort = 51820
Address = 10.0.0.1/24

[Peer]
PublicKey = <base64 public key>
AllowedIPs = 10.0.0.2/32
Endpoint = vpn.example.com:51820
PersistedKeepalive = 25
```

## Next Steps

- Get started quickly in [Getting Started](/guide/getting-started).
- Review the data model in [Configuration Model](/guide/configuration-model).
- Dive into the API details in [Core Types](/reference/core-types).
- See accepted file formats in [WireGuard](/formats/wireguard).
