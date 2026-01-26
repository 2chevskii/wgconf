namespace WgConf.Tests;

public class WireguardConfigurationTests
{
    private const string ValidConfigText = """
        [Interface]
        PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
        ListenPort = 51820
        Address = 10.0.0.1/24

        [Peer]
        PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
        AllowedIPs = 10.0.0.2/32
        """;

    private const string CompleteConfigText = """
        [Interface]
        PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
        ListenPort = 51820
        Address = 10.0.0.1/24
        PreUp = echo 'Starting WireGuard'
        PostUp = iptables -A FORWARD -i wg0 -j ACCEPT
        PreDown = echo 'Stopping WireGuard'
        PostDown = iptables -D FORWARD -i wg0 -j ACCEPT

        [Peer]
        PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
        AllowedIPs = 10.0.0.2/32
        Endpoint = vpn.example.com:51820
        PresharedKey = FpCyhws9cxwWoV4xELtfJvjJN+zQVRPISllRWgeopVE=
        PersistedKeepalive = 25
        """;

    private const string InvalidConfigText = """
        [Interface]
        PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
        ListenPort = 51820
        Address = 10.0.0.1/24
        InvalidProperty = SomeValue
        """;

    #region Parse Tests

    [Fact]
    public void Parse_ValidConfiguration_ReturnsConfiguration()
    {
        var config = WireguardConfiguration.Parse(ValidConfigText);

        Assert.NotNull(config);
        Assert.Equal(
            Convert.FromBase64String("YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY="),
            config.PrivateKey
        );
        Assert.Equal(51820, config.ListenPort);
        Assert.Equal("10.0.0.1/24", config.Address.ToString());
        Assert.Single(config.Peers);
    }

    [Fact]
    public void Parse_CompleteConfiguration_ParsesAllProperties()
    {
        var config = WireguardConfiguration.Parse(CompleteConfigText);

        Assert.Equal("echo 'Starting WireGuard'", config.PreUp);
        Assert.Equal("iptables -A FORWARD -i wg0 -j ACCEPT", config.PostUp);
        Assert.Equal("echo 'Stopping WireGuard'", config.PreDown);
        Assert.Equal("iptables -D FORWARD -i wg0 -j ACCEPT", config.PostDown);

        var peer = config.Peers[0];
        Assert.NotNull(peer.Endpoint);
        Assert.Equal("vpn.example.com:51820", peer.Endpoint.Value.ToString());
        Assert.NotNull(peer.PresharedKey);
        Assert.Equal(25, peer.PersistedKeepalive);
    }

    [Fact]
    public void Parse_InvalidConfiguration_ThrowsException()
    {
        var ex = Assert.Throws<WireguardConfigurationException>(() =>
            WireguardConfiguration.Parse(InvalidConfigText)
        );
        Assert.NotEmpty(ex.Errors);
        Assert.Contains(ex.Errors, e => e.Message.Contains("InvalidProperty"));
    }

    [Fact]
    public void Parse_EmptyString_ThrowsException()
    {
        Assert.Throws<WireguardConfigurationException>(() =>
            WireguardConfiguration.Parse(string.Empty)
        );
    }

    [Fact]
    public void Parse_MissingRequiredProperty_ThrowsException()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            """;

        Assert.Throws<WireguardConfigurationException>(() => WireguardConfiguration.Parse(input));
    }

    #endregion

    #region TryParse Tests

    [Fact]
    public void TryParse_ValidConfiguration_ReturnsTrueAndConfiguration()
    {
        var success = WireguardConfiguration.TryParse(
            ValidConfigText,
            out var config,
            out var errors
        );

        Assert.True(success);
        Assert.NotNull(config);
        Assert.Empty(errors);
        Assert.Equal(51820, config.ListenPort);
    }

    [Fact]
    public void TryParse_CompleteConfiguration_ParsesAllProperties()
    {
        var success = WireguardConfiguration.TryParse(
            CompleteConfigText,
            out var config,
            out var errors
        );

        Assert.True(success);
        Assert.NotNull(config);
        Assert.Empty(errors);
        Assert.Equal("echo 'Starting WireGuard'", config.PreUp);
        Assert.Equal("iptables -A FORWARD -i wg0 -j ACCEPT", config.PostUp);
    }

    [Fact]
    public void TryParse_InvalidConfiguration_ReturnsFalseWithErrors()
    {
        var success = WireguardConfiguration.TryParse(
            InvalidConfigText,
            out var config,
            out var errors
        );

        Assert.False(success);
        Assert.Null(config);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Message.Contains("InvalidProperty"));
    }

    [Fact]
    public void TryParse_MultipleErrors_ReturnsAllErrors()
    {
        var input = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            InvalidProperty1 = Value1
            InvalidProperty2 = Value2
            """;

