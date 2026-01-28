namespace WgConf.Amnezia;

/// <summary>
/// Represents an Amnezia WireGuard configuration.
/// </summary>
public class AmneziaWgConfiguration : WireguardConfiguration
{
    /// <summary>
    /// Gets or sets the Jc parameter.
    /// </summary>
    public int? Jc { get; set; }

    /// <summary>
    /// Gets or sets the Jmin parameter.
    /// </summary>
    public int? Jmin { get; set; }

    /// <summary>
    /// Gets or sets the Jmax parameter.
    /// </summary>
    public int? Jmax { get; set; }

    /// <summary>
    /// Gets or sets the S1 parameter.
    /// </summary>
    public int? S1 { get; set; }

    /// <summary>
    /// Gets or sets the S2 parameter.
    /// </summary>
    public int? S2 { get; set; }

    /// <summary>
    /// Gets or sets the S3 parameter.
    /// </summary>
    public int? S3 { get; set; }

    /// <summary>
    /// Gets or sets the S4 parameter.
    /// </summary>
    public int? S4 { get; set; }

    /// <summary>
    /// Gets or sets the J1 parameter.
    /// </summary>
    public int? J1 { get; set; }

    /// <summary>
    /// Gets or sets the J2 parameter.
    /// </summary>
    public int? J2 { get; set; }

    /// <summary>
    /// Gets or sets the J3 parameter.
    /// </summary>
    public int? J3 { get; set; }

    /// <summary>
    /// Gets or sets the Itime parameter.
    /// </summary>
    public int? Itime { get; set; }

    /// <summary>
    /// Gets or sets the I1 parameter.
    /// </summary>
    public string? I1 { get; set; }

    /// <summary>
    /// Gets or sets the I2 parameter.
    /// </summary>
    public string? I2 { get; set; }

    /// <summary>
    /// Gets or sets the I3 parameter.
    /// </summary>
    public string? I3 { get; set; }

    /// <summary>
    /// Gets or sets the I4 parameter.
    /// </summary>
    public string? I4 { get; set; }

    /// <summary>
    /// Gets or sets the I5 parameter.
    /// </summary>
    public string? I5 { get; set; }

    /// <summary>
    /// Gets or sets the H1 header value.
    /// </summary>
    public HeaderValue? H1 { get; set; }

    /// <summary>
    /// Gets or sets the H2 header value.
    /// </summary>
    public HeaderValue? H2 { get; set; }

    /// <summary>
    /// Gets or sets the H3 header value.
    /// </summary>
    public HeaderValue? H3 { get; set; }

    /// <summary>
    /// Gets or sets the H4 header value.
    /// </summary>
    public HeaderValue? H4 { get; set; }

    /// <summary>
    /// Parses an Amnezia WireGuard configuration from a string.
    /// </summary>
    /// <param name="text">The configuration text to parse.</param>
    /// <returns>The parsed configuration.</returns>
    /// <exception cref="WireguardConfigurationException">
    /// Thrown when parsing fails and errors are encountered.
    /// </exception>
    public static new AmneziaWgConfiguration Parse(string text)
    {
        using var reader = new AmneziaWgConfigurationReader(new StringReader(text));
        return reader.Read();
    }

    /// <summary>
    /// Attempts to parse an Amnezia WireGuard configuration from a string.
    /// </summary>
    /// <param name="text">The configuration text to parse.</param>
    /// <param name="configuration">The parsed configuration when successful; otherwise null.</param>
    /// <param name="errors">The list of parse errors encountered.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(
        string text,
        out AmneziaWgConfiguration? configuration,
        out IReadOnlyList<ParseError> errors
    )
    {
        using var reader = new AmneziaWgConfigurationReader(new StringReader(text));
        return reader.TryRead(out configuration, out errors);
    }

    /// <summary>
    /// Loads an Amnezia WireGuard configuration from a file.
    /// </summary>
    /// <param name="path">The path to the configuration file.</param>
    /// <returns>The parsed configuration.</returns>
    /// <exception cref="WireguardConfigurationException">
    /// Thrown when parsing fails and errors are encountered.
    /// </exception>
    public static new AmneziaWgConfiguration Load(string path)
    {
        using var reader = new AmneziaWgConfigurationReader(new StreamReader(path));
        return reader.Read();
    }

    /// <summary>
    /// Asynchronously loads an Amnezia WireGuard configuration from a file.
    /// </summary>
    /// <param name="path">The path to the configuration file.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>The parsed configuration.</returns>
    /// <exception cref="WireguardConfigurationException">
    /// Thrown when parsing fails and errors are encountered.
    /// </exception>
    public static new async Task<AmneziaWgConfiguration> LoadAsync(
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var text = await File.ReadAllTextAsync(path, cancellationToken);
        return Parse(text);
    }

    /// <summary>
    /// Saves the configuration to a file.
    /// </summary>
    /// <param name="path">The path to write the configuration file to.</param>
    public new void Save(string path)
    {
        using var writer = new AmneziaWgConfigurationWriter(new StreamWriter(path));
        writer.Write(this);
    }

    /// <summary>
    /// Asynchronously saves the configuration to a file.
    /// </summary>
    /// <param name="path">The path to write the configuration file to.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    public new async Task SaveAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true
        );
        await using var streamWriter = new StreamWriter(stream);
        await using var writer = new AmneziaWgConfigurationWriter(streamWriter);
        writer.Write(this);
        await streamWriter.FlushAsync(cancellationToken);
    }
}
