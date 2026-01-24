using System.Net;

namespace WgConf;

public readonly struct WireguardEndpoint
{
    public required string Host { get; init; }
    public required int Port { get; init; }

    public static implicit operator WireguardEndpoint(ReadOnlySpan<char> input)
    {
        return Parse(input);
    }

    public static bool TryParse(ReadOnlySpan<char> input, out WireguardEndpoint result, out Exception? exception)
    {
        result = default;
        exception = null;

        ParseInternal(input, ref result, ref exception);
        return exception == null;
    }

    public static WireguardEndpoint Parse(ReadOnlySpan<char> input)
    {
        WireguardEndpoint result = default;
        Exception? exception = null;

        ParseInternal(input, ref result, ref exception);
        return exception != null ? throw exception : result;
    }

    private static void ParseInternal(
        ReadOnlySpan<char> input,
        ref WireguardEndpoint result,
        ref Exception? exception
    )
    {
        var lastColonIndex = input.LastIndexOf(':');
        if (lastColonIndex == -1)
        {
            exception = new FormatException("Endpoint must be in format 'host:port'");
            return;
        }

        var hostPart = input[..lastColonIndex];
        var portPart = input[(lastColonIndex + 1)..];

        if (hostPart.Length == 0)
        {
            exception = new FormatException("Endpoint host cannot be empty");
            return;
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
                exception = new FormatException("Invalid IPv6 address in endpoint");
                return;
            }
        }
        else
        {
            host = hostPart.ToString();
        }

        if (!int.TryParse(portPart, out var port))
        {
            exception = new FormatException("Invalid port number in endpoint");
            return;
        }

        if (port < 1 || port > 65535)
        {
            exception = new FormatException($"Port must be between 1 and 65535, got {port}");
            return;
        }

        result = new WireguardEndpoint { Host = host, Port = port };
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
