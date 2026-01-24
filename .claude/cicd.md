# CI/CD Pipeline

## Versioning Strategy

### GitVersion (Mainline Mode)

**Configuration** (`GitVersion.yml`):
- **Mode**: ContinuousDelivery
- **Tag Prefix**: `[vV]` (matches `v1.0.0` or `V1.0.0`)
- **Format**: SemVer 2.0 (MAJOR.MINOR.PATCH)

**Version Calculation**:
1. Reads git tags for explicit versions
2. Parses commit messages for semantic version hints
3. Uses branch name for pre-release tags
4. Automatically increments based on history

### Version Bump Commit Messages

Control version increments with `+semver:` prefix in commit messages:

```bash
# Major version bump (1.0.0 -> 2.0.0)
git commit -m "Breaking change: New API +semver: major"
git commit -m "Refactor configuration model +semver: breaking"

# Minor version bump (1.0.0 -> 1.1.0)
git commit -m "Add AmneziaWG support +semver: minor"
git commit -m "New feature: async methods +semver: feature"

# Patch version bump (1.0.0 -> 1.0.1)
git commit -m "Fix parsing bug +semver: patch"
git commit -m "Bugfix: handle empty lines +semver: fix"

# No version increment
git commit -m "Update documentation +semver: none"
```

### Branch Configuration

- **master**: Production releases (e.g., `1.0.0`)
- **develop**: Alpha releases (e.g., `1.0.0-alpha.5`)
- **feature/**: Beta releases (e.g., `1.0.0-beta.2`)
- **pr/**: Pull request builds (e.g., `1.0.0-pr.123`)

## GitHub Actions Pipeline

**File**: `.github/workflows/ci.yml`

### Job 1: Build and Test

**Triggers**:
- Push to `master` or `develop`
- Pull requests to `master`
- Tags matching `v*` pattern

**Steps**:
1. **Checkout** - Full git history (required for GitVersion)
2. **Setup .NET** - Install .NET 8.0 SDK
3. **Install GitVersion** - Version 6.x
4. **Determine Version** - Run GitVersion to calculate version
5. **Restore Tools** - Install dotnet local tools (CSharpier, etc.)
6. **Restore Dependencies** - `dotnet restore`
7. **Build** - Release configuration with version injection
8. **Validate Formatting** - `dotnet csharpier check .` (fails on unformatted code)
9. **Run Tests** - XPlat code coverage collection
10. **Generate Coverage Report** - ReportGenerator (HTML + Cobertura + Badges)
11. **Check Coverage Threshold** - Fail if < 80% line coverage
12. **Upload Artifacts** - Coverage report and test results

**Environment**:
- OS: `ubuntu-latest`
- DOTNET_VERSION: `8.0.x`
- DOTNET_SKIP_FIRST_TIME_EXPERIENCE: `true`
- DOTNET_CLI_TELEMETRY_OPTOUT: `true`

### Job 2: Release and Publish

**Triggers**:
- Tags matching `v*` pattern only (e.g., `v1.0.0`, `v2.1.3`)

**Depends On**: Build and Test job success

**Steps**:
1. **Checkout** - Full git history
2. **Setup .NET** - Install .NET 8.0 SDK
3. **Install GitVersion** - Version 6.x
4. **Determine Version** - Extract version from tag
5. **Restore Dependencies** - `dotnet restore`
6. **Build** - Release configuration with version
7. **Pack WgConf** - Create NuGet package
8. **Pack WgConf.Amnezia** - Create extension NuGet package
9. **Push to NuGet** - Publish both packages (requires `NUGET_API_KEY`)
10. **Create GitHub Release** - Attach packages, add release notes

**GitHub Release Format**:
```markdown
# WgConf v1.0.0

## NuGet Packages
- [WgConf v1.0.0](https://www.nuget.org/packages/WgConf/1.0.0)
- [WgConf.Amnezia v1.0.0](https://www.nuget.org/packages/WgConf.Amnezia/1.0.0)

## Installation
```bash
dotnet add package WgConf --version 1.0.0
dotnet add package WgConf.Amnezia --version 1.0.0
```
```

**Pre-release Detection**: Automatically marks as pre-release if version contains `-alpha`, `-beta`, etc.

## Required GitHub Secrets

**NUGET_API_KEY**: API key for publishing to NuGet.org
- Get from: https://www.nuget.org/account/apikeys
- Permissions: `Push new packages and package versions`
- Scope: `WgConf` and `WgConf.Amnezia` packages

## Release Process

### Creating a Release

1. **Ensure Quality**
   ```bash
   # Format code
   dotnet csharpier format .

   # Run tests locally
   dotnet test

   # Check coverage (optional)
   dotnet test --collect:"XPlat Code Coverage" --settings .runsettings
   ```

2. **Commit Changes**
   ```bash
   git add .
   git commit -m "Prepare for release +semver: none"
   git push origin master
   ```

3. **Create and Push Tag**
   ```bash
   # Create tag (GitVersion will use this exact version)
   git tag v1.0.0

   # Push tag to trigger release
   git push origin v1.0.0
   ```

4. **Monitor CI/CD**
   - GitHub Actions runs build/test job
   - If successful, runs release job
   - Publishes to NuGet.org
   - Creates GitHub Release with packages

### Pre-release Versions

For alpha/beta releases, use pre-release tags:

```bash
# Alpha release
git tag v1.0.0-alpha.1
git push origin v1.0.0-alpha.1

# Beta release
git tag v1.0.0-beta.1
git push origin v1.0.0-beta.1

# Release candidate
git tag v1.0.0-rc.1
git push origin v1.0.0-rc.1
```

Pre-release packages are published to NuGet but marked as pre-release.

## Local Testing of Release

Test the package creation locally before releasing:

```bash
# Install GitVersion tool globally
dotnet tool install --global GitVersion.Tool

# Calculate version
dotnet-gitversion

# Build with specific version
dotnet build --configuration Release /p:Version=1.0.0-local

# Pack packages
dotnet pack src/WgConf/WgConf.csproj \
  --configuration Release \
  --output ./nupkgs \
  /p:Version=1.0.0-local

dotnet pack src/WgConf.Amnezia/WgConf.Amnezia.csproj \
  --configuration Release \
  --output ./nupkgs \
  /p:Version=1.0.0-local

# Verify package integrity
dotnet nuget verify ./nupkgs/*.nupkg

# Test package locally
dotnet add package WgConf --source ./nupkgs --version 1.0.0-local
```

## Package Configuration

### WgConf Package

**File**: `src/WgConf/WgConf.csproj`

```xml
<PackageId>WgConf</PackageId>
<Description>.NET library for reading and writing WireGuard configuration files</Description>
<PackageTags>wireguard;wg;configuration;parser</PackageTags>
<PackageReadmeFile>README.md</PackageReadmeFile>
```

### WgConf.Amnezia Package

**File**: `src/WgConf.Amnezia/WgConf.Amnezia.csproj`

```xml
<PackageId>WgConf.Amnezia</PackageId>
<Description>WgConf extension for AmneziaWG obfuscation parameters</Description>
<PackageTags>wireguard;wg;amneziawg;configuration;parser</PackageTags>
<PackageReadmeFile>README.md</PackageReadmeFile>
```

**Shared Properties** (from `Directory.Build.props`):
- Authors: 2CHEVSKII
- Copyright: Copyright (c) 2026 2CHEVSKII
- PackageLicenseExpression: MIT
- RepositoryUrl: https://github.com/dvchevskii/wgconf
- RepositoryType: git
- PublishRepositoryUrl: true
- IncludeSymbols: true
- SymbolPackageFormat: snupkg
- EmbedUntrackedSources: true

## Troubleshooting

### Coverage Fails Locally
```bash
# Ensure .runsettings is used
dotnet test --settings .runsettings

# Check coverage threshold
dotnet reportgenerator \
  -reports:TestResults/*/coverage.opencover.xml \
  -targetdir:./coverage-report \
  -reporttypes:"Html"
