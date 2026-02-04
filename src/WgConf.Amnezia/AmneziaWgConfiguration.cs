namespace WgConf.Amnezia;

/// <summary>
/// Represents an Amnezia WireGuard configuration.
/// </summary>
public sealed class AmneziaWgConfiguration : WireguardConfiguration
{
    /// <summary>
    /// Junk packet count
    /// </summary>
    public int? Jc { get; set; }

    /// <summary>
    /// Junk packet min size
    /// </summary>
    public int? Jmin { get; set; }

    /// <summary>
    /// Junk packet max size
    /// </summary>
    public int? Jmax { get; set; }

    /// <summary>
    /// Init packet junk size
    /// </summary>
    public int? S1 { get; set; }

    /// <summary>
    /// Response packet junk size
    /// </summary>
    public int? S2 { get; set; }

    /// <summary>
    /// Cookie reply packet junk size
    /// </summary>
    public int? S3 { get; set; }

    /// <summary>
    /// Transport packet junk size
    /// </summary>
    public int? S4 { get; set; }

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
    /// Init packet magic header
    /// </summary>
    public MagicHeader? H1 { get; set; }

    /// <summary>
    /// Response packet magic header
    /// </summary>
    public MagicHeader? H2 { get; set; }

    /// <summary>
    /// Underload packet magic header
    /// </summary>
    public MagicHeader? H3 { get; set; }

    /// <summary>
    /// Transport packet magic header
    /// </summary>
    public MagicHeader? H4 { get; set; }

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
