namespace WgConf.Tests;

public class WireguardConfigurationValidationTests
{
    #region PrivateKey Validation Tests

    [Fact]
    public void Read_MissingPrivateKey_ThrowsWithError()
    {
        var input = """
            [Interface]
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(
            ex.Errors,
            e => e.Message.Contains("PrivateKey") && e.Message.Contains("required")
        );
    }

    [Fact]
    public void Read_InvalidPrivateKeyLength_ThrowsWithError()
    {
        // Base64 string that decodes to less than 32 bytes
        var input = """
            [Interface]
            PrivateKey = c2hvcnRrZXk=
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(
            ex.Errors,
            e => e.Message.Contains("PrivateKey") && e.Message.Contains("32 bytes")
        );
    }

    #endregion

    #region ListenPort Validation Tests

    [Fact]
    public void Read_MissingListenPort_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            Address = 10.0.0.1/24
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(
            ex.Errors,
            e => e.Message.Contains("ListenPort") && e.Message.Contains("required")
        );
    }

    [Fact]
    public void Read_PortTooLow_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 0
            Address = 10.0.0.1/24
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(
            ex.Errors,
            e =>
                e.Message.Contains("ListenPort")
                && e.Message.Contains("1")
                && e.Message.Contains("65535")
        );
    }

    [Fact]
    public void Read_PortTooHigh_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 65536
            Address = 10.0.0.1/24
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(
            ex.Errors,
            e =>
                e.Message.Contains("ListenPort")
                && e.Message.Contains("1")
                && e.Message.Contains("65535")
        );
    }

    [Fact]
    public void Read_PortBoundary1_IsValid()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 1
            Address = 10.0.0.1/24
            """;

        var config = ReadConfiguration(input);
        Assert.Equal(1, config.ListenPort);
    }

    [Fact]
    public void Read_PortBoundary65535_IsValid()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 65535
            Address = 10.0.0.1/24
            """;

        var config = ReadConfiguration(input);
        Assert.Equal(65535, config.ListenPort);
    }

    #endregion

    #region Peer PublicKey Validation Tests

    [Fact]
    public void Read_PeerMissingPublicKey_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            AllowedIPs = 10.0.0.2/32
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(
            ex.Errors,
            e => e.Message.Contains("PublicKey") && e.Message.Contains("required")
        );
    }

    [Fact]
    public void Read_PeerInvalidPublicKeyLength_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = c2hvcnQ=
            AllowedIPs = 10.0.0.2/32
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(
            ex.Errors,
            e => e.Message.Contains("PublicKey") && e.Message.Contains("32 bytes")
        );
    }

    [Fact]
    public void Read_PeerInvalidPublicKeyBase64_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = not-valid-base64!!!
            AllowedIPs = 10.0.0.2/32
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(ex.Errors, e => e.Message.Contains("PublicKey"));
    }

    #endregion

    #region Peer AllowedIPs Validation Tests

    [Fact]
    public void Read_PeerMissingAllowedIPs_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(
            ex.Errors,
            e => e.Message.Contains("AllowedIPs") && e.Message.Contains("required")
        );
    }

    [Fact]
    public void Read_PeerInvalidAllowedIPsCIDR_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = invalid-cidr
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(ex.Errors, e => e.Message.Contains("AllowedIPs"));
    }

    #endregion

    #region PresharedKey Validation Tests

    [Fact]
    public void Read_PeerInvalidPresharedKeyLength_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = 10.0.0.2/32
            PresharedKey = c2hvcnQ=
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(
            ex.Errors,
            e => e.Message.Contains("PresharedKey") && e.Message.Contains("32 bytes")
        );
    }

    [Fact]
    public void Read_PeerInvalidPresharedKeyBase64_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = 10.0.0.2/32
            PresharedKey = not-valid-base64!!!
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(ex.Errors, e => e.Message.Contains("PresharedKey"));
    }

    #endregion

    #region Endpoint Validation Tests

    [Fact]
    public void Read_PeerInvalidEndpoint_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = 10.0.0.2/32
            Endpoint = invalid-endpoint-format
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(ex.Errors, e => e.Message.Contains("Endpoint"));
    }

    #endregion

    #region PersistedKeepalive Validation Tests

    [Fact]
    public void Read_PeerInvalidPersistedKeepalive_ThrowsWithError()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24

            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = 10.0.0.2/32
            PersistedKeepalive = not-a-number
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        Assert.Contains(ex.Errors, e => e.Message.Contains("PersistedKeepalive"));
    }

    #endregion

    #region ParseError Tests

    [Fact]
    public void ParseError_ToString_FormatsCorrectly()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            InvalidProperty = Value
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        var error = ex.Errors[0];

        var errorString = error.ToString();
        Assert.Contains(error.LineNumber.ToString(), errorString);
        Assert.Contains(error.Message, errorString);
    }

    [Fact]
    public void ParseError_LineText_IsAccessible()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            InvalidProperty = Value
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        var error = ex.Errors[0];

        // Access LineText property to get coverage
        var lineText = error.LineText;
        Assert.Equal("InvalidProperty = Value", lineText);
    }

    [Fact]
    public void ParseError_Section_ReflectsCurrentSection()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            InvalidProperty = Value
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        var error = ex.Errors[0];

        Assert.Equal("Interface", error.Section);
    }

    [Fact]
    public void ParseError_PropertyName_ReflectsInvalidProperty()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            InvalidProperty = Value
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        var error = ex.Errors[0];

        Assert.Equal("InvalidProperty", error.PropertyName);
    }

    [Fact]
    public void ParseError_SurroundingLines_ProvideContext()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            InvalidProperty = Value
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));
        var error = ex.Errors[0];

        Assert.NotNull(error.SurroundingLines);
        Assert.NotEmpty(error.SurroundingLines);
    }

    #endregion

    #region Multiple Errors Tests

    [Fact]
    public void Read_MultipleValidationErrors_CollectsAllErrors()
    {
        var input = """
            [Interface]
            PrivateKey = c2hvcnQ=
            ListenPort = 70000
            Address = invalid-cidr

            [Peer]
            AllowedIPs = 10.0.0.2/32
            """;

        var ex = Assert.Throws<WireguardConfigurationException>(() => ReadConfiguration(input));

        // Should have errors for: invalid PrivateKey, invalid ListenPort, invalid Address, missing PublicKey
        Assert.True(ex.Errors.Count >= 4);
    }

    #endregion

    private static WireguardConfiguration ReadConfiguration(string input)
    {
        using var reader = new WireguardConfigurationReader(new StringReader(input));
        return reader.Read();
    }
}
