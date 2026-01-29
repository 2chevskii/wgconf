# WgConf.Amnezia

Extension package for [WgConf](https://www.nuget.org/packages/WgConf/) adding support for AmneziaWG-specific obfuscation parameters.

## About AmneziaWG

AmneziaWG is a WireGuard fork with additional obfuscation capabilities designed to bypass DPI (Deep Packet Inspection) systems. This package extends WgConf to support the 20 additional configuration parameters used by AmneziaWG.

## Features

- Extends WgConf and keeps the same API shape
- 20 obfuscation parameters in the `[Interface]` section
- Type safe models for integer, string, and header range properties
- Same `Read()`/`TryRead()` and `Save()`/`SaveAsync()` patterns
- Backward compatible with standard WireGuard configs

## Installation

```bash
dotnet add package WgConf.Amnezia
```

This package includes the base `WgConf` package as a dependency.

## Quick Start

### Reading an AmneziaWG configuration

```csharp
using WgConf.Amnezia;

var config = AmneziaWgConfiguration.Parse(configText);

if (config.Jc.HasValue)
    Console.WriteLine($"Junk packet count: {config.Jc}");
if (config.H1.HasValue)
    Console.WriteLine($"H1 range: {config.H1.Value}");
```

### Writing an AmneziaWG configuration

```csharp
using WgConf.Amnezia;

var config = new AmneziaWgConfiguration
{
    PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
    ListenPort = 51820,
    Address = CIDR.Parse("10.0.0.1/24"),

    // AmneziaWG obfuscation parameters (all optional)
    Jc = 5,
    Jmin = 20,
    Jmax = 1000,
    S1 = 10,
    I1 = "custom",
    H1 = "25-30",
};

config.Peers.Add(new WireguardPeerConfiguration
{
    PublicKey = Convert.FromBase64String("xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg="),
    AllowedIPs = [CIDR.Parse("10.0.0.2/32")],
});

config.Save("awg0.conf");
```

## AmneziaWG Parameters

All parameters are optional and appear in the `[Interface]` section only.

### Integer parameters (11)

- `Jc`, `Jmin`, `Jmax`
- `S1`, `S2`, `S3`, `S4`
- `J1`, `J2`, `J3`
- `Itime`

### String parameters (5)

- `I1`, `I2`, `I3`, `I4`, `I5`

### Header parameters (4)

- `H1`, `H2`, `H3`, `H4`

Header parameters use the `HeaderValue` type and accept either a single unsigned integer (`25`) or a range (`25-30`).

## HeaderValue value type

`HeaderValue` represents a single header value or a range in the format `start-end`:

```csharp
var single = new HeaderValue(25ul);
var range = new HeaderValue(25ul, 30ul);

HeaderValue fromString = "25-30";
HeaderValue fromTuple = (100ul, 200ul);
```

## Error Handling

Same as WgConf: `TryParse`, `TryRead`, and `TryReadAsync` return a list of `ParseError` entries without throwing.

## Requirements

- .NET 8.0 or later
- WgConf (automatically included)

## License

MIT License - see [LICENSE](https://github.com/dvchevskii/wgconf/blob/master/LICENSE) for details.

## Links

- [GitHub Repository](https://github.com/dvchevskii/wgconf)
- [WgConf Package](https://www.nuget.org/packages/WgConf/)
- [AmneziaWG Project](https://github.com/amnezia-vpn/amneziawg)

