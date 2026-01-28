# AmneziaWG Format

`WgConf.Amnezia` extends the WireGuard format by adding properties in the `[Interface]` section. Peers follow the standard WireGuard rules.

## Additional [Interface] properties

### Integers

- `Jc`, `Jmin`, `Jmax`
- `S1`, `S2`, `S3`, `S4`
- `J1`, `J2`, `J3`
- `Itime`

### Strings

- `I1`, `I2`, `I3`, `I4`, `I5`

### Header values

- `H1`, `H2`, `H3`, `H4`

Header values accept either a single unsigned integer (`25`) or a range (`25-30`).

## Example

```ini
[Interface]
PrivateKey = <base64 private key>
ListenPort = 51820
Address = 10.0.0.1/24

Jc = 5
Jmin = 20
Jmax = 1000
S1 = 10
I1 = custom
H1 = 25-30

[Peer]
PublicKey = <base64 public key>
AllowedIPs = 10.0.0.2/32
```
