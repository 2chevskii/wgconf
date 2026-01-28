namespace WgConf.Amnezia;

/// <summary>
/// Reads and parses Amnezia WireGuard configurations from a text reader.
/// </summary>
/// <param name="textReader">The reader that provides the configuration text.</param>
public class AmneziaWgConfigurationReader(TextReader textReader)
    : WireguardConfigurationReader(textReader)
{
    /// <summary>
    /// Determines whether a property name is valid for the [Interface] section.
    /// </summary>
    /// <param name="propertyName">The property name to validate.</param>
    /// <returns><see langword="true"/> when the property is valid; otherwise <see langword="false"/>.</returns>
    protected override bool IsValidInterfaceProperty(string propertyName)
    {
        if (base.IsValidInterfaceProperty(propertyName))
        {
            return true;
        }

        return propertyName.Equals("Jc", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("Jmin", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("Jmax", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("S1", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("S2", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("S3", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("S4", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("J1", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("J2", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("J3", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("Itime", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("I1", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("I2", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("I3", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("I4", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("I5", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("H1", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("H2", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("H3", StringComparison.OrdinalIgnoreCase)
            || propertyName.Equals("H4", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Builds an Amnezia configuration object from parsed properties.
    /// </summary>
    /// <param name="interfaceProps">Parsed interface properties.</param>
    /// <param name="peers">Parsed peer property dictionaries.</param>
    /// <param name="allLines">All lines from the configuration file.</param>
    /// <param name="errors">The error list to populate.</param>
    /// <returns>The built configuration.</returns>
    protected override WireguardConfiguration BuildConfiguration(
        Dictionary<string, string> interfaceProps,
        List<Dictionary<string, string>> peers,
        List<string> allLines,
        List<ParseError> errors
    )
    {
        var baseConfig = base.BuildConfiguration(interfaceProps, peers, allLines, errors);

        var amneziaConfig = new AmneziaWgConfiguration
        {
            PrivateKey = baseConfig.PrivateKey,
            ListenPort = baseConfig.ListenPort,
            Address = baseConfig.Address,
            PreUp = baseConfig.PreUp,
            PostUp = baseConfig.PostUp,
            PreDown = baseConfig.PreDown,
            PostDown = baseConfig.PostDown,
        };

        foreach (var peer in baseConfig.Peers)
        {
            amneziaConfig.Peers.Add(peer);
        }

        if (interfaceProps.TryGetValue("Jc", out var jc) && int.TryParse(jc, out var jcValue))
        {
            amneziaConfig.Jc = jcValue;
        }
        else if (interfaceProps.ContainsKey("Jc"))
        {
            errors.Add(new ParseError(0, $"Invalid Jc format. Expected integer value."));
        }

        if (
            interfaceProps.TryGetValue("Jmin", out var jmin)
            && int.TryParse(jmin, out var jminValue)
        )
        {
            amneziaConfig.Jmin = jminValue;
        }
        else if (interfaceProps.ContainsKey("Jmin"))
        {
            errors.Add(new ParseError(0, $"Invalid Jmin format. Expected integer value."));
        }

        if (
            interfaceProps.TryGetValue("Jmax", out var jmax)
            && int.TryParse(jmax, out var jmaxValue)
        )
        {
            amneziaConfig.Jmax = jmaxValue;
        }
        else if (interfaceProps.ContainsKey("Jmax"))
        {
            errors.Add(new ParseError(0, $"Invalid Jmax format. Expected integer value."));
        }

        if (interfaceProps.TryGetValue("S1", out var s1) && int.TryParse(s1, out var s1Value))
        {
            amneziaConfig.S1 = s1Value;
        }
        else if (interfaceProps.ContainsKey("S1"))
        {
            errors.Add(new ParseError(0, $"Invalid S1 format. Expected integer value."));
        }

        if (interfaceProps.TryGetValue("S2", out var s2) && int.TryParse(s2, out var s2Value))
        {
            amneziaConfig.S2 = s2Value;
        }
        else if (interfaceProps.ContainsKey("S2"))
        {
            errors.Add(new ParseError(0, $"Invalid S2 format. Expected integer value."));
        }

        if (interfaceProps.TryGetValue("S3", out var s3) && int.TryParse(s3, out var s3Value))
        {
            amneziaConfig.S3 = s3Value;
        }
        else if (interfaceProps.ContainsKey("S3"))
        {
            errors.Add(new ParseError(0, $"Invalid S3 format. Expected integer value."));
        }

        if (interfaceProps.TryGetValue("S4", out var s4) && int.TryParse(s4, out var s4Value))
        {
            amneziaConfig.S4 = s4Value;
        }
        else if (interfaceProps.ContainsKey("S4"))
        {
            errors.Add(new ParseError(0, $"Invalid S4 format. Expected integer value."));
        }

        if (interfaceProps.TryGetValue("J1", out var j1) && int.TryParse(j1, out var j1Value))
        {
            amneziaConfig.J1 = j1Value;
        }
        else if (interfaceProps.ContainsKey("J1"))
        {
            errors.Add(new ParseError(0, $"Invalid J1 format. Expected integer value."));
        }

        if (interfaceProps.TryGetValue("J2", out var j2) && int.TryParse(j2, out var j2Value))
        {
            amneziaConfig.J2 = j2Value;
        }
        else if (interfaceProps.ContainsKey("J2"))
        {
            errors.Add(new ParseError(0, $"Invalid J2 format. Expected integer value."));
        }

        if (interfaceProps.TryGetValue("J3", out var j3) && int.TryParse(j3, out var j3Value))
        {
            amneziaConfig.J3 = j3Value;
        }
        else if (interfaceProps.ContainsKey("J3"))
        {
            errors.Add(new ParseError(0, $"Invalid J3 format. Expected integer value."));
        }

        if (
            interfaceProps.TryGetValue("Itime", out var itime)
            && int.TryParse(itime, out var itimeValue)
        )
        {
            amneziaConfig.Itime = itimeValue;
        }
        else if (interfaceProps.ContainsKey("Itime"))
        {
            errors.Add(new ParseError(0, $"Invalid Itime format. Expected integer value."));
        }

        if (interfaceProps.TryGetValue("I1", out var i1))
        {
            amneziaConfig.I1 = i1;
        }

        if (interfaceProps.TryGetValue("I2", out var i2))
        {
            amneziaConfig.I2 = i2;
        }

        if (interfaceProps.TryGetValue("I3", out var i3))
        {
            amneziaConfig.I3 = i3;
        }

        if (interfaceProps.TryGetValue("I4", out var i4))
        {
            amneziaConfig.I4 = i4;
        }

        if (interfaceProps.TryGetValue("I5", out var i5))
        {
            amneziaConfig.I5 = i5;
        }

        if (
            interfaceProps.TryGetValue("H1", out var h1)
            && HeaderValue.TryParse(h1, out var h1Value)
        )
        {
            amneziaConfig.H1 = h1Value;
        }
        else if (interfaceProps.ContainsKey("H1"))
        {
            errors.Add(new ParseError(0, $"Invalid H1 format. Expected 'start-end' format."));
        }

        if (
            interfaceProps.TryGetValue("H2", out var h2)
            && HeaderValue.TryParse(h2, out var h2Value)
        )
        {
            amneziaConfig.H2 = h2Value;
        }
        else if (interfaceProps.ContainsKey("H2"))
        {
            errors.Add(new ParseError(0, $"Invalid H2 format. Expected 'start-end' format."));
        }

        if (
            interfaceProps.TryGetValue("H3", out var h3)
            && HeaderValue.TryParse(h3, out var h3Value)
        )
        {
            amneziaConfig.H3 = h3Value;
        }
        else if (interfaceProps.ContainsKey("H3"))
        {
            errors.Add(new ParseError(0, $"Invalid H3 format. Expected 'start-end' format."));
        }

        if (
            interfaceProps.TryGetValue("H4", out var h4)
            && HeaderValue.TryParse(h4, out var h4Value)
        )
        {
            amneziaConfig.H4 = h4Value;
        }
        else if (interfaceProps.ContainsKey("H4"))
        {
            errors.Add(new ParseError(0, $"Invalid H4 format. Expected 'start-end' format."));
        }

        return amneziaConfig;
    }

    /// <summary>
    /// Reads and parses an Amnezia configuration, throwing when errors are encountered.
    /// </summary>
    /// <returns>The parsed configuration.</returns>
    /// <exception cref="WireguardConfigurationException">
    /// Thrown when parsing fails and errors are encountered.
    /// </exception>
    public new AmneziaWgConfiguration Read()
    {
        return (AmneziaWgConfiguration)base.Read();
    }

    /// <summary>
    /// Asynchronously reads and parses an Amnezia configuration, throwing when errors are encountered.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>The parsed configuration.</returns>
    /// <exception cref="WireguardConfigurationException">
    /// Thrown when parsing fails and errors are encountered.
    /// </exception>
    public new async Task<AmneziaWgConfiguration> ReadAsync(
        CancellationToken cancellationToken = default
    )
    {
        return (AmneziaWgConfiguration)await base.ReadAsync(cancellationToken);
    }

    /// <summary>
    /// Attempts to read and parse an Amnezia configuration from the underlying reader.
    /// </summary>
    /// <param name="configuration">The parsed configuration when successful; otherwise null.</param>
    /// <param name="errors">The list of parse errors encountered.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise <see langword="false"/>.</returns>
    public bool TryRead(
        out AmneziaWgConfiguration? configuration,
        out IReadOnlyList<ParseError> errors
    )
    {
        var result = base.TryRead(out var baseConfig, out errors);
        configuration = baseConfig as AmneziaWgConfiguration;
        return result;
    }

    /// <summary>
    /// Asynchronously attempts to read and parse an Amnezia configuration.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>
    /// A tuple containing the parsed configuration (when successful) and the list of errors.
    /// </returns>
    public new async Task<(
        AmneziaWgConfiguration? Configuration,
        IReadOnlyList<ParseError> Errors
    )> TryReadAsync(CancellationToken cancellationToken = default)
    {
        var (baseConfig, errors) = await base.TryReadAsync(cancellationToken);
        return (baseConfig as AmneziaWgConfiguration, errors);
    }
}
