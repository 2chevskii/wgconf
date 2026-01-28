# Parsing and Writing

This section describes how the reader and writer behave and how to integrate them in your own flows.

## Parsing APIs

You can parse using the static helpers on `WireguardConfiguration`:

```csharp
var config = WireguardConfiguration.Parse(text);
```

Or use the reader directly to capture errors without throwing:

```csharp
using var reader = new WireguardConfigurationReader(new StringReader(text));
if (reader.TryRead(out var config, out var errors))
{
    // use config
}
else
{
    // inspect errors
}
```

Async variants are also available:

```csharp
var config = await WireguardConfiguration.LoadAsync("wg0.conf");
```

## Reader behavior

- The parser understands only `[Interface]` and `[Peer]` sections.
- Property names are case insensitive.
- Duplicate properties are allowed; the last value wins.
- Lines must be in `Key = Value` format. Anything else is an error.
- Properties outside a section are errors.
- Inline and full line comments start with `#` and are stripped before parsing.
- Unknown properties and unknown sections are reported as errors.

### Required properties

The reader treats these properties as required and reports errors if they are missing:

- `[Interface]`: `PrivateKey`, `ListenPort`, `Address`
- `[Peer]`: `PublicKey`, `AllowedIPs`

### Line numbers in errors

Structural errors (unknown sections, invalid line format, properties outside sections) include line numbers and surrounding lines. Value validation errors (invalid Base64 keys, invalid CIDR, invalid ports) are recorded with `LineNumber = 0` because they are detected after property collection.

## Writing APIs

The writer serializes a configuration to a text writer:

```csharp
using var writer = new WireguardConfigurationWriter(new StreamWriter("wg0.conf"));
writer.Write(config);
```

`WireguardConfiguration.Save` and `SaveAsync` are convenience wrappers that create a writer for you.

## Writer output format

The writer outputs:

- `[Interface]` first, then each `[Peer]` section.
- A blank line after the interface section and after each peer section.
- Properties in a fixed order.
- Base64 for keys and comma+space for `AllowedIPs`.

Interface property order:

1. `PrivateKey`
2. `ListenPort`
3. `Address`
4. `PreUp`
5. `PostUp`
6. `PreDown`
7. `PostDown`

Peer property order:

1. `AllowedIPs`
2. `PublicKey`
3. `PresharedKey`
4. `Endpoint`
5. `PersistedKeepalive`

## Customizing behavior

`WireguardConfigurationReader` and `WireguardConfigurationWriter` are designed for extension:

- Override `IsValidInterfaceProperty` and `IsValidPeerProperty` to accept additional keys.
- Override `BuildConfiguration` or `BuildPeer` to map custom values to your own models.
- Override `WriteInterface` or `WritePeer` to change output ordering or formatting.

AmneziaWG support is implemented using this extension model. See [AmneziaWG Extension](/reference/amnezia).
