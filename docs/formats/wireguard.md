# WireGuard Format

WgConf reads and writes WireGuard configuration files using a strict INI-like format.

## Sections

- `[Interface]` (exactly one)
- `[Peer]` (zero or more)

## Required properties

These properties must be present for the parser to succeed:

- `[Interface]`: `PrivateKey`, `ListenPort`, `Address`
- `[Peer]`: `PublicKey`, `AllowedIPs`

## Supported properties

### [Interface]

- `PrivateKey`
- `ListenPort`
- `Address`
- `PreUp`
- `PostUp`
- `PreDown`
- `PostDown`

### [Peer]

- `PublicKey`
- `AllowedIPs`
- `Endpoint`
- `PresharedKey`
- `PersistedKeepalive`

## Formatting rules

- Property names are case insensitive.
- `Key = Value` format is required.
- `#` starts a comment (full line or inline).
- Duplicate keys are allowed; the last value wins.
- Unknown sections and properties are reported as errors.

## Example

```ini
[Interface]
PrivateKey = <base64 private key>
ListenPort = 51820
Address = 10.0.0.1/24
PostUp = iptables -A FORWARD -i wg0 -j ACCEPT

[Peer]
PublicKey = <base64 public key>
AllowedIPs = 10.0.0.2/32, 10.0.0.3/32
Endpoint = vpn.example.com:51820
PersistedKeepalive = 25
```
