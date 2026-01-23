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

- **Target Framework**: .NET 8.0 LTS (net8.0)
- **SDK Version**: 10.0.102 (specified in global.json - using newer SDK for build tooling)
- **Solution Format**: XML-based `.slnx` format (not traditional `.sln`)
- **Package Management**: Central Package Management enabled via Directory.Packages.props
- **Code Formatter**: CSharpier 1.2.5 (local tool, configured in dotnet-tools.json)

## Architecture

### Core Domain Models

- **WireguardConfiguration**: Represents the `[Interface]` section containing PrivateKey, ListenPort, Address (CIDR), and optional hook commands (PreUp, PostUp, PreDown, PostDown). Contains a collection of Peers. Includes static factory methods: `Parse()`, `TryParse()`, `Load()`, `LoadAsync()`, and instance methods `Save()`, `SaveAsync()`.
- **WireguardPeerConfiguration**: Represents a `[Peer]` section with PublicKey, AllowedIPs (CIDR array), and optional Endpoint (WireguardEndpoint), PresharedKey, and PersistedKeepalive.
- **CIDR**: Value type (struct) representing an IP address with prefix length (e.g., 10.0.0.1/24).
- **WireguardEndpoint**: Value type (struct) representing a WireGuard endpoint in `host:port` format, with support for IPv6 addresses in brackets (e.g., `[::1]:51820`).

### Reader/Writer Pattern

- **WireguardConfigurationReader**: Fully implemented reader that parses WireGuard configuration files using a state machine approach. Features:
  - Dual API: `Read()`/`ReadAsync()` (throws on error) and `TryRead()`/`TryReadAsync()` (returns bool/tuple)
  - Comment stripping (both full-line and inline `#` comments)
  - Case-insensitive property names
  - Last-wins behavior for duplicate properties
  - Error accumulation with detailed context (line numbers, surrounding lines)
  - Validation: WireGuard key length (32 bytes), port range (1-65535)
- **WireguardConfigurationWriter**: Fully implemented writer that serializes WireguardConfiguration to WireGuard's INI-like format. Uses TextWriter dependency and supports both sync (IDisposable) and async (IAsyncDisposable) disposal.

### Error Handling

- **ParseError**: Record containing structured error information (line number, message, property name, section, line text, surrounding lines for context).
- **WireguardConfigurationException**: Custom exception aggregating all ParseError instances found during parsing. Provides detailed error messages similar to compiler output.

### Utilities

- **FunctionalExtensions**: Internal extension providing `Let` method for conditional execution on nullable types (both reference and value types). Used throughout WireguardConfigurationWriter to conditionally write optional properties.

## Testing

The project includes xUnit test coverage for:

- **WireguardConfigurationWriter**: Tests cover minimal and complete configurations, optional properties, multiple peers, IPv4/IPv6 support, and proper formatting.
- **WireguardConfigurationReader**: Tests cover valid configurations, comment handling, case-insensitivity, duplicate properties, error cases (unknown properties/sections, invalid formats), TryRead pattern, and round-trip with writer.
- **CIDR**: Tests cover parsing (both string and ReadOnlySpan<char> overloads), validation of prefix lengths for IPv4/IPv6, ToString() formatting, and round-trip serialization.
- **WireguardEndpoint**: Tests cover parsing hostnames, IPv4, IPv6 (with brackets), port validation, TryParse pattern, ToString formatting, and round-trip serialization.

## Key Implementation Details

- All cryptographic keys (PrivateKey, PublicKey, PresharedKey) are stored as byte arrays and serialized to/from Base64 in the configuration file format.
- The writer outputs property names using nameof() to ensure consistency with model property names.
- CIDR arrays in AllowedIPs are comma-separated in both reading and writing.
- The CIDR struct includes Parse() methods for both string and ReadOnlySpan<char>, with full validation of IP addresses and prefix lengths.
- The reader uses a state machine to track Interface/Peer sections and accumulates all errors before throwing.

## WgConf.Amnezia Extension

