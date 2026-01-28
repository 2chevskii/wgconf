# Core Types

## WireguardConfiguration

Represents a WireGuard configuration file.

### Properties

- `PrivateKey` (`byte[]`, required)
  - Must be 32 bytes. The parser expects Base64 in files.
- `ListenPort` (`ushort`, required)
  - Must be within 1-65535.
- `Address` (`CIDR`, required)
  - Interface address in CIDR notation.
- `PreUp`, `PostUp`, `PreDown`, `PostDown` (`string?`)
  - Optional command hooks.
- `Peers` (`List<WireguardPeerConfiguration>`)
  - Peer collection.

### Methods

- `Parse(string text)`
- `TryParse(string text, out WireguardConfiguration? configuration, out IReadOnlyList<ParseError> errors)`
- `Load(string path)`
- `LoadAsync(string path, CancellationToken cancellationToken = default)`
- `Save(string path)`
- `SaveAsync(string path, CancellationToken cancellationToken = default)`

## WireguardPeerConfiguration

Represents a `[Peer]` section.

### Properties

- `PublicKey` (`byte[]`, required)
  - Must be 32 bytes.
- `AllowedIPs` (`List<CIDR>`, required)
  - Comma separated in files.
- `Endpoint` (`WireguardEndpoint?`)
  - Optional `host:port` or `[ipv6]:port`.
- `PresharedKey` (`byte[]?`)
  - Optional, must be 32 bytes when set.
- `PersistedKeepalive` (`int?`)
  - Optional, parsed from `PersistedKeepalive`.

## ParseError

`ParseError` is a record that describes a single error. It includes the line number, message, optional section and property names, the original line text, and a small context window when available.

## WireguardConfigurationException

Thrown by `Read()` and `Parse()` when errors are present. The `Errors` property exposes the complete list of `ParseError` values.
