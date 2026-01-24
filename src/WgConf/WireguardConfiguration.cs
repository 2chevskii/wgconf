namespace WgConf;

public class WireguardConfiguration
{
    public required byte[] PrivateKey
    {
        get;
        set =>
            field =
                value.Length != Constants.KEY_LENGTH_BYTES
                    ? throw new ArgumentException("Private key must be of 256 bit in size")
                    : value;
    }

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

    public required CIDR Address { get; set; }

    public string? PreUp { get; set; }
    public string? PostUp { get; set; }
    public string? PreDown { get; set; }
    public string? PostDown { get; set; }

    public List<WireguardPeerConfiguration> Peers { get; } = [];

    public static WireguardConfiguration Parse(string text)
    {
        using var reader = new WireguardConfigurationReader(new StringReader(text));
        return reader.Read();
    }

    public static bool TryParse(
        string text,
        out WireguardConfiguration? configuration,
        out IReadOnlyList<ParseError> errors
    )
    {
        using var reader = new WireguardConfigurationReader(new StringReader(text));
        return reader.TryRead(out configuration, out errors);
    }

    public static WireguardConfiguration Load(string path)
    {
        using var reader = new WireguardConfigurationReader(new StreamReader(path));
        return reader.Read();
    }

    public static async Task<WireguardConfiguration> LoadAsync(
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var text = await File.ReadAllTextAsync(path, cancellationToken);
        return Parse(text);
    }

    public void Save(string path)
    {
        using var writer = new WireguardConfigurationWriter(new StreamWriter(path));
        writer.Write(this);
    }

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
