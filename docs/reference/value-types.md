# Value Types

## CIDR

Represents an IP address with a prefix length.

```csharp
var cidr = CIDR.Parse("10.0.0.1/24");
Console.WriteLine(cidr.Address);      // 10.0.0.1
Console.WriteLine(cidr.PrefixLength); // 24
```

Parsing rules:

- Format is `address/prefix`.
- Address can be IPv4 or IPv6.
- Prefix length must be a non-negative integer and within the address family range (0-32 for IPv4, 0-128 for IPv6).

### APIs

- `CIDR.Parse(ReadOnlySpan<char> input)`
- `CIDR.TryParse(ReadOnlySpan<char> input, out CIDR result, out Exception? exception)`
- Implicit conversion from `ReadOnlySpan<char>` (for example, from a `string`).

## WireguardEndpoint

Represents a peer endpoint in `host:port` format.

```csharp
var endpoint = WireguardEndpoint.Parse("vpn.example.com:51820");
var ipv6 = WireguardEndpoint.Parse("[::1]:51820");
```

Parsing rules:

- `host:port` for hostnames and IPv4.
- `[ipv6]:port` for IPv6 addresses.
- Port must be between 1 and 65535.

### APIs

- `WireguardEndpoint.Parse(ReadOnlySpan<char> input)`
- `WireguardEndpoint.TryParse(ReadOnlySpan<char> input, out WireguardEndpoint result, out Exception? exception)`
- Implicit conversion from `ReadOnlySpan<char>` (for example, from a `string`).

## HeaderValue (WgConf.Amnezia)

Represents a single unsigned integer or a range used by AmneziaWG header parameters `H1`-`H4`.

```csharp
var single = new HeaderValue(25ul);
var range = new HeaderValue(25ul, 30ul);

HeaderValue fromString = "25-30";
HeaderValue fromTuple = (100ul, 200ul);
```

Parsing rules:

- Single values are written as `25`.
- Ranges are written as `25-30`.
- The end value must be greater than the start value.

### APIs

- `HeaderValue.Parse(ReadOnlySpan<char> input)`
- `HeaderValue.TryParse(ReadOnlySpan<char> input, out HeaderValue headerValue)`
- Implicit conversions from `string`, `ulong`, and `(ulong, ulong)`.
