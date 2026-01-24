# CLAUDE.md

**WgConf**: .NET library for WireGuard configuration files with strongly-typed models.

## Quick Reference

- **Framework**: .NET 8.0 LTS
- **SDK**: 10.0.102 (global.json)
- **Solution**: XML `.slnx` format
- **Formatter**: CSharpier 1.2.5

### Commands

```bash
dotnet build                    # Build
dotnet test                     # Run tests
dotnet csharpier format .       # Format code
```

### Projects

- **WgConf**: Base library (Interface/Peer configs, CIDR, Endpoint, Reader/Writer)
- **WgConf.Amnezia**: Extension with 20 AmneziaWG obfuscation parameters (H1-H4 support single values and ranges)

### Documentation

- [Architecture](.claude/architecture.md) - Domain models, reader/writer patterns, extensibility
- [Development](.claude/development.md) - Build setup, tooling, code style
- [Testing](.claude/testing.md) - Test structure, coverage (80% threshold)
- [CI/CD](.claude/cicd.md) - Pipeline, versioning, release process
- [Project Structure](.claude/project-structure.md) - File organization, dependencies

### Key Patterns

- **Dual API**: `Read()`/`TryRead()` (throw vs bool return)
- **Error handling**: Structured `ParseError` with context aggregation
- **Extensibility**: `protected virtual` methods for custom readers/writers
- **Resource safety**: `IDisposable`/`IAsyncDisposable` on writers

### Release Process

1. Commit changes to master
2. `git tag v1.0.0 && git push origin v1.0.0`
3. CI builds, tests, packs, publishes to NuGet automatically

Use `+semver: {major|minor|patch|none}` in commit messages to control version bumps.
