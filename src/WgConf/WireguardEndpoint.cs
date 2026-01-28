using System.Net;

namespace WgConf;

/// <summary>
/// Represents a WireGuard endpoint in host:port form.
/// </summary>
public readonly struct WireguardEndpoint
{
    /// <summary>
    /// Gets the endpoint host.
    /// </summary>
    public required string Host { get; init; }

    /// <summary>
    /// Gets the endpoint port.
    /// </summary>
    public required int Port { get; init; }

    /// <summary>
    /// Parses an endpoint from a character span.
    /// </summary>
    /// <param name="input">The input in <c>host:port</c> format.</param>
    public static implicit operator WireguardEndpoint(ReadOnlySpan<char> input)
    {
        return Parse(input);
    }

    /// <summary>
    /// Attempts to parse an endpoint from a character span.
    /// </summary>
    /// <param name="input">The input in <c>host:port</c> format.</param>
    /// <param name="result">The parsed endpoint when successful.</param>
    /// <param name="exception">The parsing exception when unsuccessful.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(
        ReadOnlySpan<char> input,
        out WireguardEndpoint result,
        out Exception? exception
    )
    {
        result = default;
        exception = null;

        ParseInternal(input, ref result, ref exception);
        return exception == null;
    }

    /// <summary>
    /// Parses an endpoint from a character span.
    /// </summary>
    /// <param name="input">The input in <c>host:port</c> format.</param>
    /// <returns>The parsed endpoint.</returns>
    /// <exception cref="FormatException">Thrown when the input is invalid.</exception>
    public static WireguardEndpoint Parse(ReadOnlySpan<char> input)
    {
        WireguardEndpoint result = default;
        Exception? exception = null;

        ParseInternal(input, ref result, ref exception);
        return exception != null ? throw exception : result;
    }

    /// <summary>
    /// Parses an endpoint into the provided result and exception references.
    /// </summary>
    /// <param name="input">The input in <c>host:port</c> format.</param>
    /// <param name="result">The parsed endpoint when successful.</param>
    /// <param name="exception">The parsing exception when unsuccessful.</param>
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

    /// <summary>
    /// Returns the endpoint in <c>host:port</c> format, with IPv6 hosts wrapped in brackets.
    /// </summary>
    /// <returns>The endpoint string.</returns>
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