The WgConf.Amnezia project extends WgConf to support AmneziaWG-specific configuration parameters. AmneziaWG is a WireGuard variant with additional obfuscation parameters.

### AmneziaWG-Specific Parameters (All Optional, [Interface] Section Only)

- **Integer Properties** (11): Jc, Jmin, Jmax, S1, S2, S3, S4, J1, J2, J3, Itime
- **String Properties** (5): I1, I2, I3, I4, I5
- **Integer Range Properties** (4): H1, H2, H3, H4 (format: "25-30")

### Amnezia Domain Models

- **AmneziaWgConfiguration**: Extends `WireguardConfiguration` with 20 additional optional properties for AmneziaWG obfuscation parameters. Inherits all standard WireGuard properties and peer collection. Includes same static factory methods as base class: `Parse()`, `TryParse()`, `Load()`, `LoadAsync()`, `Save()`, `SaveAsync()`.
- **IntegerRange**: Value type (struct) representing an integer range in `start-end` format (e.g., "25-30"). Follows the same pattern as CIDR and WireguardEndpoint with `Parse(string)`, `Parse(ReadOnlySpan<char>)`, `TryParse()`, and `ToString()` methods. Uses `LastIndexOf('-')` to support negative numbers.

### Amnezia Reader/Writer

- **AmneziaWgConfigurationReader**: Extends `WireguardConfigurationReader` to handle AmneziaWG properties. Overrides `IsValidInterfaceProperty()` to whitelist AmneziaWG property names and `BuildConfiguration()` to create `AmneziaWgConfiguration` instances with parsed AmneziaWG properties. Maintains all base reader features (error accumulation, case-insensitivity, dual API).
- **AmneziaWgConfigurationWriter**: Extends `WireguardConfigurationWriter` to output AmneziaWG properties. Overrides `WriteInterface()` to write standard WireGuard properties first, then AmneziaWG properties. Uses `FunctionalExtensions.Let()` for optional properties (now public instead of internal).

### Extensibility Pattern

The WgConf base library has been refactored for extensibility:

- `WireguardConfigurationReader` methods changed from `private` to `protected virtual`: `IsValidInterfaceProperty()`, `IsValidPeerProperty()`, `BuildConfiguration()`, `BuildPeer()`
- `WireguardConfigurationWriter` methods changed from `private` to `protected virtual`: `WriteInterface()`, `WritePeer()`
- `FunctionalExtensions` changed from `internal` to `public` for use in extension libraries
- All changes are backward compatible (only access modifiers changed, no behavior changes)

### Testing

The WgConf.Amnezia.Tests project includes comprehensive test coverage:

- **IntegerRangeTests** (16 tests): Parse valid/invalid ranges, negative numbers, TryParse pattern, ToString formatting, round-trip serialization
- **AmneziaWgConfigurationReaderTests** (14 tests): Parse minimal configs, integer/string/range properties, complete configs, case-insensitivity, error handling, TryRead pattern, async methods
- **AmneziaWgConfigurationWriterTests** (10 tests): Write minimal/complete configs, integer/string/range properties, null optional properties, property ordering, round-trip with reader

Total test count: 117 tests (81 WgConf + 36 WgConf.Amnezia)

## Versioning and Release Process

### Versioning Strategy

The project uses **GitVersion** with **Mainline mode** for automatic semantic versioning based on git history and tags.

- **Version Format**: SemVer 2.0 (MAJOR.MINOR.PATCH)
- **Tag Prefix**: `v` (e.g., `v1.0.0`, `v2.1.3`)
- **Version Calculation**: GitVersion automatically calculates versions based on:
  - Tags (explicit versions)
  - Commit messages (with `+semver:` prefixes)
  - Branch names (feature branches get beta tags)

### Version Bump Commit Messages

You can control version increments via commit messages:

- `+semver: major` or `+semver: breaking` - Increments MAJOR version
- `+semver: minor` or `+semver: feature` - Increments MINOR version
- `+semver: patch` or `+semver: fix` - Increments PATCH version
- `+semver: none` - No version increment

### Branch Configuration

