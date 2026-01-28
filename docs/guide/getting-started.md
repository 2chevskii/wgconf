# Getting Started

## Install

```bash
dotnet add package WgConf
```

For AmneziaWG support:

```bash
dotnet add package WgConf.Amnezia
```

Requirements: .NET 8.0 or later.

## Read a configuration

Parse from a string:

```csharp
using WgConf;

var config = WireguardConfiguration.Parse(text);
```

Load from a file:

```csharp
using WgConf;

var config = WireguardConfiguration.Load("wg0.conf");
```

Async load:

```csharp
using WgConf;

var config = await WireguardConfiguration.LoadAsync("wg0.conf");
```

## Write a configuration

```csharp
using WgConf;

var config = new WireguardConfiguration
{
    PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
    ListenPort = 51820,
    Address = CIDR.Parse("10.0.0.1/24"),
};

config.Peers.Add(new WireguardPeerConfiguration
{
    PublicKey = Convert.FromBase64String("xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg="),
    AllowedIPs = [CIDR.Parse("10.0.0.2/32")],
    Endpoint = WireguardEndpoint.Parse("vpn.example.com:51820"),
    PersistedKeepalive = 25,
});

config.Save("wg0.conf");
```

## AmneziaWG quick start

```csharp
using WgConf.Amnezia;

var config = new AmneziaWgConfiguration
{
    PrivateKey = Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftTLq5yJTfxLUcv7hag="),
    ListenPort = 51820,
    Address = "10.0.0.1/24",
    Jc = 5,
    Jmin = 20,
    I1 = "custom",
    H1 = "25-30",
};

config.Save("awg0.conf");
```

## Minimal required fields

The parser treats these properties as required:

- `[Interface]`: `PrivateKey`, `ListenPort`, `Address`
- `[Peer]`: `PublicKey`, `AllowedIPs`

Missing required properties are returned as parse errors.
