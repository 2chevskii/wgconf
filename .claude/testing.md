# Testing

## Test Structure

### WgConf.Tests (81 tests, 2,187 lines)

**Test Classes**:
1. **CIDRTests.cs** (120 lines)
   - Parse valid/invalid IP addresses with prefix
   - Prefix length validation (IPv4: 0-32, IPv6: 0-128)
   - ToString() formatting
   - Round-trip serialization

2. **WireguardConfigurationReaderTests.cs** (439 lines)
   - Valid configuration parsing
   - Comment handling (full-line and inline `#`)
   - Case-insensitive property names
   - Duplicate properties (last-wins behavior)
   - Error cases: unknown properties/sections, invalid formats
   - TryRead pattern
   - Round-trip with writer

3. **WireguardConfigurationReaderAsyncTests.cs** (133 lines)
   - ReadAsync() method
   - TryReadAsync() method
   - Async I/O patterns

4. **WireguardConfigurationWriterTests.cs** (297 lines)
   - Minimal configuration output
   - Complete configuration with all optional properties
   - Multiple peers
   - IPv4 and IPv6 support
   - Property formatting and ordering

5. **WireguardConfigurationTests.cs** (652 lines)
   - Factory methods: Parse(), TryParse()
   - File I/O: Load(), LoadAsync(), Save(), SaveAsync()
   - Integration with reader/writer
   - Round-trip serialization

6. **WireguardConfigurationValidationTests.cs** (449 lines)
   - WireGuard key validation (exactly 32 bytes)
   - Port range validation (1-65535)
   - Error message quality
   - ParseError context (line numbers, surrounding lines)

7. **WireguardEndpointTests.cs** (120 lines)
   - Hostname parsing
   - IPv4 endpoint parsing
   - IPv6 endpoint parsing (with brackets)
   - Port validation
   - TryParse pattern
   - ToString() formatting
   - Round-trip serialization

### WgConf.Amnezia.Tests (52 tests, 926 lines)

**Test Classes**:
1. **IntegerRangeTests.cs** (213 lines, 24 tests)
   - Parse valid ranges (e.g., "25-30")
   - Parse single values (e.g., "25")
   - Parse single negative values (e.g., "-10")
   - Parse invalid values
   - Negative number support in ranges
   - TryParse pattern for both formats
   - ToString() formatting for single values and ranges
   - Round-trip serialization for both formats

2. **AmneziaWgConfigurationReaderTests.cs** (386 lines, 16 tests)
   - Parse minimal AmneziaWG configs
   - Parse integer properties (Jc, Jmin, etc.)
   - Parse string properties (I1-I5)
   - Parse range properties (H1-H4) with both single values and ranges
   - Parse mixed single value and range properties
   - Complete configuration parsing
   - Case-insensitivity
   - Error handling for invalid properties
   - TryRead pattern
   - Async methods

3. **AmneziaWgConfigurationWriterTests.cs** (327 lines, 12 tests)
   - Write minimal AmneziaWG configs
   - Write complete configs with all properties
   - Integer/string/range property serialization
   - Single value range property serialization
   - Mixed single value and range property serialization
   - Null optional property handling
   - Property ordering (standard WireGuard first, then AmneziaWG)
   - Round-trip with reader

## Test Commands

```bash
# Run all tests
dotnet test

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage" --settings .runsettings

# Run tests for specific project
dotnet test test/WgConf.Tests/WgConf.Tests.csproj
dotnet test test/WgConf.Amnezia.Tests/WgConf.Amnezia.Tests.csproj

# Run tests with verbose output
dotnet test --verbosity detailed

# Run specific test by name filter
dotnet test --filter "FullyQualifiedName~WireguardConfigurationReaderTests"
```

## Coverage Configuration (`.runsettings`)

### Settings
- **Format**: OpenCover XML
- **Output**: `./TestResults` directory
- **Collectors**: XPlat Code Coverage (coverlet)
- **Threshold**: 80% line coverage (enforced in CI)

### Exclusions
- Test assemblies (`*.Tests`)
- Compiler-generated code
- Generated code files

