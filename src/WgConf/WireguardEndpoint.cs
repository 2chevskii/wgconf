using System.Net;

namespace WgConf;

public readonly struct WireguardEndpoint
{
    public required string Host { get; init; }
    public required int Port { get; init; }

    public static WireguardEndpoint Parse(string s)
    {
        return Parse(s.AsSpan());
    }

    public static WireguardEndpoint Parse(ReadOnlySpan<char> s)
    {
        var lastColonIndex = s.LastIndexOf(':');
        if (lastColonIndex == -1)
        {
            throw new FormatException("Endpoint must be in format 'host:port'");
        }

        var hostPart = s[..lastColonIndex];
        var portPart = s[(lastColonIndex + 1)..];

        if (hostPart.Length == 0)
        {
            throw new FormatException("Endpoint host cannot be empty");
        }

        // Handle IPv6 addresses in brackets: [::1]:51820
        string host;
        if (hostPart[0] == '[' && hostPart[^1] == ']')
        {
            host = hostPart[1..^1].ToString();
            if (
                !IPAddress.TryParse(host, out var ipv6)
                || ipv6.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6
            )
            {
                throw new FormatException("Invalid IPv6 address in endpoint");
            }
        }
        else
        {
            host = hostPart.ToString();
        }

        if (!int.TryParse(portPart, out var port))
        {
            throw new FormatException("Invalid port number in endpoint");
        }

        if (port < 1 || port > 65535)
        {
            throw new FormatException($"Port must be between 1 and 65535, got {port}");
        }

        return new WireguardEndpoint { Host = host, Port = port };
    }

    public static bool TryParse(string s, out WireguardEndpoint result)
    {
        try
        {
            result = Parse(s);
            return true;
        }
        catch (FormatException)
        {
            result = default;
            return false;
        }
    }

    public override string ToString()
    {
        // If the host is an IPv6 address, wrap it in brackets
        if (
            IPAddress.TryParse(Host, out var ip)
            && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
        )
        {
            return $"[{Host}]:{Port}";
        }

        return $"{Host}:{Port}";
    }
}
