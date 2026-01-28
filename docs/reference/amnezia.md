# AmneziaWG Extension

`WgConf.Amnezia` extends the core model with AmneziaWG-specific obfuscation parameters while keeping the same reader and writer APIs.

## AmneziaWgConfiguration

`AmneziaWgConfiguration` inherits from `WireguardConfiguration` and adds optional properties that live in the `[Interface]` section.

### Integer properties (`int?`)

- `Jc`, `Jmin`, `Jmax`
- `S1`, `S2`, `S3`, `S4`
- `J1`, `J2`, `J3`
- `Itime`

### String properties (`string?`)

- `I1`, `I2`, `I3`, `I4`, `I5`

### Header properties (`HeaderValue?`)

- `H1`, `H2`, `H3`, `H4`

`HeaderValue` accepts either a single unsigned integer (`25`) or a range (`25-30`).

## Reading and writing

`AmneziaWgConfiguration` exposes the same static helpers as the base type:

- `Parse(string text)`
- `TryParse(string text, out AmneziaWgConfiguration? configuration, out IReadOnlyList<ParseError> errors)`
- `Load(string path)`
- `LoadAsync(string path, CancellationToken cancellationToken = default)`
- `Save(string path)`
- `SaveAsync(string path, CancellationToken cancellationToken = default)`

The reader is strict about invalid values: any malformed integer or header value is reported as a `ParseError` and causes `TryParse` and `TryRead` to return `false`.

## Example

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

## Output ordering

AmneziaWG properties are written after the standard WireGuard `[Interface]` properties.
