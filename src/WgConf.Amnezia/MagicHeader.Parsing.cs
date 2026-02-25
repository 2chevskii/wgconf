namespace WgConf.Amnezia;

public readonly partial struct MagicHeader
{
    /// <summary>
    /// Attempts to parse a header value from a character span.
    /// </summary>
    /// <param name="input">The input value.</param>
    /// <param name="magicHeader">The parsed header value when successful.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> input, out MagicHeader magicHeader)
    {
        Exception? exception = null;
        magicHeader = default;

        ParseInternal(input, ref magicHeader, ref exception);
        return exception == null;
    }

    /// <summary>
    /// Parses a header value from a character span.
    /// </summary>
    /// <param name="input">The input value.</param>
    /// <returns>The parsed header value.</returns>
    /// <exception cref="FormatException">Thrown when the input is invalid.</exception>
    public static MagicHeader Parse(ReadOnlySpan<char> input)
    {
        MagicHeader result = default;
        Exception? exception = null;

        ParseInternal(input, ref result, ref exception);
        return exception != null ? throw exception : result;
    }

    /// <summary>
    /// Parses a header value into the provided result and exception references.
    /// </summary>
    /// <param name="input">The input value.</param>
    /// <param name="result">The parsed header value when successful.</param>
    /// <param name="exception">The parsing exception when unsuccessful.</param>
    private static void ParseInternal(
        ReadOnlySpan<char> input,
        ref MagicHeader result,
        ref Exception? exception
    )
    {
        input = input.Trim();
        var dashIndex = input.IndexOf('-');

        if (dashIndex == -1)
        {
            if (!uint.TryParse(input, out uint start))
            {
                exception = new FormatException($"Invalid integer value in HeaderValue: '{input}'");
            }

            result = start;
            return;
        }

        Span<Range> partRanges = stackalloc Range[2];
        var partCount = input.Split(partRanges, '-');
        if (partCount != 2)
        {
            exception = new FormatException(
                "Header value should consist of exactly 2 unsigned long integers"
            );
            return;
        }

        if (!uint.TryParse(input[partRanges[0]], out uint start1))
        {
            exception = new FormatException("Could not parse the first part of HeaderValue");
            return;
        }

        if (!uint.TryParse(input[partRanges[1]], out uint end))
        {
            exception = new FormatException("Could not parse the second part of HeaderValue");
            return;
        }

        result = [start1, end];
    }
}
