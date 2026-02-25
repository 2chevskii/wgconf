using System.Collections;
using System.Runtime.CompilerServices;

namespace WgConf.Amnezia;

/// <summary>
/// Represents a single header value or a range of header values.
/// </summary>
[CollectionBuilder(typeof(HeaderValue), nameof(Create))]
public readonly struct HeaderValue
{
    /// <summary>
    /// The start value of the header or range.
    /// </summary>
    public readonly uint Start;

    /// <summary>
    /// The optional end value of the header range.
    /// </summary>
    public readonly uint End;

    public bool IsRange => Start != End;

    public HeaderValue(uint start, uint end)
    {
        if (end < start)
        {
            throw new ArgumentException("End cannot be less than Start", nameof(end));
        }

        Start = start;
        End = end;
    }

    /// <summary>
    /// Initializes a single-value header with the specified start value.
    /// </summary>
    /// <param name="value">The header value (Start == End).</param>
    public HeaderValue(uint value)
        : this(value, value) { }

    /// <summary>
    /// Creates a header range from a two-element span.
    /// </summary>
    /// <param name="range">A span containing the start and end values.</param>
    /// <returns>The created header value.</returns>
    public static HeaderValue Create(ReadOnlySpan<uint> range)
    {
        return new HeaderValue(range[0], range[1]);
    }

    /// <summary>
    /// Creates a single-value header from an unsigned long.
    /// </summary>
    /// <param name="start">The header value.</param>
    public static implicit operator HeaderValue(uint start) => new HeaderValue(start);

    /// <summary>
    /// Creates a header range from a tuple.
    /// </summary>
    /// <param name="range">The tuple containing start and end values.</param>
    public static implicit operator HeaderValue(ValueTuple<uint, uint> range) =>
        new HeaderValue(range.Item1, range.Item2);

    /// <summary>
    /// Parses a header value from a string.
    /// </summary>
    /// <param name="input">The input value.</param>
    public static implicit operator HeaderValue(string input) => Parse(input);

    /// <summary>
    /// Attempts to parse a header value from a character span.
    /// </summary>
    /// <param name="input">The input value.</param>
    /// <param name="headerValue">The parsed header value when successful.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> input, out HeaderValue headerValue)
    {
        Exception? exception = null;
        headerValue = default;

        ParseInternal(input, ref headerValue, ref exception);
        return exception == null;
    }

    /// <summary>
    /// Parses a header value from a character span.
    /// </summary>
    /// <param name="input">The input value.</param>
    /// <returns>The parsed header value.</returns>
    /// <exception cref="FormatException">Thrown when the input is invalid.</exception>
    public static HeaderValue Parse(ReadOnlySpan<char> input)
    {
        HeaderValue result = default;
        Exception? exception = null;

        ParseInternal(input, ref result, ref exception);
        if (exception != null)
            throw exception;

        return result;
    }

    /// <summary>
    /// Parses a header value into the provided result and exception references.
    /// </summary>
    /// <param name="input">The input value.</param>
    /// <param name="result">The parsed header value when successful.</param>
    /// <param name="exception">The parsing exception when unsuccessful.</param>
    private static void ParseInternal(
        ReadOnlySpan<char> input,
        ref HeaderValue result,
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

    /// <summary>
    /// Returns an enumerator for the header values.
    /// </summary>
    /// <returns>An enumerator over the header values.</returns>
    public IEnumerator<uint> GetEnumerator()
    {
        var enumerable = Enumerable.Empty<uint>().Append(Start);
        if (IsRange)
            enumerable = enumerable.Append(End);

        return enumerable.GetEnumerator();
    }

    /// <summary>
    /// Returns the header value formatted as a single value or range.
    /// </summary>
    /// <returns>The formatted header value.</returns>
    public override string ToString()
    {
        return IsRange ? $"{Start}-{End}" : Start.ToString();
    }
}
