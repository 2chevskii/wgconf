# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WgConf is a .NET library for reading and writing WireGuard configuration files. The project provides strongly-typed models for WireGuard configurations (Interface and Peer sections) and reader/writer classes to serialize/deserialize them.

## Build and Development Commands

### Build
```bash
dotnet build
```

### Restore dependencies
```bash
dotnet restore
```

### Run tests
```bash
dotnet test
```

### Format code (using CSharpier)
```bash
dotnet csharpier format .
```

## Project Configuration

- **Target Framework**: .NET 10.0 (net10.0)
- **SDK Version**: 10.0.102 (specified in global.json)
- **Solution Format**: XML-based `.slnx` format (not traditional `.sln`)
- **Package Management**: Central Package Management enabled via Directory.Packages.props
- **Code Formatter**: CSharpier 1.2.5 (local tool, configured in dotnet-tools.json)

## Architecture

### Core Domain Models

- **WireguardConfiguration**: Represents the `[Interface]` section containing PrivateKey, ListenPort, Address (CIDR), and optional hook commands (PreUp, PostUp, PreDown, PostDown). Contains a collection of Peers.
- **WireguardPeerConfiguration**: Represents a `[Peer]` section with PublicKey, AllowedIPs (CIDR array), and optional Endpoint, PresharedKey, and PersistedKeepalive.
- **CIDR**: Value type (struct) representing an IP address with prefix length (e.g., 10.0.0.1/24).

### Reader/Writer Pattern

- **WireguardConfigurationReader**: Base class with TextReader dependency. The `Read()` method is virtual but currently throws NotImplementedException - this is a skeleton awaiting implementation.
- **WireguardConfigurationWriter**: Fully implemented writer that serializes WireguardConfiguration to WireGuard's INI-like format. Uses TextWriter dependency and supports both sync (IDisposable) and async (IAsyncDisposable) disposal.

### Utilities

- **FunctionalExtensions**: Internal extension providing `Let` method for conditional execution on nullable types (both reference and value types). Used throughout WireguardConfigurationWriter to conditionally write optional properties.

## Testing

The project includes xUnit test coverage for:
- **WireguardConfigurationWriter**: Tests cover minimal and complete configurations, optional properties, multiple peers, IPv4/IPv6 support, and proper formatting.
- **CIDR**: Tests cover parsing (both string and ReadOnlySpan<char> overloads), validation of prefix lengths for IPv4/IPv6, ToString() formatting, and round-trip serialization.

## Key Implementation Details

- All cryptographic keys (PrivateKey, PublicKey, PresharedKey) are stored as byte arrays and serialized to/from Base64 in the configuration file format.
- The writer outputs property names using nameof() to ensure consistency with model property names.
- CIDR arrays in AllowedIPs are comma-separated in the output format.
- The CIDR struct includes Parse() methods for both string and ReadOnlySpan<char>, with full validation of IP addresses and prefix lengths.
- The WireguardConfigurationReader is currently a stub - any implementation work should complete the parsing logic for the WireGuard configuration format.