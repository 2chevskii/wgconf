# AGENTS.md

This file is the local operating guide for assistants working in this repository. Read it first whenever context is missing.

## Project Snapshot

- **Repo**: WgConf
- **Purpose**: .NET library for reading and writing WireGuard configuration files with strong typing, strict validation, and detailed error reporting. `WgConf.Amnezia` extends this with AmneziaWG obfuscation parameters.
- **Target framework**: .NET 8.0 (SDK locked via `global.json`)
- **Solution**: `WgConf.slnx` (XML)
- **Formatting**: CSharpier (local tool) + `.editorconfig` rules
- **Docs**: Vitepress v2 under `docs/`

## Sources of Truth

- **Code is authoritative**. Some `.claude/*.md` docs are outdated (they still mention `IntegerRange`). The current Amnezia type is `HeaderValue` and uses `ulong` ranges.
- Prefer reading `src/` and tests when unsure.

## Key Domain Concepts

### WireguardConfiguration (`src/WgConf/WireguardConfiguration.cs`)

Represents the `[Interface]` section.

Required properties:
- `PrivateKey` (`byte[]`) must be **32 bytes** (Base64 in config files)
- `ListenPort` (`ushort`) must be **1-65535**
- `Address` (`CIDR`) parsed from `address/prefix`

Optional properties:
- `PreUp`, `PostUp`, `PreDown`, `PostDown` (`string?`)
- `Peers` (`List<WireguardPeerConfiguration>`)

Factory and I/O methods:
- `Parse`, `TryParse`, `Load`, `LoadAsync`
- `Save`, `SaveAsync`

### WireguardPeerConfiguration (`src/WgConf/WireguardPeerConfiguration.cs`)

Represents a `[Peer]` section.

Required:
- `PublicKey` (`byte[]`, 32 bytes)
- `AllowedIPs` (`List<CIDR>`, comma-separated in files)

Optional:
- `Endpoint` (`WireguardEndpoint?`, `host:port` or `[ipv6]:port`)
- `PresharedKey` (`byte[]?`, 32 bytes)
- `PersistedKeepalive` (`int?`)  **Note the spelling: `PersistedKeepalive`**

### CIDR (`src/WgConf/CIDR.cs`)

- `address/prefix` format, IPv4 or IPv6
- Prefix length must be within address-family limits (0-32 or 0-128)
- `Parse`, `TryParse` plus implicit conversion from `ReadOnlySpan<char>`

### WireguardEndpoint (`src/WgConf/WireguardEndpoint.cs`)

- `host:port`, IPv6 requires brackets (`[::1]:51820`)
- Port range: 1-65535
- `Parse`, `TryParse` plus implicit conversion from `ReadOnlySpan<char>`

### ParseError / WireguardConfigurationException

- `ParseError` includes line number, message, optional property/section, and surrounding lines.
- Validation errors discovered **after** property collection use `LineNumber = 0`.
- `Read()`/`Parse()` throw `WireguardConfigurationException` when any errors exist.

### AmneziaWG Extension (`src/WgConf.Amnezia`)

`AmneziaWgConfiguration` inherits from `WireguardConfiguration` and adds optional `[Interface]` properties:

- **Integers**: `Jc`, `Jmin`, `Jmax`, `S1`, `S2`, `S3`, `S4`, `J1`, `J2`, `J3`, `Itime`
- **Strings**: `I1`, `I2`, `I3`, `I4`, `I5`
- **Header values**: `H1`, `H2`, `H3`, `H4` (`HeaderValue?`)

`HeaderValue` (`src/WgConf.Amnezia/HeaderValue.cs`):
- `ulong Start` and optional `ulong End`
- Accepts single values (`25`) or ranges (`25-30`)
- Range requires `End > Start` (no equal)

## Parser Behavior (WireguardConfigurationReader)

- Only `[Interface]` and `[Peer]` sections are valid.
- Property names are **case-insensitive**.
- Duplicate properties are allowed; **last wins**.
- Line format must be `Key = Value`.
- Properties outside any section are errors.
- Inline and full-line `#` comments are stripped before parsing.
- Unknown properties/sections are reported as errors.
- Required fields missing produce `ParseError`s.

## Writer Behavior (WireguardConfigurationWriter)

- Writes `[Interface]` first, then each `[Peer]`.
- Blank line after interface and after each peer section.
- Fixed property order.

Interface order:
1. `PrivateKey`
2. `ListenPort`
3. `Address`
4. `PreUp`
5. `PostUp`
6. `PreDown`
7. `PostDown`

Peer order:
1. `AllowedIPs`
2. `PublicKey`
3. `PresharedKey`
4. `Endpoint`
5. `PersistedKeepalive`

AmneziaWG writer appends Amnezia properties **after** the standard `[Interface]` properties.

## Repo Layout (Important Paths)

- `src/WgConf/` - Core library
- `src/WgConf.Amnezia/` - AmneziaWG extension
- `tests/` - xUnit tests
- `docs/` - Vitepress v2 documentation (avoid editing `docs/node_modules`)
- `.claude/` - Supplemental docs (may be outdated)

## Code Style

- Use file-scoped namespaces and modern C# patterns.
- Follow `.editorconfig`: 4 spaces, CRLF, UTF-8 with BOM for C# files.
- Keep comments minimal; prefer self-explanatory code.
- Nullable reference types are enabled.

Formatting:
- `dotnet csharpier format .`
- `dotnet csharpier check .` (CI gate)

## Tests and Coverage

- Test projects: `tests/WgConf.Tests`, `tests/WgConf.Amnezia.Tests`
- Run: `dotnet test`
- CI enforces **>= 80% line coverage**.

## CI/CD and Versioning

- Versions are set manually in `Directory.Build.props` (`Version` is authoritative).
- Release tags are `vX.Y.Z` or `vX.Y.Z-prerelease` and must match `Version` on `master` tip.
- Release is two-stage: `start_release.yaml` validates/builds/tests/coverage and creates a draft release with assets, `finish_release.yaml` publishes packages + docs after release is published.
- CI runs: build, formatting check, tests, coverage on push/PR.

## Documentation Workflow

- Vitepress docs live in `docs/`.
- Dev server: `cd docs; npm run dev`
- Update docs when public APIs or behavior changes.

## Common Pitfalls to Remember

- `PersistedKeepalive` spelling is intentional and used in files.
- `HeaderValue` is the Amnezia range type (not `IntegerRange`).
- Validation errors often use `LineNumber = 0`.
- Unknown properties/sections are treated as errors (strict parsing).

## Quick Commands

```bash
# Build
dotnet build

# Test
dotnet test

# Format
dotnet csharpier format .
```