- **master**: Production releases (no tag suffix)
- **develop**: Alpha releases (tagged as `1.0.0-alpha.X`)
- **feature/**: Beta releases (tagged as `1.0.0-beta.X`)
- **pr/**: Pull request builds (tagged as `1.0.0-pr.X`)

### Creating a Release

To create a new release and publish to NuGet:

1. **Ensure all tests pass**: Run `dotnet test` locally
2. **Format code**: Run `dotnet csharpier format .`
3. **Commit changes**: Commit all changes to master
4. **Create and push a tag**:

   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

The CI/CD pipeline will automatically:

- Build the solution with the tagged version
- Run all tests with coverage validation (80% threshold)
- Validate code formatting
- Pack both `WgConf` and `WgConf.Amnezia` NuGet packages
- Publish packages to NuGet.org
- Create a GitHub Release with package links and installation instructions

### CI/CD Pipeline

The project uses GitHub Actions with two main jobs:

#### Build and Test Job (runs on every push/PR)

- **Trigger**: Push to master/develop, or pull requests
- **Steps**:
  1. Checkout code with full git history
  2. Setup .NET 8.0
  3. Install and run GitVersion to calculate version
  4. Restore dependencies
  5. Build with version injection
  6. Validate code formatting (CSharpier)
  7. Run tests with code coverage collection
  8. Generate HTML coverage report with ReportGenerator
  9. Check coverage threshold (80% line coverage required)
  10. Upload coverage and test result artifacts

#### Release Job (runs only on version tags)

- **Trigger**: Tags matching `v*` pattern (e.g., `v1.0.0`)
- **Steps**:
  1. Checkout code with full git history
  2. Setup .NET 8.0
  3. Install and run GitVersion
  4. Restore and build
  5. Pack `WgConf` NuGet package
  6. Pack `WgConf.Amnezia` NuGet package
  7. Push both packages to NuGet.org (requires `NUGET_API_KEY` secret)
  8. Create GitHub Release with:
     - Release notes with package links
     - Installation instructions
     - Attached `.nupkg` files
     - Prerelease flag if version contains pre-release tag

### GitHub Secrets Required

The following secrets must be configured in the GitHub repository:

- **NUGET_API_KEY**: API key for publishing to NuGet.org (get from <https://www.nuget.org/account/apikeys>)

### Package Configuration

Both packages are configured in their respective `.csproj` files:

- **PackageId**: `WgConf` and `WgConf.Amnezia`
- **Authors**: 2CHEVSKII
- **License**: MIT
- **Repository**: <https://github.com/dvchevskii/wgconf>
- **README**: Each package includes its own README.md shown on NuGet.org
- **SourceLink**: Enabled for source debugging support
- **Symbols**: Published as `.snupkg` symbol packages

### Coverage Settings

Code coverage is configured via `.runsettings`:

- **Format**: OpenCover XML
- **Threshold**: 80% line coverage (enforced in CI)
- **Exclusions**: Test assemblies, generated code, compiler-generated code
- **Reports**: HTML reports with badges and Cobertura XML for tooling

### Local Testing of Release Process

To test the release process locally without publishing:

```bash
# Calculate version
dotnet tool install --global GitVersion.Tool
dotnet-gitversion

# Build with version
dotnet build --configuration Release /p:Version=1.0.0

# Pack packages
dotnet pack src/WgConf/WgConf.csproj --configuration Release --output ./nupkgs /p:Version=1.0.0
dotnet pack src/WgConf.Amnezia/WgConf.Amnezia.csproj --configuration Release --output ./nupkgs /p:Version=1.0.0

# Inspect packages
dotnet nuget verify ./nupkgs/*.nupkg
```

### Troubleshooting

- **Coverage fails locally**: Ensure you have the latest SDK and run with `--settings .runsettings`
- **GitVersion fails**: Ensure git history is available (not shallow clone) and tags are present
- **Formatting check fails**: Run `dotnet csharpier format .` to fix formatting
- **NuGet push fails**: Verify `NUGET_API_KEY` secret is set and has push permissions
