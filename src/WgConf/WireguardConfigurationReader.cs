namespace WgConf;

public class WireguardConfigurationReader : IDisposable
{
    protected readonly TextReader _textReader;

    public WireguardConfigurationReader(TextReader textReader)
    {
        _textReader = textReader;
    }

    public void Dispose()
    {
        _textReader.Dispose();
    }

    public virtual WireguardConfiguration Read()
    {
        throw new NotImplementedException();
    }
}
