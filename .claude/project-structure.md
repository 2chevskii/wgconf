# Project Structure

## Directory Layout

```
wgconf/
├── .github/
│   └── workflows/
│       └── ci.yml                                # CI/CD pipeline
│
├── src/
│   ├── WgConf/                                   # Base library (632 lines in reader)
│   │   ├── WireguardConfiguration.cs             # Interface section model
│   │   ├── WireguardPeerConfiguration.cs         # Peer section model
│   │   ├── CIDR.cs                               # IP+prefix value type
│   │   ├── WireguardEndpoint.cs                  # host:port value type
│   │   ├── ParseError.cs                         # Structured error info
│   │   ├── WireguardConfigurationException.cs    # Aggregated errors
│   │   ├── WireguardConfigurationReader.cs       # INI parser (state machine)
│   │   ├── WireguardConfigurationWriter.cs       # INI serializer
│   │   ├── Extensions/
│   │   │   └── FunctionalExtensions.cs           # Let() method for nullables
│   │   ├── WgConf.csproj                         # Project file
│   │   └── README.md                             # Package documentation
│   │
│   └── WgConf.Amnezia/                           # AmneziaWG extension
│       ├── AmneziaWgConfiguration.cs             # Extended config with 20 properties
│       ├── AmneziaWgConfigurationReader.cs       # Extended reader
│       ├── AmneziaWgConfigurationWriter.cs       # Extended writer
│       ├── IntegerRange.cs                       # start-end value type
│       ├── WgConf.Amnezia.csproj                 # Project file
│       └── README.md                             # Package documentation
│
├── test/
│   ├── WgConf.Tests/                             # Base library tests (81 tests)
│   │   ├── CIDRTests.cs
│   │   ├── WireguardConfigurationReaderTests.cs
│   │   ├── WireguardConfigurationReaderAsyncTests.cs
│   │   ├── WireguardConfigurationWriterTests.cs
│   │   ├── WireguardConfigurationTests.cs
│   │   ├── WireguardConfigurationValidationTests.cs
│   │   ├── WireguardEndpointTests.cs
│   │   └── WgConf.Tests.csproj
│   │
│   └── WgConf.Amnezia.Tests/                     # Extension tests (36 tests)
│       ├── IntegerRangeTests.cs
│       ├── AmneziaWgConfigurationReaderTests.cs
│       ├── AmneziaWgConfigurationWriterTests.cs
│       └── WgConf.Amnezia.Tests.csproj
│
├── .claude/                                       # Claude Code documentation
│   ├── architecture.md                           # Domain models, patterns
│   ├── development.md                            # Build, tooling, style
│   ├── testing.md                                # Test structure, coverage
│   ├── cicd.md                                   # Pipeline, versioning, release
│   └── project-structure.md                      # This file
│
├── WgConf.slnx                                   # XML solution file
├── global.json                                   # SDK version lock (10.0.102)
├── dotnet-tools.json                             # Local tools manifest
├── Directory.Build.props                         # Shared MSBuild properties
├── Directory.Packages.props                      # Central package versions
├── GitVersion.yml                                # Versioning configuration
├── .runsettings                                  # Test coverage settings
├── .editorconfig                                 # Code style rules (390 lines)
├── .gitattributes                                # Git line endings
├── .gitignore                                    # Git ignore patterns
├── CLAUDE.md                                     # Quick reference (this file)
├── README.md                                     # Repository README
└── LICENSE                                       # MIT License

Build Artifacts (auto-generated, gitignored):
├── bin/                                          # Compiled assemblies
├── obj/                                          # Intermediate build files
├── TestResults/                                  # Test results and coverage
├── coverage-report/                              # HTML coverage reports
├── nupkgs/                                       # Local NuGet packages
└── artifacts/                                    # Multilingual resources
```

## File Counts and Sizes

### Production Code
- **WgConf**: 7 files, ~1,100 lines
  - Largest: `WireguardConfigurationReader.cs` (632 lines)
  - Smallest: `WireguardPeerConfiguration.cs` (12 lines)
- **WgConf.Amnezia**: 4 files, ~480 lines
  - Largest: `AmneziaWgConfigurationReader.cs` (274 lines)

### Test Code
- **WgConf.Tests**: 7 files, 2,187 lines
  - Largest: `WireguardConfigurationTests.cs` (652 lines)
- **WgConf.Amnezia.Tests**: 3 files, 696 lines
  - Largest: `AmneziaWgConfigurationReaderTests.cs` (314 lines)

### Total
- **Production**: 11 files, ~1,580 lines
- **Tests**: 10 files, 2,883 lines
- **Test Coverage**: 117 tests

## Dependencies

### Production Dependencies
**WgConf**: None (pure .NET 8.0)

**WgConf.Amnezia**:
- ProjectReference: WgConf

### Build Dependencies (All Projects)
- GitVersion.MsBuild 6.2.0 (automatic versioning)
- Microsoft.SourceLink.GitHub 8.0.0 (source debugging)

### Test Dependencies (Test Projects Only)
- xunit 2.9.3 (test framework)
- xunit.runner.visualstudio 3.1.5 (VS test integration)
- Microsoft.NET.Test.Sdk 18.0.1 (test SDK)
- coverlet.collector 6.0.4 (code coverage)

### Development Tools (dotnet-tools.json)
- CSharpier 1.2.5 (code formatter)
- coverlet.console 6.0.4 (coverage CLI)
- ReportGenerator 5.4.3 (coverage reports - global tool)
- GitVersion.Tool (versioning - global tool)

