using System.Collections;
using System.Runtime.CompilerServices;

namespace WgConf.Amnezia;

[CollectionBuilder(typeof(HeaderValue), nameof(Create))]
public readonly struct HeaderValue
{
    public readonly ulong Start;
    public readonly ulong? End;

    public HeaderValue(ulong start)
    {
        Start = start;
    }

    public HeaderValue(ulong start, ulong end)
    {
        if (end == start)
        {
            throw new ArgumentException(
                $"Use constructor with single parameter to initialize a single-value {nameof(HeaderValue)}",
                nameof(start)
            );
        }

        if (end < start)
        {
            throw new ArgumentException(
                $"{nameof(end)} cannot be less than {nameof(start)}",
                nameof(end)
            );
        }

        Start = start;
        End = end;
    }

    public static HeaderValue Create(ReadOnlySpan<ulong> range)
    {
        return new HeaderValue(range[0], range[1]);
    }

    public static implicit operator HeaderValue(ulong start) => new HeaderValue(start);

    public static implicit operator HeaderValue(ValueTuple<ulong, ulong> range) =>
        new HeaderValue(range.Item1, range.Item2);

    public static bool TryParse(ReadOnlySpan<char> input, out HeaderValue headerValue)
    {
        Exception? exception = null;
        headerValue = default;

        ParseInternal(input, ref headerValue, ref exception);
        return exception == null;
    }

    public static HeaderValue Parse(ReadOnlySpan<char> input)
    {
        HeaderValue result = default;
        Exception? exception = null;

        ParseInternal(input, ref result, ref exception);
        if (exception != null)
            throw exception;

        return result;
    }

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
            if (!ulong.TryParse(input, out ulong start))
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

        if (!ulong.TryParse(input[partRanges[0]], out ulong start1))
        {
            exception = new FormatException("Could not parse the first part of HeaderValue");
            return;
        }

        if (!ulong.TryParse(input[partRanges[1]], out ulong end))
        {
            exception = new FormatException("Could not parse the second part of HeaderValue");
            return;
        }

        result = [start1, end];
    }

    public IEnumerator<ulong> GetEnumerator()
    {
        List<ulong> list = [Start];
        if (End.HasValue)
            list.Add(End.Value);

        return list.GetEnumerator();
    }

    public override string ToString()
    {
        return End.HasValue ? $"{Start}-{End.Value}" : Start.ToString();
    }
}