        var success = WireguardConfiguration.TryParse(input, out var config, out var errors);

        Assert.False(success);
        Assert.Null(config);
        Assert.Equal(2, errors.Count);
    }

    [Fact]
    public void TryParse_EmptyString_ReturnsFalseWithErrors()
    {
        var success = WireguardConfiguration.TryParse(string.Empty, out var config, out var errors);

        Assert.False(success);
        Assert.Null(config);
        Assert.NotEmpty(errors);
    }

    #endregion

    #region Load Tests

    [Fact]
    public void Load_ValidFile_ReturnsConfiguration()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, ValidConfigText);

            var config = WireguardConfiguration.Load(tempFile);

            Assert.NotNull(config);
            Assert.Equal(51820, config.ListenPort);
            Assert.Single(config.Peers);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Load_CompleteConfiguration_ParsesAllProperties()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, CompleteConfigText);

            var config = WireguardConfiguration.Load(tempFile);

            Assert.Equal("echo 'Starting WireGuard'", config.PreUp);
            Assert.Equal("iptables -A FORWARD -i wg0 -j ACCEPT", config.PostUp);
            Assert.Single(config.Peers);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Load_InvalidFile_ThrowsException()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, InvalidConfigText);

            Assert.Throws<WireguardConfigurationException>(() =>
                WireguardConfiguration.Load(tempFile)
            );
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Load_NonExistentFile_ThrowsException()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        Assert.Throws<FileNotFoundException>(() => WireguardConfiguration.Load(nonExistentPath));
    }

    #endregion

    #region LoadAsync Tests

    [Fact]
    public async Task LoadAsync_ValidFile_ReturnsConfiguration()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(
                tempFile,
                ValidConfigText,
                TestContext.Current.CancellationToken
            );

            var config = await WireguardConfiguration.LoadAsync(
                tempFile,
                TestContext.Current.CancellationToken
            );

            Assert.NotNull(config);
            Assert.Equal(51820, config.ListenPort);
            Assert.Single(config.Peers);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsync_CompleteConfiguration_ParsesAllProperties()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(
                tempFile,
                CompleteConfigText,
                TestContext.Current.CancellationToken
            );

            var config = await WireguardConfiguration.LoadAsync(
                tempFile,
                TestContext.Current.CancellationToken
            );

            Assert.Equal("echo 'Starting WireGuard'", config.PreUp);
            Assert.Equal("iptables -A FORWARD -i wg0 -j ACCEPT", config.PostUp);
            Assert.Single(config.Peers);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsync_InvalidFile_ThrowsException()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(
                tempFile,
                InvalidConfigText,
                TestContext.Current.CancellationToken
            );

            await Assert.ThrowsAsync<WireguardConfigurationException>(async () =>
                await WireguardConfiguration.LoadAsync(
                    tempFile,
                    TestContext.Current.CancellationToken
                )
            );
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsync_NonExistentFile_ThrowsException()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await WireguardConfiguration.LoadAsync(
                nonExistentPath,
                TestContext.Current.CancellationToken
            )
        );
    }

    [Fact]
    public async Task LoadAsync_WithCancellationToken_RespectsCancellation()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(
                tempFile,
                ValidConfigText,
                TestContext.Current.CancellationToken
            );
            var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await WireguardConfiguration.LoadAsync(tempFile, cts.Token)
            );
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    #endregion

    #region Save Tests

    [Fact]
    public void Save_ValidConfiguration_WritesFile()
    {
        var config = WireguardConfiguration.Parse(ValidConfigText);
        var tempFile = Path.GetTempFileName();
        try
        {
            config.Save(tempFile);

            Assert.True(File.Exists(tempFile));
            var written = File.ReadAllText(tempFile);
            Assert.NotEmpty(written);

            var reloaded = WireguardConfiguration.Load(tempFile);
            Assert.Equal(config.PrivateKey, reloaded.PrivateKey);
            Assert.Equal(config.ListenPort, reloaded.ListenPort);
            Assert.Equal(config.Address.ToString(), reloaded.Address.ToString());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Save_CompleteConfiguration_PreservesAllProperties()
    {
        var config = WireguardConfiguration.Parse(CompleteConfigText);
        var tempFile = Path.GetTempFileName();
        try
        {
            config.Save(tempFile);

            var reloaded = WireguardConfiguration.Load(tempFile);
            Assert.Equal(config.PreUp, reloaded.PreUp);
            Assert.Equal(config.PostUp, reloaded.PostUp);
            Assert.Equal(config.PreDown, reloaded.PreDown);
            Assert.Equal(config.PostDown, reloaded.PostDown);
            Assert.Equal(config.Peers.Count, reloaded.Peers.Count);
            Assert.Equal(
                config.Peers[0].Endpoint?.ToString(),
                reloaded.Peers[0].Endpoint?.ToString()
            );
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Save_OverwritesExistingFile()
    {
        var config1 = WireguardConfiguration.Parse(ValidConfigText);
        var config2Text = """
            [Interface]
            PrivateKey = TrMvSoP4jYQlY6RIzBgbssQqY3vxI2Pi+y71lOWWXX0=
            ListenPort = 51821
            Address = 10.0.0.2/24
            """;
        var config2 = WireguardConfiguration.Parse(config2Text);

        var tempFile = Path.GetTempFileName();
        try
        {
            config1.Save(tempFile);
            config2.Save(tempFile);

            var reloaded = WireguardConfiguration.Load(tempFile);
            Assert.Equal(51821, reloaded.ListenPort);
            Assert.Equal("10.0.0.2/24", reloaded.Address.ToString());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Save_ToInvalidPath_ThrowsException()
    {
        var config = WireguardConfiguration.Parse(ValidConfigText);
        var invalidPath = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString(),
            "nonexistent",
            "config.conf"
        );

        Assert.Throws<DirectoryNotFoundException>(() => config.Save(invalidPath));
    }

    #endregion

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_ValidConfiguration_WritesFile()
    {
        var config = WireguardConfiguration.Parse(ValidConfigText);
        var tempFile = Path.GetTempFileName();
        try
        {
            await config.SaveAsync(tempFile, TestContext.Current.CancellationToken);

            Assert.True(File.Exists(tempFile));
            var written = await File.ReadAllTextAsync(
                tempFile,
                TestContext.Current.CancellationToken
            );
            Assert.NotEmpty(written);

            var reloaded = await WireguardConfiguration.LoadAsync(
                tempFile,
                TestContext.Current.CancellationToken
            );
            Assert.Equal(config.PrivateKey, reloaded.PrivateKey);
            Assert.Equal(config.ListenPort, reloaded.ListenPort);
            Assert.Equal(config.Address.ToString(), reloaded.Address.ToString());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveAsync_CompleteConfiguration_PreservesAllProperties()
    {
        var config = WireguardConfiguration.Parse(CompleteConfigText);
        var tempFile = Path.GetTempFileName();
        try
        {
            await config.SaveAsync(tempFile, TestContext.Current.CancellationToken);

            var reloaded = await WireguardConfiguration.LoadAsync(
                tempFile,
                TestContext.Current.CancellationToken
            );
            Assert.Equal(config.PreUp, reloaded.PreUp);
            Assert.Equal(config.PostUp, reloaded.PostUp);
            Assert.Equal(config.PreDown, reloaded.PreDown);
            Assert.Equal(config.PostDown, reloaded.PostDown);
            Assert.Equal(config.Peers.Count, reloaded.Peers.Count);
            Assert.Equal(
                config.Peers[0].Endpoint?.ToString(),
                reloaded.Peers[0].Endpoint?.ToString()
            );
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveAsync_OverwritesExistingFile()
    {
        var config1 = WireguardConfiguration.Parse(ValidConfigText);
        var config2Text = """
            [Interface]
            PrivateKey = TrMvSoP4jYQlY6RIzBgbssQqY3vxI2Pi+y71lOWWXX0=
            ListenPort = 51821
            Address = 10.0.0.2/24
            """;
        var config2 = WireguardConfiguration.Parse(config2Text);

        var tempFile = Path.GetTempFileName();
        try
        {
            await config1.SaveAsync(tempFile, TestContext.Current.CancellationToken);
            await config2.SaveAsync(tempFile, TestContext.Current.CancellationToken);

            var reloaded = await WireguardConfiguration.LoadAsync(
                tempFile,
                TestContext.Current.CancellationToken
            );
            Assert.Equal(51821, reloaded.ListenPort);
            Assert.Equal("10.0.0.2/24", reloaded.Address.ToString());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveAsync_ToInvalidPath_ThrowsException()
    {
        var config = WireguardConfiguration.Parse(ValidConfigText);
        var invalidPath = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString(),
            "nonexistent",
            "config.conf"
        );

        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () =>
            await config.SaveAsync(invalidPath, TestContext.Current.CancellationToken)
        );
    }

    [Fact]
    public async Task SaveAsync_WithCancellationToken_RespectsCancellation()
    {
        var config = WireguardConfiguration.Parse(ValidConfigText);
        var tempFile = Path.GetTempFileName();
        try
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await config.SaveAsync(tempFile, cts.Token)
            );
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion

    #region Round-Trip Integration Tests

    [Fact]
    public void ParseAndSave_RoundTrip_PreservesData()
    {
        var original = WireguardConfiguration.Parse(CompleteConfigText);
        var tempFile = Path.GetTempFileName();
        try
        {
            original.Save(tempFile);
            var reloaded = WireguardConfiguration.Load(tempFile);

            AssertConfigurationsEqual(original, reloaded);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ParseAndSaveAsync_RoundTrip_PreservesData()
    {
        var original = WireguardConfiguration.Parse(CompleteConfigText);
        var tempFile = Path.GetTempFileName();
        try
        {
            await original.SaveAsync(tempFile, TestContext.Current.CancellationToken);
            var reloaded = await WireguardConfiguration.LoadAsync(
                tempFile,
                TestContext.Current.CancellationToken
            );

            AssertConfigurationsEqual(original, reloaded);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAndSaveAsync_MixedSync_WorksCorrectly()
    {
        var tempFile1 = Path.GetTempFileName();
        var tempFile2 = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(
                tempFile1,
                CompleteConfigText,
                TestContext.Current.CancellationToken
            );

            var config = WireguardConfiguration.Load(tempFile1);
            await config.SaveAsync(tempFile2, TestContext.Current.CancellationToken);

            var reloaded = await WireguardConfiguration.LoadAsync(
                tempFile2,
                TestContext.Current.CancellationToken
            );
            AssertConfigurationsEqual(config, reloaded);
        }
        finally
        {
            File.Delete(tempFile1);
            File.Delete(tempFile2);
        }
    }

    #endregion

    private static void AssertConfigurationsEqual(
        WireguardConfiguration expected,
        WireguardConfiguration actual
    )
    {
        Assert.Equal(expected.PrivateKey, actual.PrivateKey);
        Assert.Equal(expected.ListenPort, actual.ListenPort);
        Assert.Equal(expected.Address.ToString(), actual.Address.ToString());
        Assert.Equal(expected.PreUp, actual.PreUp);
        Assert.Equal(expected.PostUp, actual.PostUp);
        Assert.Equal(expected.PreDown, actual.PreDown);
        Assert.Equal(expected.PostDown, actual.PostDown);
        Assert.Equal(expected.Peers.Count, actual.Peers.Count);

        for (int i = 0; i < expected.Peers.Count; i++)
        {
            Assert.Equal(expected.Peers[i].PublicKey, actual.Peers[i].PublicKey);
            Assert.Equal(expected.Peers[i].AllowedIPs.Count, actual.Peers[i].AllowedIPs.Count);
            Assert.Equal(
                expected.Peers[i].Endpoint?.ToString(),
                actual.Peers[i].Endpoint?.ToString()
            );
            Assert.Equal(expected.Peers[i].PresharedKey, actual.Peers[i].PresharedKey);
            Assert.Equal(expected.Peers[i].PersistedKeepalive, actual.Peers[i].PersistedKeepalive);
        }
    }
}
