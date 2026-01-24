namespace WgConf.Amnezia;

public class AmneziaWgConfiguration : WireguardConfiguration
{
    public int? Jc { get; set; }
    public int? Jmin { get; set; }
    public int? Jmax { get; set; }
    public int? S1 { get; set; }
    public int? S2 { get; set; }
    public int? S3 { get; set; }
    public int? S4 { get; set; }
    public int? J1 { get; set; }
    public int? J2 { get; set; }
    public int? J3 { get; set; }
    public int? Itime { get; set; }

    public string? I1 { get; set; }
    public string? I2 { get; set; }
    public string? I3 { get; set; }
    public string? I4 { get; set; }
    public string? I5 { get; set; }

    public HeaderValue? H1 { get; set; }
    public HeaderValue? H2 { get; set; }
    public HeaderValue? H3 { get; set; }
    public HeaderValue? H4 { get; set; }

    public static new AmneziaWgConfiguration Parse(string text)
    {
        using var reader = new AmneziaWgConfigurationReader(new StringReader(text));
        return reader.Read();
    }

    public static bool TryParse(
        string text,
        out AmneziaWgConfiguration? configuration,
        out IReadOnlyList<ParseError> errors
    )
    {
        using var reader = new AmneziaWgConfigurationReader(new StringReader(text));
        return reader.TryRead(out configuration, out errors);
    }

    public static new AmneziaWgConfiguration Load(string path)
    {
        using var reader = new AmneziaWgConfigurationReader(new StreamReader(path));
        return reader.Read();
    }

    public static new async Task<AmneziaWgConfiguration> LoadAsync(
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var text = await File.ReadAllTextAsync(path, cancellationToken);
        return Parse(text);
    }

    public new void Save(string path)
    {
        using var writer = new AmneziaWgConfigurationWriter(new StreamWriter(path));
        writer.Write(this);
    }

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