```

### GitVersion Fails
```bash
# Ensure git history is available (not shallow clone)
git fetch --unshallow

# Ensure tags are present
git fetch --tags

# Debug GitVersion
dotnet-gitversion /diag
```

### Formatting Check Fails
```bash
# Auto-fix formatting issues
dotnet csharpier format .

# Check what would change
dotnet csharpier check .
```

### NuGet Push Fails
- Verify `NUGET_API_KEY` secret exists in repository settings
- Check API key has push permissions
- Ensure package version doesn't already exist on NuGet.org
- Check package ID matches expected format

### Release Job Skipped
- Ensure tag matches `v*` pattern (e.g., `v1.0.0`, not `1.0.0`)
- Check that build/test job completed successfully
- Verify tag was pushed to remote: `git ls-remote --tags origin`

## CI Environment Variables

**Set Automatically by CI**:
- `CI=true` - Enables deterministic builds
- `ContinuousIntegrationBuild=true` - SourceLink integration
- `GITHUB_SHA` - Commit hash
- `GITHUB_REF` - Branch or tag reference
- `GITHUB_RUN_NUMBER` - Build number

**Set in Workflow**:
- `DOTNET_VERSION` - .NET SDK version
- `DOTNET_SKIP_FIRST_TIME_EXPERIENCE` - Skip first-run experience
- `DOTNET_CLI_TELEMETRY_OPTOUT` - Disable telemetry
