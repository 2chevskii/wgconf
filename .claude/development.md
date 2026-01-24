# Development

## Build Configuration

### Target Framework
- **.NET 8.0 LTS** (`net8.0`)
- **SDK**: 10.0.102 (locked in `global.json`)

### Solution Format
- **XML-based `.slnx`** (not traditional `.sln`)
- Organizes into folders: `/common/`, `/misc/`, `/src/`, `/test/`

### Package Management
- **Central Package Management** via `Directory.Packages.props`
- All package versions defined centrally
- Projects reference packages without version numbers

## Build Commands

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Build Release configuration
dotnet build --configuration Release

# Restore local tools (CSharpier, etc.)
dotnet tool restore

# Clean build artifacts
dotnet clean
```

## Code Formatting

**Formatter**: CSharpier 1.2.5 (local tool)

```bash
# Format entire solution
dotnet csharpier format .

# Check formatting without modifying
dotnet csharpier check .

# Format specific directory
dotnet csharpier format src/WgConf
```

**CI Enforcement**: CI pipeline validates formatting with `dotnet csharpier check .`

## Code Style (`.editorconfig`)

### General
- **Indentation**: 4 spaces for C#, 2 spaces for XML/JSON
- **Encoding**: UTF-8 with BOM for C#
- **Line endings**: CRLF (Windows)
- **No hard line limit**: 80 chars suggested

### C# Preferences
- **File-scoped namespaces**: Preferred (`csharp_style_namespace_declarations = file_scoped:warning`)
- **Primary constructors**: Preferred when applicable
- **Expression bodies**: Allowed for methods/properties/accessors
- **Pattern matching**: Enabled and preferred
- **Explicit types**: Preferred over `var` (no hard rule)

### Naming Conventions
- **Types/Methods/Properties**: PascalCase
- **Local variables/parameters**: camelCase
- **Private fields**: `_camelCase` (underscore prefix)
- **Static fields**: `s_camelCase` (s_ prefix for static, `t_` for thread-static)
- **Constants**: PascalCase

### Code Quality
- **Nullable reference types**: Enabled project-wide
- **Implicit usings**: Enabled
- **Treat warnings as errors**: Enabled in Release builds
- **Deterministic builds**: Enabled on CI

## Project Properties (`Directory.Build.props`)

**Shared across all projects**:
- Target framework: `net8.0`
- Nullable: `enable`
- ImplicitUsings: `enable`
- Authors: 2CHEVSKII
- Copyright: Copyright (c) 2026 2CHEVSKII
- RepositoryUrl: https://github.com/dvchevskii/wgconf
- PackageLicenseExpression: MIT
- PackageTags: wireguard;wg;configuration;config;parser;amneziawg

**Build-time packages**:
- GitVersion.MsBuild 6.2.0 (automatic versioning)
- Microsoft.SourceLink.GitHub 8.0.0 (source debugging support)

**CI-specific**:
- Deterministic: `true` on CI
- ContinuousIntegrationBuild: `true` on CI

## Dependencies

### Production
**WgConf**: No external runtime dependencies (pure .NET 8.0)

**WgConf.Amnezia**: ProjectReference to WgConf only

### Build-time
- GitVersion.MsBuild 6.2.0
- Microsoft.SourceLink.GitHub 8.0.0

### Testing
- xunit 2.9.3
- xunit.runner.visualstudio 3.1.5
- Microsoft.NET.Test.Sdk 18.0.1
- coverlet.collector 6.0.4

### Tools (dotnet-tools.json)
- **CSharpier 1.2.5** - Code formatter (no rollForward)
- **coverlet.console 6.0.4** - Code coverage
- **GitVersion.Tool** - Semantic versioning (global tool)
- **ReportGenerator 5.4.3** - Coverage report generation

## Local Development Workflow

### Initial Setup
```bash
# Clone repository
git clone https://github.com/dvchevskii/wgconf.git
cd wgconf

# Restore dependencies and tools
dotnet restore
dotnet tool restore
```

### Development Cycle
```bash
# 1. Make changes to code
# 2. Format code
dotnet csharpier format .

# 3. Build
dotnet build

# 4. Run tests
dotnet test

# 5. Check coverage (optional)
dotnet test --collect:"XPlat Code Coverage" --settings .runsettings
```

### Testing Local Package Build
```bash
# Calculate version
dotnet-gitversion

# Build with specific version
dotnet build --configuration Release /p:Version=1.0.0-local

# Pack packages
dotnet pack src/WgConf/WgConf.csproj --configuration Release --output ./nupkgs
dotnet pack src/WgConf.Amnezia/WgConf.Amnezia.csproj --configuration Release --output ./nupkgs

# Inspect packages
dotnet nuget verify ./nupkgs/*.nupkg
```

## Code Conventions

### Comments
- **Minimize comments**: Write self-explanatory code
- Only comment non-obvious logic requiring explicit explanation
- No in-code documentation unless explicitly requested
- No XML doc comments on internal types (public API only)

### Error Handling
- Use structured `ParseError` for validation errors
- Aggregate errors before throwing `WireguardConfigurationException`
- Provide dual API: throw vs try-pattern (`Read()` vs `TryRead()`)

### Resource Management
- Implement `IDisposable` for sync operations
- Implement `IAsyncDisposable` for async operations
- Use `using` statements/declarations for proper cleanup

### Testing
- xUnit framework
- One test class per production class
- Separate async tests into dedicated test classes
- Test names follow pattern: `MethodName_Scenario_ExpectedBehavior`
- Arrange-Act-Assert pattern
