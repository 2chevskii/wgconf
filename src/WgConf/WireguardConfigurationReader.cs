using System.Globalization;

namespace WgConf;

public class WireguardConfigurationReader(TextReader textReader) : IDisposable, IAsyncDisposable
{
    protected readonly TextReader _textReader = textReader;

    public void Dispose()
    {
        _textReader.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_textReader is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            _textReader.Dispose();
        }
    }

    public virtual WireguardConfiguration Read()
    {
        if (TryRead(out var config, out var errors))
        {
            return config!;
        }

        throw new WireguardConfigurationException(errors);
    }

    public virtual async Task<WireguardConfiguration> ReadAsync(
        CancellationToken cancellationToken = default
    )
    {
        var (config, errors) = await TryReadAsync(cancellationToken);
        if (config != null)
        {
            return config;
        }

        throw new WireguardConfigurationException(errors);
    }

    public bool TryRead(
        out WireguardConfiguration? configuration,
        out IReadOnlyList<ParseError> errors
    )
    {
        var errorList = new List<ParseError>();
        var allLines = new List<string>();
        var lineNumber = 0;

        string? line;
        while ((line = _textReader.ReadLine()) != null)
        {
            lineNumber++;
            allLines.Add(line);
        }

        return ProcessLines(allLines, lineNumber, errorList, out configuration, out errors);
    }

    public async Task<(
        WireguardConfiguration? Configuration,
        IReadOnlyList<ParseError> Errors
    )> TryReadAsync(CancellationToken cancellationToken = default)
    {
        var errorList = new List<ParseError>();
        var allLines = new List<string>();
        var lineNumber = 0;

        string? line;
        while ((line = await _textReader.ReadLineAsync(cancellationToken)) != null)
        {
            lineNumber++;
            allLines.Add(line);
        }

        ProcessLines(allLines, lineNumber, errorList, out var configuration, out var errors);
        return (configuration, errors);
    }

    private bool ProcessLines(
        List<string> allLines,
        int lineNumber,
        List<ParseError> errorList,
        out WireguardConfiguration? configuration,
        out IReadOnlyList<ParseError> errors
    )
    {
        try
        {
            var parsedConfig = ParseConfiguration(allLines, errorList);
            errors = errorList;

            if (errorList.Count == 0)
            {
                configuration = parsedConfig;
                return true;
            }
            else
            {
                configuration = null;
                return false;
            }
        }
        catch (Exception ex) when (ex is not WireguardConfigurationException)
        {
            errorList.Add(
                new ParseError(lineNumber, $"Unexpected error during parsing: {ex.Message}")
            );
            configuration = null;
            errors = errorList;
            return false;
        }
    }

    private WireguardConfiguration ParseConfiguration(
        List<string> allLines,
        List<ParseError> errors
    )
    {
        var interfaceProps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var peers = new List<Dictionary<string, string>>();
        Dictionary<string, string>? currentPeerProps = null;

        string? currentSection = null;

        for (var i = 0; i < allLines.Count; i++)
        {
            var lineNumber = i + 1;
            var originalLine = allLines[i];
            var line = StripComments(originalLine).Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                var sectionName = line[1..^1].Trim();

                if (currentSection == "Peer" && currentPeerProps != null)
                {
                    peers.Add(currentPeerProps);
                    currentPeerProps = null;
                }

                currentSection = sectionName;

                if (
                    !string.Equals(sectionName, "Interface", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(sectionName, "Peer", StringComparison.OrdinalIgnoreCase)
                )
                {
                    errors.Add(
                        new ParseError(
                            lineNumber,
                            $"Unknown section '[{sectionName}]'. Expected [Interface] or [Peer].",
                            Section: currentSection,
                            LineText: originalLine,
                            SurroundingLines: GetSurroundingLines(allLines, i)
                        )
                    );
                    continue;
                }

                if (string.Equals(sectionName, "Peer", StringComparison.OrdinalIgnoreCase))
                {
                    currentPeerProps = new Dictionary<string, string>(
                        StringComparer.OrdinalIgnoreCase
                    );
                }

                continue;
            }

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex == -1)
            {
                errors.Add(
                    new ParseError(
                        lineNumber,
                        "Invalid property format. Expected 'Key = Value'.",
                        Section: currentSection,
                        LineText: originalLine,
                        SurroundingLines: GetSurroundingLines(allLines, i)
                    )
                );
                continue;
            }

            var propertyName = line[..equalsIndex].Trim();
            var propertyValue = line[(equalsIndex + 1)..].Trim();

            if (currentSection == null)
            {
                errors.Add(
                    new ParseError(
                        lineNumber,
                        "Property found outside of any section. Properties must be within [Interface] or [Peer] sections.",
                        PropertyName: propertyName,
                        LineText: originalLine,
                        SurroundingLines: GetSurroundingLines(allLines, i)
                    )
                );
                continue;
            }

            if (string.Equals(currentSection, "Interface", StringComparison.OrdinalIgnoreCase))
            {
                if (
                    !IsValidInterfaceProperty(propertyName)
                    && !interfaceProps.ContainsKey(propertyName)
                )
                {
                    errors.Add(
                        new ParseError(
                            lineNumber,
                            $"Unknown property '{propertyName}' in [Interface] section.",
                            PropertyName: propertyName,
                            Section: currentSection,
                            LineText: originalLine,
                            SurroundingLines: GetSurroundingLines(allLines, i)
                        )
                    );
                    continue;
                }

                interfaceProps[propertyName] = propertyValue;
            }
            else if (string.Equals(currentSection, "Peer", StringComparison.OrdinalIgnoreCase))
            {
                if (currentPeerProps == null)
                {
                    errors.Add(
                        new ParseError(
                            lineNumber,
                            "Internal error: Peer properties found but no peer section started.",
                            PropertyName: propertyName,
                            Section: currentSection,
                            LineText: originalLine
                        )
                    );
                    continue;
                }

                if (
                    !IsValidPeerProperty(propertyName)
                    && !currentPeerProps.ContainsKey(propertyName)
                )
                {
                    errors.Add(
                        new ParseError(
                            lineNumber,
                            $"Unknown property '{propertyName}' in [Peer] section.",
                            PropertyName: propertyName,
                            Section: currentSection,
                            LineText: originalLine,
                            SurroundingLines: GetSurroundingLines(allLines, i)
                        )
                    );
                    continue;
                }

                currentPeerProps[propertyName] = propertyValue;
            }
        }

        if (currentSection == "Peer" && currentPeerProps != null)
        {
            peers.Add(currentPeerProps);
        }

        var config = BuildConfiguration(interfaceProps, peers, allLines, errors);
        return config;
    }

    protected virtual WireguardConfiguration BuildConfiguration(
        Dictionary<string, string> interfaceProps,
        List<Dictionary<string, string>> peers,
        List<string> allLines,
        List<ParseError> errors
    )
    {
        var config = new WireguardConfiguration
        {
            PrivateKey = ParsePrivateKey(interfaceProps, allLines, errors),
            ListenPort = ParseListenPort(interfaceProps, allLines, errors),
            Address = ParseAddress(interfaceProps, allLines, errors),
        };

        if (interfaceProps.TryGetValue("PreUp", out var preUp))
        {
            config.PreUp = preUp;
        }

        if (interfaceProps.TryGetValue("PostUp", out var postUp))
        {
            config.PostUp = postUp;
        }

        if (interfaceProps.TryGetValue("PreDown", out var preDown))
        {
            config.PreDown = preDown;
        }

        if (interfaceProps.TryGetValue("PostDown", out var postDown))
        {
            config.PostDown = postDown;
        }

        foreach (var peerProps in peers)
        {
            var peer = BuildPeer(peerProps, allLines, errors);
            if (peer != null)
            {
                config.Peers.Add(peer);
            }
        }

        return config;
    }

    protected virtual WireguardPeerConfiguration? BuildPeer(
        Dictionary<string, string> peerProps,
        List<string> allLines,
        List<ParseError> errors
    )
    {
        try
        {
            var peer = new WireguardPeerConfiguration
            {
                PublicKey = ParsePublicKey(peerProps, allLines, errors),
                AllowedIPs = ParseAllowedIPs(peerProps, allLines, errors),
            };

            if (peerProps.TryGetValue("Endpoint", out var endpoint))
            {
                if (WireguardEndpoint.TryParse(endpoint, out var parsedEndpoint, out var exception))
                {
                    peer.Endpoint = parsedEndpoint;
                }
                else
                {
                    errors.Add(
                        new ParseError(
                            0,
                            $"Invalid Endpoint format: {endpoint}. {exception?.Message ?? "Expected 'host:port' format."}"
                        )
                    );
                }
            }

            if (peerProps.TryGetValue("PresharedKey", out var presharedKey))
            {
                try
                {
                    var key = Convert.FromBase64String(presharedKey);
                    if (key.Length != WireguardConfiguration.Constants.KEY_LENGTH_BYTES)
                    {
                        errors.Add(
                            new ParseError(
                                0,
                                $"Invalid PresharedKey length. Expected {WireguardConfiguration.Constants.KEY_LENGTH_BYTES} bytes, got {key.Length}."
                            )
                        );
                    }

                    peer.PresharedKey = key;
                }
                catch (FormatException)
                {
                    errors.Add(
                        new ParseError(
                            0,
                            $"Invalid PresharedKey format. Expected Base64 encoded string."
                        )
                    );
                }
            }

            if (peerProps.TryGetValue("PersistedKeepalive", out var persistedKeepalive))
            {
                if (int.TryParse(persistedKeepalive, out var keepalive))
                {
                    peer.PersistedKeepalive = keepalive;
                }
                else
                {
                    errors.Add(
                        new ParseError(
                            0,
                            $"Invalid PersistedKeepalive format. Expected integer value."
                        )
                    );
                }
            }

            return peer;
        }
        catch
        {
            return null;
        }
    }

    private byte[] ParsePrivateKey(
        Dictionary<string, string> props,
        List<string> allLines,
        List<ParseError> errors
    )
    {
        if (!props.TryGetValue("PrivateKey", out var value))
        {
            errors.Add(
                new ParseError(0, "Missing required property 'PrivateKey' in [Interface] section.")
            );
            return new byte[WireguardConfiguration.Constants.KEY_LENGTH_BYTES];
        }

        try
        {
            var key = Convert.FromBase64String(value);
            if (key.Length != WireguardConfiguration.Constants.KEY_LENGTH_BYTES)
            {
                errors.Add(
                    new ParseError(
                        0,
                        $"Invalid PrivateKey length. Expected {WireguardConfiguration.Constants.KEY_LENGTH_BYTES} bytes, got {key.Length}."
                    )
                );
                return new byte[WireguardConfiguration.Constants.KEY_LENGTH_BYTES];
            }

            return key;
        }
        catch (FormatException)
        {
            errors.Add(
                new ParseError(0, "Invalid PrivateKey format. Expected Base64 encoded string.")
            );
            return new byte[WireguardConfiguration.Constants.KEY_LENGTH_BYTES];
        }
    }

    private ushort ParseListenPort(
        Dictionary<string, string> props,
        List<string> allLines,
        List<ParseError> errors
    )
    {
        if (!props.TryGetValue("ListenPort", out var value))
        {
            errors.Add(
                new ParseError(0, "Missing required property 'ListenPort' in [Interface] section.")
            );
            return WireguardConfiguration.Constants.PORT_MIN;
        }

        if (int.TryParse(value, out var port))
        {
            if (
                port < WireguardConfiguration.Constants.PORT_MIN
                || port > WireguardConfiguration.Constants.PORT_MAX
            )
            {
                errors.Add(
                    new ParseError(
                        0,
                        $"Invalid ListenPort value. Must be between {WireguardConfiguration.Constants.PORT_MIN} and {WireguardConfiguration.Constants.PORT_MAX}, got {port}."
                    )
                );
                return WireguardConfiguration.Constants.PORT_MIN;
            }

            return (ushort)port;
        }

        errors.Add(new ParseError(0, "Invalid ListenPort format. Expected integer value."));
        return WireguardConfiguration.Constants.PORT_MIN;
    }

    private CIDR ParseAddress(
        Dictionary<string, string> props,
        List<string> allLines,
        List<ParseError> errors
    )
    {
        if (!props.TryGetValue("Address", out var value))
        {
            errors.Add(
                new ParseError(0, "Missing required property 'Address' in [Interface] section.")
            );
            return default;
        }

        try
        {
            return CIDR.Parse(value);
        }
        catch (FormatException ex)
        {
            errors.Add(new ParseError(0, $"Invalid Address format: {ex.Message}"));
            return default;
        }
    }

    private byte[] ParsePublicKey(
        Dictionary<string, string> props,
        List<string> allLines,
        List<ParseError> errors
    )
    {
        if (!props.TryGetValue("PublicKey", out var value))
        {
            errors.Add(
                new ParseError(0, "Missing required property 'PublicKey' in [Peer] section.")
            );
            return Array.Empty<byte>();
        }

        try
        {
            var key = Convert.FromBase64String(value);
            if (key.Length != WireguardConfiguration.Constants.KEY_LENGTH_BYTES)
            {
                errors.Add(
                    new ParseError(
                        0,
                        $"Invalid PublicKey length. Expected {WireguardConfiguration.Constants.KEY_LENGTH_BYTES} bytes, got {key.Length}."
                    )
                );
            }

            return key;
        }
        catch (FormatException)
        {
            errors.Add(
                new ParseError(0, "Invalid PublicKey format. Expected Base64 encoded string.")
            );
            return Array.Empty<byte>();
        }
    }

    private List<CIDR> ParseAllowedIPs(
        Dictionary<string, string> props,
        List<string> allLines,
        List<ParseError> errors
    )
    {
        if (!props.TryGetValue("AllowedIPs", out var value))
        {
            errors.Add(
                new ParseError(0, "Missing required property 'AllowedIPs' in [Peer] section.")
            );
            return [];
        }

        var cidrs = new List<CIDR>();
        var parts = value.Split(',');

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            try
            {
                cidrs.Add(CIDR.Parse(trimmed));
            }
            catch (FormatException ex)
            {
                errors.Add(new ParseError(0, $"Invalid CIDR in AllowedIPs: {ex.Message}"));
            }
        }

        return cidrs.ToList();
    }

    private static string StripComments(string line)
    {
        var commentIndex = line.IndexOf('#');
        return commentIndex >= 0 ? line[..commentIndex] : line;
    }

    private static string[] GetSurroundingLines(List<string> allLines, int currentIndex)
    {
        const int contextLines = 2;
        var start = Math.Max(0, currentIndex - contextLines);
        var end = Math.Min(allLines.Count - 1, currentIndex + contextLines);
        var count = end - start + 1;

        var result = new string[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = allLines[start + i];
        }

        return result;
    }

    protected virtual bool IsValidInterfaceProperty(string propertyName)
    {
        return propertyName.Equals("PrivateKey", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("ListenPort", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("Address", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("PreUp", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("PostUp", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("PreDown", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("PostDown", StringComparison.OrdinalIgnoreCase);
    }

    protected virtual bool IsValidPeerProperty(string propertyName)
    {
        return propertyName.Equals("PublicKey", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("AllowedIPs", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("Endpoint", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("PresharedKey", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("PersistedKeepalive", StringComparison.OrdinalIgnoreCase);
    }
}
