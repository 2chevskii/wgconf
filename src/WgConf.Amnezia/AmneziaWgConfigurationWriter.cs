using WgConf.Extensions;

namespace WgConf.Amnezia;

public class AmneziaWgConfigurationWriter(TextWriter textWriter)
    : WireguardConfigurationWriter(textWriter)
{
    protected override void WriteInterface(WireguardConfiguration configuration)
    {
        base.WriteInterface(configuration);

        if (configuration is AmneziaWgConfiguration amneziaConfig)
        {
            amneziaConfig.Jc.Let(v => WritePropertyDirect("Jc", v));
            amneziaConfig.Jmin.Let(v => WritePropertyDirect("Jmin", v));
            amneziaConfig.Jmax.Let(v => WritePropertyDirect("Jmax", v));
            amneziaConfig.S1.Let(v => WritePropertyDirect("S1", v));
            amneziaConfig.S2.Let(v => WritePropertyDirect("S2", v));
            amneziaConfig.S3.Let(v => WritePropertyDirect("S3", v));
            amneziaConfig.S4.Let(v => WritePropertyDirect("S4", v));
            amneziaConfig.J1.Let(v => WritePropertyDirect("J1", v));
            amneziaConfig.J2.Let(v => WritePropertyDirect("J2", v));
            amneziaConfig.J3.Let(v => WritePropertyDirect("J3", v));
            amneziaConfig.Itime.Let(v => WritePropertyDirect("Itime", v));
            amneziaConfig.I1.Let(v => WritePropertyDirect("I1", v));
            amneziaConfig.I2.Let(v => WritePropertyDirect("I2", v));
            amneziaConfig.I3.Let(v => WritePropertyDirect("I3", v));
            amneziaConfig.I4.Let(v => WritePropertyDirect("I4", v));
            amneziaConfig.I5.Let(v => WritePropertyDirect("I5", v));
            amneziaConfig.H1.Let(v => WritePropertyDirect("H1", v.ToString()));
            amneziaConfig.H2.Let(v => WritePropertyDirect("H2", v.ToString()));
            amneziaConfig.H3.Let(v => WritePropertyDirect("H3", v.ToString()));
            amneziaConfig.H4.Let(v => WritePropertyDirect("H4", v.ToString()));
        }
    }

    public void Write(AmneziaWgConfiguration configuration)
    {
        base.Write(configuration);
    }

    private void WritePropertyDirect(string name, int value)
    {
        _textWriter.Write(name);
        _textWriter.Write(" = ");
        _textWriter.Write(value);
        _textWriter.WriteLine();
    }

    private void WritePropertyDirect(string name, string value)
    {
        _textWriter.Write(name);
        _textWriter.Write(" = ");
        _textWriter.Write(value);
        _textWriter.WriteLine();
    }
}
