namespace WgConf;

/// <summary>
/// Represents a WireGuard configuration.
/// </summary>
public class WireguardConfiguration
{
    /// <summary>
    /// Gets or sets the private key for the interface.
    /// </summary>
    public required byte[] PrivateKey
    {
        get;
        set =>
            field =
                value.Length != Constants.KEY_LENGTH_BYTES
                    ? throw new ArgumentException("Private key must be of 256 bit in size")
                    : value;
    }

    /// <summary>
    /// Gets or sets the listen port for the interface.
    /// </summary>
    public required ushort ListenPort
    {
        get;
        set =>
            field =
                value < Constants.PORT_MIN || value > Constants.PORT_MAX
                    ? throw new ArgumentException(
                        $"ListenPort value should be between {Constants.PORT_MIN} and {Constants.PORT_MAX}"
                    )
                    : value;
    }

    /// <summary>
    /// Gets or sets the interface address in CIDR notation.
    /// </summary>
    public required CIDR Address { get; set; }

    /// <summary>
    /// Gets or sets the command executed before the interface is brought up.
    /// </summary>
    public string? PreUp { get; set; }

    /// <summary>
    /// Gets or sets the command executed after the interface is brought up.
    /// </summary>
    public string? PostUp { get; set; }

    /// <summary>
    /// Gets or sets the command executed before the interface is brought down.
    /// </summary>
    public string? PreDown { get; set; }

    /// <summary>
    /// Gets or sets the command executed after the interface is brought down.
    /// </summary>
    public string? PostDown { get; set; }

    /// <summary>
    /// Gets the list of peer configurations.
    /// </summary>
    public List<WireguardPeerConfiguration> Peers { get; } = [];

    /// <summary>
    /// Parses a WireGuard configuration from a string.
    /// </summary>
    /// <param name="text">The configuration text to parse.</param>
    /// <returns>The parsed configuration.</returns>
    /// <exception cref="WireguardConfigurationException">
    /// Thrown when parsing fails and errors are encountered.
    /// </exception>
    public static WireguardConfiguration Parse(string text)
    {
        using var reader = new WireguardConfigurationReader(new StringReader(text));
        return reader.Read();
    }

    /// <summary>
    /// Attempts to parse a WireGuard configuration from a string.
    /// </summary>
    /// <param name="text">The configuration text to parse.</param>
    /// <param name="configuration">The parsed configuration when successful; otherwise null.</param>
    /// <param name="errors">The list of parse errors encountered.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(
        string text,
        out WireguardConfiguration? configuration,
        out IReadOnlyList<ParseError> errors
    )
    {
        using var reader = new WireguardConfigurationReader(new StringReader(text));
        return reader.TryRead(out configuration, out errors);
    }

    /// <summary>
    /// Loads a WireGuard configuration from a file.
    /// </summary>
    /// <param name="path">The path to the configuration file.</param>
    /// <returns>The parsed configuration.</returns>
    /// <exception cref="WireguardConfigurationException">
    /// Thrown when parsing fails and errors are encountered.
    /// </exception>
    public static WireguardConfiguration Load(string path)
    {
        using var reader = new WireguardConfigurationReader(new StreamReader(path));
        return reader.Read();
    }

    /// <summary>
    /// Asynchronously loads a WireGuard configuration from a file.
    /// </summary>
    /// <param name="path">The path to the configuration file.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    /// <returns>The parsed configuration.</returns>
    /// <exception cref="WireguardConfigurationException">
    /// Thrown when parsing fails and errors are encountered.
    /// </exception>
    public static async Task<WireguardConfiguration> LoadAsync(
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
    public void Save(string path)
    {
        using var writer = new WireguardConfigurationWriter(new StreamWriter(path));
        writer.Write(this);
    }

    /// <summary>
    /// Asynchronously saves the configuration to a file.
    /// </summary>
    /// <param name="path">The path to write the configuration file to.</param>
    /// <param name="cancellationToken">The token to cancel the operation.</param>
    public async Task SaveAsync(string path, CancellationToken cancellationToken = default)
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
        await using var writer = new WireguardConfigurationWriter(streamWriter);
        writer.Write(this);
        await streamWriter.FlushAsync(cancellationToken);
    }

    internal static class Constants
    {
        public const byte KEY_LENGTH_BYTES = 32;
        public const ushort PORT_MIN = 1;
        public const ushort PORT_MAX = ushort.MaxValue;
    }
}
