# Readers and Writers

## WireguardConfigurationReader

Reads and validates configuration files from a `TextReader`.

### Constructor

```csharp
var reader = new WireguardConfigurationReader(new StreamReader(path));
```

### Methods

- `Read()`
- `ReadAsync(CancellationToken cancellationToken = default)`
- `TryRead(out WireguardConfiguration? configuration, out IReadOnlyList<ParseError> errors)`
- `TryReadAsync(CancellationToken cancellationToken = default)`
- `Dispose()` / `DisposeAsync()`

### Extension points

These protected virtual methods can be overridden to customize parsing behavior:

- `IsValidInterfaceProperty(string propertyName)`
- `IsValidPeerProperty(string propertyName)`
- `BuildConfiguration(Dictionary<string, string> interfaceProps, List<Dictionary<string, string>> peers, List<string> allLines, List<ParseError> errors)`
- `BuildPeer(Dictionary<string, string> peerProps, List<string> allLines, List<ParseError> errors)`

## WireguardConfigurationWriter

Writes configurations to a `TextWriter`.

### Constructor

```csharp
var writer = new WireguardConfigurationWriter(new StreamWriter(path));
```

### Methods

- `Write(WireguardConfiguration configuration)`
- `Dispose()` / `DisposeAsync()`

### Extension points

- `WriteInterface(WireguardConfiguration configuration)`
- `WritePeer(WireguardPeerConfiguration configuration)`

## AmneziaWG readers and writers

`AmneziaWgConfigurationReader` and `AmneziaWgConfigurationWriter` extend the base reader and writer to support AmneziaWG properties. They reuse the same extension points and simply widen the set of accepted `[Interface]` keys.