### Reports Generated (CI)
- **OpenCover XML**: For coverage analysis tools
- **Cobertura XML**: For CI/CD integrations
- **HTML Report**: Human-readable with ReportGenerator
- **Badges**: SVG badges for README

### Coverage Enforcement
CI pipeline fails if line coverage < 80%:
```bash
# CI validation command
dotnet test --collect:"XPlat Code Coverage" --settings .runsettings
dotnet reportgenerator -reports:TestResults/*/coverage.opencover.xml \
  -targetdir:./coverage-report -reporttypes:"Html;Cobertura;Badges"

# Check threshold (custom script in CI)
# Fails build if coverage < 80%
```

## Test Patterns

### Naming Convention
```
MethodName_Scenario_ExpectedBehavior
```

**Examples**:
- `Parse_ValidIPv4WithPrefix_ReturnsCorrectCIDR`
- `Read_InvalidPrivateKey_ThrowsWireguardConfigurationException`
- `TryParse_InvalidEndpoint_ReturnsFalse`

### Structure (Arrange-Act-Assert)
```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var input = "test data";

    // Act
    var result = SomeMethod(input);

    // Assert
    Assert.Equal(expected, result);
}
```

### Theory Tests (Data-Driven)
```csharp
[Theory]
[InlineData("10.0.0.1/24", "10.0.0.1", 24)]
[InlineData("192.168.1.1/16", "192.168.1.1", 16)]
public void Parse_ValidCIDR_ReturnsCorrectComponents(
    string input, string expectedIp, int expectedPrefix)
{
    // Test implementation
}
```

### Async Tests
```csharp
[Fact]
public async Task ReadAsync_ValidConfiguration_ReturnsConfiguration()
{
    // Arrange
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(config));
    using var reader = new StreamReader(stream);
    var configReader = new WireguardConfigurationReader(reader);

    // Act
    var result = await configReader.ReadAsync();

    // Assert
    Assert.NotNull(result);
}
```

### Error Testing
```csharp
[Fact]
public void Read_InvalidConfiguration_ThrowsExceptionWithDetails()
{
    // Arrange
    var invalidConfig = "[Interface]\nInvalidProperty = value";
    using var reader = new StringReader(invalidConfig);
    var configReader = new WireguardConfigurationReader(reader);

    // Act & Assert
    var exception = Assert.Throws<WireguardConfigurationException>(
        () => configReader.Read()
    );
    Assert.Contains("InvalidProperty", exception.Message);
    Assert.Single(exception.Errors);
}
```

### Round-Trip Testing
```csharp
[Fact]
public void Writer_CompleteConfiguration_RoundTripsCorrectly()
{
    // Arrange
    var original = CreateCompleteConfiguration();

    // Act - Write
    var written = WriteToString(original);

    // Act - Read back
    var parsed = ParseFromString(written);

    // Assert
    AssertConfigurationsEqual(original, parsed);
}
```

## Test Data Management

### Inline Test Data
Use `[InlineData]` for simple test cases:
```csharp
[Theory]
[InlineData("valid-input", true)]
[InlineData("invalid-input", false)]
public void TestMethod(string input, bool expected)
{
    // Test logic
}
```

### Complex Test Data
Use `[MemberData]` or `[ClassData]` for complex scenarios:
```csharp
public static IEnumerable<object[]> GetValidConfigurations()
{
    yield return new object[] { CreateMinimalConfig() };
    yield return new object[] { CreateCompleteConfig() };
}

[Theory]
[MemberData(nameof(GetValidConfigurations))]
public void TestMethod(WireguardConfiguration config)
{
    // Test logic
}
```

## Test Isolation

- Each test is independent (no shared state)
- Use `using` statements for `IDisposable` test fixtures
- Avoid static state
- Create fresh instances per test

## Coverage Goals

- **Line Coverage**: 80% minimum (enforced)
- **Branch Coverage**: Best effort
- **Focus Areas**:
  - All public APIs
  - Error handling paths
  - Edge cases (null, empty, invalid input)
  - Round-trip serialization
  - Async variants
