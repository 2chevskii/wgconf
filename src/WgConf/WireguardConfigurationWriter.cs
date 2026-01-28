using WgConf.Extensions;

namespace WgConf;

/// <summary>
/// Writes WireGuard configurations to a text writer.
/// </summary>
/// <param name="textWriter">The writer that receives the configuration text.</param>
public class WireguardConfigurationWriter(TextWriter textWriter) : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The underlying text writer.
    /// </summary>
    protected readonly TextWriter _textWriter = textWriter;

    /// <summary>
    /// Disposes the underlying text writer.
    /// </summary>
    public void Dispose()
    {
        _textWriter.Dispose();
    }

    /// <summary>
    /// Asynchronously disposes the underlying text writer.
    /// </summary>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await _textWriter.DisposeAsync();
    }

    /// <summary>
    /// Writes a WireGuard configuration to the underlying writer.
    /// </summary>
    /// <param name="configuration">The configuration to write.</param>
    public virtual void Write(WireguardConfiguration configuration)
    {
        WriteInterface(configuration);
        _textWriter.WriteLine();
        foreach (WireguardPeerConfiguration peerConfiguration in configuration.Peers)
        {
            WritePeer(peerConfiguration);
            _textWriter.WriteLine();
        }
    }

    /// <summary>
    /// Writes the [Interface] section for the configuration.
    /// </summary>
    /// <param name="configuration">The configuration to write.</param>
    protected virtual void WriteInterface(WireguardConfiguration configuration)
    {
        WriteSectionHeader("Interface");
        WriteProperty(nameof(WireguardConfiguration.PrivateKey), configuration.PrivateKey);
        WriteProperty(nameof(WireguardConfiguration.ListenPort), configuration.ListenPort);
        WriteProperty(nameof(WireguardConfiguration.Address), configuration.Address);
        configuration.PreUp.Let(v => WriteProperty(nameof(WireguardConfiguration.PreUp), v));
        configuration.PostUp.Let(v => WriteProperty(nameof(WireguardConfiguration.PostUp), v));
        configuration.PreDown.Let(v => WriteProperty(nameof(WireguardConfiguration.PreDown), v));
        configuration.PostDown.Let(v => WriteProperty(nameof(WireguardConfiguration.PostDown), v));
    }

    /// <summary>
    /// Writes a [Peer] section for the provided peer configuration.
    /// </summary>
    /// <param name="configuration">The peer configuration to write.</param>
    protected virtual void WritePeer(WireguardPeerConfiguration configuration)
    {
        WriteSectionHeader("Peer");
        WriteProperty(nameof(WireguardPeerConfiguration.AllowedIPs), configuration.AllowedIPs);
        WriteProperty(nameof(WireguardPeerConfiguration.PublicKey), configuration.PublicKey);
        configuration.PresharedKey.Let(v =>
            WriteProperty(nameof(WireguardPeerConfiguration.PresharedKey), v)
        );
        configuration.Endpoint.Let(v =>
            WriteProperty(nameof(WireguardPeerConfiguration.Endpoint), v)
        );
        configuration.PersistedKeepalive.Let(v =>
            WriteProperty(nameof(WireguardPeerConfiguration.PersistedKeepalive), v)
        );
    }

    /// <summary>
    /// Writes a section header line.
    /// </summary>
    /// <param name="sectionName">The section name to write.</param>
    private void WriteSectionHeader(string sectionName)
    {
        _textWriter.Write('[');
        _textWriter.Write(sectionName);
        _textWriter.Write(']');
        _textWriter.WriteLine();
    }

    /// <summary>
    /// Writes an integer property line.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The property value.</param>
    private void WriteProperty(string name, int value)
    {
        WritePropertyNameAndSeparator(name);
        _textWriter.Write(value);
        _textWriter.WriteLine();
    }

    /// <summary>
    /// Writes a string property line.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The property value.</param>
    private void WriteProperty(string name, string value)
    {
        WritePropertyNameAndSeparator(name);
        _textWriter.Write(value);
        _textWriter.WriteLine();
    }

    /// <summary>
    /// Writes a CIDR property line.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The CIDR value.</param>
    private void WriteProperty(string name, CIDR value)
    {
        WritePropertyNameAndSeparator(name);
        _textWriter.Write(value.ToString());
        _textWriter.WriteLine();
    }

    /// <summary>
    /// Writes a list of CIDR values as a comma-separated property line.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The list of CIDR values.</param>
    private void WriteProperty(string name, List<CIDR> value)
    {
        WritePropertyNameAndSeparator(name);
        for (var i = 0; i < value.Count; i++)
        {
            _textWriter.Write(value[i].ToString());
            if (i != value.Count - 1)
            {
                _textWriter.Write(',');
                _textWriter.Write(' ');
            }
        }

        _textWriter.WriteLine();
    }

    /// <summary>
    /// Writes an endpoint property line.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The endpoint value.</param>
    private void WriteProperty(string name, WireguardEndpoint value)
    {
        WritePropertyNameAndSeparator(name);
        _textWriter.Write(value.ToString());
        _textWriter.WriteLine();
    }

    /// <summary>
    /// Writes a byte array property line encoded as Base64.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The byte array value.</param>
    private void WriteProperty(string name, byte[] value)
    {
        WritePropertyNameAndSeparator(name);
        _textWriter.Write(Convert.ToBase64String(value));
        _textWriter.WriteLine();
    }

    /// <summary>
    /// Writes the property name and separator (" = ").
    /// </summary>
    /// <param name="name">The property name.</param>
    private void WritePropertyNameAndSeparator(string name)
    {
        _textWriter.Write(name);
        _textWriter.Write(' ');
        _textWriter.Write('=');
        _textWriter.Write(' ');
    }
}
