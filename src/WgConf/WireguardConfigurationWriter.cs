using WgConf.Extensions;

namespace WgConf;

public class WireguardConfigurationWriter(TextWriter textWriter) : IDisposable, IAsyncDisposable
{
    protected readonly TextWriter _textWriter = textWriter;

    public void Dispose()
    {
        _textWriter.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _textWriter.DisposeAsync();
    }

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

    private void WriteSectionHeader(string sectionName)
    {
        _textWriter.Write('[');
        _textWriter.Write(sectionName);
        _textWriter.Write(']');
        _textWriter.WriteLine();
    }

    private void WriteProperty(string name, int value)
    {
        WritePropertyNameAndSeparator(name);
        _textWriter.Write(value);
        _textWriter.WriteLine();
    }

    private void WriteProperty(string name, string value)
    {
        WritePropertyNameAndSeparator(name);
        _textWriter.Write(value);
        _textWriter.WriteLine();
    }

    private void WriteProperty(string name, CIDR value)
    {
        WritePropertyNameAndSeparator(name);
        _textWriter.Write(value.ToString());
        _textWriter.WriteLine();
    }

    private void WriteProperty(string name, CIDR[] value)
    {
        WritePropertyNameAndSeparator(name);
        for (var i = 0; i < value.Length; i++)
        {
            _textWriter.Write(value[i].ToString());
            if (i != value.Length - 1)
            {
                _textWriter.Write(',');
                _textWriter.Write(' ');
            }
        }

        _textWriter.WriteLine();
    }

    private void WriteProperty(string name, WireguardEndpoint value)
    {
        WritePropertyNameAndSeparator(name);
        _textWriter.Write(value.ToString());
        _textWriter.WriteLine();
    }

    private void WriteProperty(string name, byte[] value)
    {
        WritePropertyNameAndSeparator(name);
        _textWriter.Write(Convert.ToBase64String(value));
        _textWriter.WriteLine();
    }

    private void WritePropertyNameAndSeparator(string name)
    {
        _textWriter.Write(name);
        _textWriter.Write(' ');
        _textWriter.Write('=');
        _textWriter.Write(' ');
    }
}
