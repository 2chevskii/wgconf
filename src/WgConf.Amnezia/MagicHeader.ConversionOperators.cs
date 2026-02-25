namespace WgConf.Amnezia;

public readonly partial struct MagicHeader
{
    /// <summary>
    /// Creates a single-value header from an unsigned long.
    /// </summary>
    /// <param name="start">The header value.</param>
    public static implicit operator MagicHeader(uint start) => new MagicHeader(start);

    /// <summary>
    /// Creates a header range from a tuple.
    /// </summary>
    /// <param name="range">The tuple containing start and end values.</param>
    public static implicit operator MagicHeader(ValueTuple<uint, uint> range) =>
        new MagicHeader(range.Item1, range.Item2);

    /// <summary>
    /// Parses a header value from <see cref="ReadOnlySpan{T}" />.
    /// </summary>
    /// <param name="input">The input value.</param>
    public static implicit operator MagicHeader(ReadOnlySpan<char> input) => Parse(input);
}
