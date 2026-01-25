using System.Net;
using WgConf.Extensions;

namespace WgConf;

public readonly struct CIDR
{
    public required IPAddress Address { get; init; }
    public required int PrefixLength { get; init; }

    // public static implicit operator CIDR(string input) => Parse(input);
    public static implicit operator CIDR(ReadOnlySpan<char> input) => Parse(input);

    public static bool TryParse(ReadOnlySpan<char> input, out CIDR result, out Exception? exception)
    {
        result = default;
        exception = null;

        ParseInternal(input, ref result, ref exception);
        return exception == null;
    }

    public static CIDR Parse(ReadOnlySpan<char> input)
    {
        CIDR result = default;
        Exception? exception = null;

        ParseInternal(input, ref result, ref exception);
        return exception != null ? throw exception : result;
    }

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

    public override string ToString() => $"{Address}/{PrefixLength}";
}
