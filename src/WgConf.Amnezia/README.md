# WgConf.Amnezia

Extension package for [WgConf](https://www.nuget.org/packages/WgConf/) adding support for AmneziaWG-specific obfuscation parameters.

## About AmneziaWG

AmneziaWG is a WireGuard fork with additional obfuscation capabilities designed to bypass DPI (Deep Packet Inspection) systems. This package extends WgConf to support the 20 additional configuration parameters used by AmneziaWG.

## Features

- **Extends WgConf** - Inherits all WireGuard configuration capabilities
- **20 obfuscation parameters** - Full support for AmneziaWG-specific settings
- **Type-safe models** - Strongly-typed integer, string, and range properties
- **Same API pattern** - Familiar `Read()`/`TryRead()` and `Save()`/`SaveAsync()` methods
- **Backward compatible** - Can read standard WireGuard configs (AmneziaWG parameters will be `null`)

## Installation

```bash
dotnet add package WgConf.Amnezia
```

This package automatically includes the base `WgConf` package as a dependency.

## Quick Start

### Reading an AmneziaWG Configuration

```csharp
using WgConf.Amnezia;

// Parse from string
var config = AmneziaWgConfiguration.Parse(configText);

// Load from file
var config = AmneziaWgConfiguration.Load("awg0.conf");

// Check which obfuscation parameters are set
if (config.Jc.HasValue)
    Console.WriteLine($"Junk packet count: {config.Jc}");
if (config.H1.HasValue)
    Console.WriteLine($"H1 range: {config.H1.Value}");
```

### Writing an AmneziaWG Configuration

```csharp
using WgConf.Amnezia;

var config = new AmneziaWgConfiguration
{
    // Standard WireGuard properties
    PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
    ListenPort = 51820,
    Address = CIDR.Parse("10.0.0.1/24"),

    // AmneziaWG obfuscation parameters (all optional)
    Jc = 5,          // Junk packet count
    Jmin = 20,       // Minimum junk packet size
    Jmax = 1000,     // Maximum junk packet size
    S1 = 10,         // Init packet junk size
    I1 = "custom",   // Init packet magic header
    H1 = IntegerRange.Parse("25-30"),  // Response packet size range
};

config.Peers.Add(new WireguardPeerConfiguration
{
    PublicKey = Convert.FromBase64String("xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg="),
    AllowedIPs = new[] { CIDR.Parse("10.0.0.2/32") },
});

config.Save("awg0.conf");
```

## AmneziaWG Parameters

All parameters are optional (`int?`, `string?`, `IntegerRange?`) and appear in the `[Interface]` section only.

### Integer Parameters (11)

- `Jc` - Junk packet count
- `Jmin` - Minimum junk packet size
- `Jmax` - Maximum junk packet size
- `S1`, `S2`, `S3`, `S4` - Init packet junk sizes
- `J1`, `J2`, `J3` - Additional junk parameters
- `Itime` - Init time parameter

### String Parameters (5)

- `I1`, `I2`, `I3`, `I4`, `I5` - Init packet magic headers

### Integer Range Parameters (4)

- `H1`, `H2`, `H3`, `H4` - Packet size ranges (format: `"start-end"`)

## IntegerRange Value Type

The `IntegerRange` struct represents integer ranges in the format `"start-end"`:

```csharp
var range = IntegerRange.Parse("25-30");
Console.WriteLine(range.Start); // 25
Console.WriteLine(range.End);   // 30
Console.WriteLine(range.ToString()); // "25-30"

// Supports negative numbers
var negRange = IntegerRange.Parse("-10-5");
```

## Error Handling

Same as WgConf - comprehensive error accumulation with line numbers:

```csharp
if (!AmneziaWgConfiguration.TryParse(text, out var config, out var errors))
{
    foreach (var error in errors)
        Console.WriteLine($"Line {error.LineNumber}: {error.Message}");
}
```

## Round-Trip Compatibility

Configurations are fully round-trippable:

```csharp
// Read -> Write -> Read produces identical results
var original = AmneziaWgConfiguration.Load("awg0.conf");
original.Save("awg0_copy.conf");
var copy = AmneziaWgConfiguration.Load("awg0_copy.conf");
// original and copy have identical values
```

## Requirements

- .NET 8.0 or later
- WgConf (automatically included)

## License

MIT License - see [LICENSE](https://github.com/dvchevskii/wgconf/blob/master/LICENSE) for details.

## Links

- [GitHub Repository](https://github.com/dvchevskii/wgconf)
- [WgConf Package](https://www.nuget.org/packages/WgConf/)
- [AmneziaWG Project](https://github.com/amnezia-vpn/amneziawg)
