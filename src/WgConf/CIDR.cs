using System.Net;
using System.Net.Sockets;

namespace WgConf;

public readonly struct CIDR
{
    public required IPAddress Address { get; init; }
    public required int PrefixLength { get; init; }

    public static CIDR Parse(string s)
    {
        return Parse(s.AsSpan());
    }

    public static CIDR Parse(ReadOnlySpan<char> s)
    {
        int slashIndex = s.IndexOf('/');
        if (slashIndex == -1)
        {
            throw new FormatException("CIDR notation must contain '/' separator");
        }

        var addressPart = s[..slashIndex];
        var prefixPart = s[slashIndex..][1..];

        if (!IPAddress.TryParse(addressPart, out var address))
        {
            throw new FormatException("Invalid IP address in CIDR notation");
        }

        if (!int.TryParse(prefixPart, out var prefixLength))
        {
            throw new FormatException("Invalid prefix length in CIDR notation");
        }

        int maxPrefixLength = address.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;
        if (prefixLength < 0 || prefixLength > maxPrefixLength)
        {
            throw new FormatException(
                $"Prefix length must be between 0 and {maxPrefixLength} for {address.AddressFamily}"
            );
        }

        return new CIDR { Address = address, PrefixLength = prefixLength };
    }

    public override string ToString()
    {
        return $"{Address}/{PrefixLength}";
    }
}