## Configuration Files

### Build System
- **WgConf.slnx**: XML-based solution manifest (new .NET format)
- **global.json**: Locks SDK to version 10.0.102
- **Directory.Build.props**: Shared MSBuild properties for all projects
  - Target framework: net8.0
  - Nullable: enabled
  - ImplicitUsings: enabled
  - Package metadata (authors, license, tags)
  - SourceLink configuration
  - Deterministic builds on CI
- **Directory.Packages.props**: Central Package Management
  - All package versions defined here
  - Projects reference packages without versions

### Code Quality
- **.editorconfig** (390 lines): Comprehensive C# style guide
  - Indentation: 4 spaces for C#
  - File-scoped namespaces preferred
  - Naming conventions (PascalCase, camelCase, _camelCase)
  - Modern C# features enabled
- **dotnet-tools.json**: Local tool manifest
  - CSharpier 1.2.5 (no rollForward)
  - coverlet.console 6.0.4

### Versioning
- **GitVersion.yml**: Semantic versioning configuration
  - Mode: ContinuousDelivery
  - Tag prefix: `[vV]`
  - Commit message parsing for `+semver:` directives

### Testing
- **.runsettings**: Test execution configuration
  - Coverage format: OpenCover XML
  - Results directory: ./TestResults
  - Exclusions: test assemblies, generated code
  - Threshold: 80% line coverage

### Git
- **.gitattributes**: Line ending normalization (CRLF on Windows)
- **.gitignore**: Standard .NET ignore patterns
  - /bin/, /obj/, /TestResults/
  - *.user, *.suo, .vs/
  - /nupkgs/, /coverage-report/

## Project References

### Source Projects

**WgConf**:
- No project references
- No NuGet dependencies (runtime)
- Build dependencies: GitVersion.MsBuild, Microsoft.SourceLink.GitHub

**WgConf.Amnezia**:
- ProjectReference: `../WgConf/WgConf.csproj`
- Inherits all WgConf capabilities
- No additional NuGet dependencies

### Test Projects

**WgConf.Tests**:
- ProjectReference: `../../src/WgConf/WgConf.csproj`
- NuGet: xunit, Microsoft.NET.Test.Sdk, coverlet.collector

**WgConf.Amnezia.Tests**:
- ProjectReference: `../../src/WgConf.Amnezia/WgConf.Amnezia.csproj`
- NuGet: xunit, Microsoft.NET.Test.Sdk, coverlet.collector
- Transitively references WgConf via WgConf.Amnezia

## Solution Organization (.slnx)

**Folders**:
1. **/common/** - Build configuration
   - Directory.Build.props
   - Directory.Packages.props
   - global.json

2. **/misc/** - Editor and documentation
   - .editorconfig
   - .gitattributes
   - .gitignore
   - README.md
   - LICENSE

3. **/src/** - Production code
   - WgConf/
   - WgConf.Amnezia/

4. **/test/** - Test code
   - WgConf.Tests/
   - WgConf.Amnezia.Tests/

## Build Output Locations

**Debug/Release Builds**:
- Assemblies: `{Project}/bin/{Configuration}/net8.0/`
- Intermediate: `{Project}/obj/{Configuration}/net8.0/`

**NuGet Packages** (dotnet pack):
- Default: `{Project}/bin/{Configuration}/`
- Custom: Specify `--output` parameter

**Test Results**:
- Location: `./TestResults/`
- Coverage: `TestResults/{guid}/coverage.opencover.xml`
- TRX files: `TestResults/{guid}/results.trx`

**Coverage Reports** (CI):
- HTML: `./coverage-report/index.html`
- Cobertura: `./coverage-report/Cobertura.xml`
- Badges: `./coverage-report/badge_linecoverage.svg`

## File Naming Conventions

### Source Files
- Class per file: `ClassName.cs`
- Value types: `CIDR.cs`, `WireguardEndpoint.cs`, `IntegerRange.cs`
- Exceptions: `WireguardConfigurationException.cs`
- Records: `ParseError.cs`

### Test Files
- Pattern: `{ClassName}Tests.cs`
- Async variant: `{ClassName}AsyncTests.cs`
- Validation: `{ClassName}ValidationTests.cs`

### Configuration Files
- MSBuild: `{Name}.props`, `{Name}.targets`, `{Name}.csproj`
- JSON: `global.json`, `dotnet-tools.json`
- YAML: `GitVersion.yml`, `ci.yml`
- XML: `.editorconfig`, `.runsettings`, `WgConf.slnx`

## Package Structure (NuGet)

### WgConf.nupkg
```
WgConf.1.0.0.nupkg
├── lib/
│   └── net8.0/
│       ├── WgConf.dll
│       └── WgConf.xml (doc comments)
├── README.md
├── LICENSE
└── .signature.p7s (if signed)
```

### WgConf.Amnezia.nupkg
```
WgConf.Amnezia.1.0.0.nupkg
├── lib/
│   └── net8.0/
│       ├── WgConf.Amnezia.dll
│       └── WgConf.Amnezia.xml
├── README.md
├── LICENSE
└── Dependencies:
    └── WgConf >= 1.0.0
```

### Symbol Packages (.snupkg)
- Contains PDB files for debugging
- Published alongside main packages
- SourceLink enabled for source stepping
