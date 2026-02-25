using System.Net;
using System.Net.Sockets;
using WgConf.Extensions;

namespace WgConf;

/// <summary>
/// Represents a CIDR address with an IP address and prefix length.
/// </summary>
public readonly struct CIDR
{
    /// <summary>
    /// Gets the IP address portion of the CIDR.
    /// </summary>
    public readonly IPAddress Address;

    /// <summary>
    /// Gets the prefix length portion of the CIDR.
    /// </summary>
    public readonly int PrefixLength;

    public CIDR(IPAddress address, int prefixLength = 0)
    {
        if (
            address.AddressFamily
            is not AddressFamily.InterNetwork
                and not AddressFamily.InterNetworkV6
        )
        {
            throw new ArgumentOutOfRangeException(
                nameof(address),
                "Only IPv4 and IPv6 AddressFamily is supported"
            );
        }

        if (PrefixLength < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(prefixLength),
                "Prefix length cannot be less than zero"
            );
        }

        if (address.AddressFamily == AddressFamily.InterNetwork && PrefixLength > 32)
        {
            throw new ArgumentOutOfRangeException(
                nameof(prefixLength),
                "Prefix length for IPv4 address cannot be greater than 32"
            );
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6 && prefixLength > 64)
        {
            throw new ArgumentOutOfRangeException(
                nameof(prefixLength),
                "Prefix length for IPv6 address cannot be greater than 64"
            );
        }

        Address = address;
        PrefixLength = prefixLength;
    }

    // public static implicit operator CIDR(string input) => Parse(input);
    /// <summary>
    /// Parses a CIDR value from a character span.
    /// </summary>
    /// <param name="input">The input in <c>address/prefix</c> format.</param>
    public static implicit operator CIDR(ReadOnlySpan<char> input) => Parse(input);

    public static implicit operator CIDR(ValueTuple<IPAddress, int> tuple) =>
        new CIDR(tuple.Item1, tuple.Item2);

    /// <summary>
    /// Attempts to parse a CIDR value from a character span.
    /// </summary>
    /// <param name="input">The input in <c>address/prefix</c> format.</param>
    /// <param name="result">The parsed CIDR when successful.</param>
    /// <param name="exception">The parsing exception when unsuccessful.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> input, out CIDR result, out Exception? exception)
    {
        result = default;
        exception = null;

        ParseInternal(input, ref result, ref exception);
        return exception == null;
    }

    /// <summary>
    /// Parses a CIDR value from a character span.
    /// </summary>
    /// <param name="input">The input in <c>address/prefix</c> format.</param>
    /// <returns>The parsed CIDR value.</returns>
    /// <exception cref="FormatException">Thrown when the input is invalid.</exception>
    public static CIDR Parse(ReadOnlySpan<char> input)
    {
        CIDR result = default;
        Exception? exception = null;

        ParseInternal(input, ref result, ref exception);
        return exception != null ? throw exception : result;
    }

    /// <summary>
    /// Parses a CIDR value into the provided result and exception references.
    /// </summary>
    /// <param name="input">The input in <c>address/prefix</c> format.</param>
    /// <param name="result">The parsed CIDR when successful.</param>
    /// <param name="exception">The parsing exception when unsuccessful.</param>
    private static void ParseInternal(
        ReadOnlySpan<char> input,
        ref CIDR result,
        ref Exception? exception
    )
    {
        int slashIndex = input.IndexOf('/');
        if (slashIndex == -1)
        {
            exception = new FormatException("CIDR notation must contain '/' separator");
            return;
        }

        var addressPart = input[..slashIndex];
        var prefixPart = input[slashIndex..][1..];

        if (!IPAddress.TryParse(addressPart, out var address))
        {
            exception = new FormatException("Invalid IP address in CIDR notation");
            return;
        }

        if (!int.TryParse(prefixPart, out var prefixLength))
        {
            exception = new FormatException(
                "Invalid prefix length in CIDR notation: prefix length must be a non-negative integer"
            );
            return;
        }

        if (prefixLength < 0 || prefixLength > address.AddressFamily.MaxPrefixLength)
        {
            exception = new FormatException(
                $"Prefix length must be between 0 and {address.AddressFamily.MaxPrefixLength} for {address.AddressFamily.FriendlyName}"
            );
            return;
        }

        result = new CIDR { Address = address, PrefixLength = prefixLength };
    }

    /// <summary>
    /// Returns the CIDR value in <c>address/prefix</c> format.
    /// </summary>
    /// <returns>The CIDR string.</returns>
    public override string ToString() => $"{Address}/{PrefixLength}";
}
