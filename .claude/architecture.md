# Architecture

## Domain Models

### WgConf (Base Library)

**WireguardConfiguration** - `[Interface]` section
- `byte[] PrivateKey` - 32-byte WireGuard private key (Base64 in config)
- `ushort? ListenPort` - Port 1-65535
- `CIDR? Address` - IP address with prefix (e.g., `10.0.0.1/24`)
- `string? PreUp/PostUp/PreDown/PostDown` - Hook commands
- `List<WireguardPeerConfiguration> Peers` - Peer collection
- **Factory methods**: `Parse()`, `TryParse()`, `Load()`, `LoadAsync()`
- **Instance methods**: `Save()`, `SaveAsync()`

**WireguardPeerConfiguration** - `[Peer]` section
- `byte[] PublicKey` - 32-byte WireGuard public key (Base64 in config)
- `CIDR[] AllowedIPs` - Comma-separated in config
- `WireguardEndpoint? Endpoint` - Optional `host:port`
- `byte[]? PresharedKey` - Optional 32-byte key (Base64 in config)
- `ushort? PersistedKeepalive` - Optional keepalive interval

**CIDR** (struct) - IP address with prefix
- `Parse(string)`, `Parse(ReadOnlySpan<char>)`, `TryParse()`
- Validates IPv4 (0-32) and IPv6 (0-128) prefix lengths
- `ToString()` - Round-trip serialization

**WireguardEndpoint** (struct) - `host:port` format
- Supports hostnames, IPv4, IPv6 with brackets (e.g., `[::1]:51820`)
- `Parse(string)`, `Parse(ReadOnlySpan<char>)`, `TryParse()`
- Port validation (1-65535)

**ParseError** (record) - Structured error information
- `int LineNumber`, `string Message`, `string? PropertyName`
- `string? Section`, `string? LineText`, `List<string> SurroundingLines`

**WireguardConfigurationException** - Custom exception
- Aggregates all `ParseError` instances found during parsing
- Provides compiler-like error messages with context

### WgConf.Amnezia (Extension)

**AmneziaWgConfiguration** - Extends `WireguardConfiguration`
- Adds 20 optional AmneziaWG obfuscation properties:
  - **Integer** (11): `Jc`, `Jmin`, `Jmax`, `S1-S4`, `J1-J3`, `Itime`
  - **String** (5): `I1-I5`
  - **IntegerRange** (4): `H1-H4` (format: "25-30")
- Inherits all base factory/instance methods

**IntegerRange** (struct) - Single integer or `start-end` range format
- Supports both single values (e.g., `"25"`) and ranges (e.g., `"25-30"`)
- `int Start` - Always required
- `int? End` - Nullable; `null` indicates single value, non-null indicates range
- `Parse(string)`, `Parse(ReadOnlySpan<char>)`, `TryParse()`
- Uses `LastIndexOf('-')` to support negative numbers
- `ToString()` - Outputs single value or range format based on `End` value

## Reader/Writer Pattern

### WireguardConfigurationReader

**Features**:
- State machine parser for WireGuard INI-like format
- Comment stripping (full-line and inline `#`)
- Case-insensitive property names
- Last-wins behavior for duplicate properties
- Error accumulation with detailed context (line numbers, surrounding lines)
- Key validation (32 bytes Base64), port validation (1-65535)

**Dual API**:
- `Read()/ReadAsync()` - Throws `WireguardConfigurationException` on error
- `TryRead()/TryReadAsync()` - Returns `bool` with tuple output parameter

**Extensibility** (`protected virtual`):
- `IsValidInterfaceProperty(string)` - Whitelist Interface properties
- `IsValidPeerProperty(string)` - Whitelist Peer properties
- `BuildConfiguration()` - Construct configuration object
- `BuildPeer()` - Construct peer object

### WireguardConfigurationWriter

**Features**:
- Serializes to WireGuard INI-like format
- Uses `TextWriter` dependency injection
- Implements `IDisposable` (sync) and `IAsyncDisposable` (async)
- Property names use `nameof()` for consistency
- Uses `FunctionalExtensions.Let()` for optional properties

**Extensibility** (`protected virtual`):
- `WriteInterface()` - Write Interface section
- `WritePeer()` - Write Peer section

### AmneziaWgConfigurationReader

**Extends** `WireguardConfigurationReader`:
- Overrides `IsValidInterfaceProperty()` to whitelist AmneziaWG properties
- Overrides `BuildConfiguration()` to create `AmneziaWgConfiguration` instances
- Maintains all base features (error accumulation, dual API, async)

### AmneziaWgConfigurationWriter

**Extends** `WireguardConfigurationWriter`:
- Overrides `WriteInterface()` to write standard + AmneziaWG properties
- Standard WireGuard properties written first, then AmneziaWG properties
- Uses `FunctionalExtensions.Let()` (now public) for optional properties

## Utilities

**FunctionalExtensions** (now `public` for extensibility)
- `Let<T>(this T? value, Action<T> action)` - Conditional execution on nullable reference types
- `Let<T>(this T? value, Action<T> action) where T : struct` - Conditional execution on nullable value types
- Used throughout writers to conditionally write optional properties

## Extensibility Pattern

Base library refactored for extension:
1. Changed methods from `private` to `protected virtual`
2. Changed `FunctionalExtensions` from `internal` to `public`
3. All changes are backward compatible (access modifiers only)

**Example**: AmneziaWG extension demonstrates the pattern:
- Inherits base reader/writer classes
- Overrides specific `protected virtual` methods
- Adds new domain model properties
- Maintains full compatibility with base functionality
