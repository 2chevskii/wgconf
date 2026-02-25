using System.Runtime.CompilerServices;

namespace WgConf.Amnezia;

/// <summary>
/// Represents a single header value or a range of header values.
/// </summary>
[CollectionBuilder(typeof(MagicHeader), nameof(Create))]
public readonly partial struct MagicHeader
{
    /// <summary>
    /// The start value of the header or range.
    /// </summary>
    public readonly uint Start;

    /// <summary>
    /// The optional end value of the header range.
    /// </summary>
    public readonly uint End;

    /// <summary>
    /// Indicates that MagicHeader is range (Start != End)
    /// </summary>
    public bool IsRange => Start != End;

    /// <summary>
    /// Initializes MagicHeader with two values
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <exception cref="ArgumentException"></exception>
    public MagicHeader(uint start, uint end)
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
    public MagicHeader(uint value)
        : this(value, value) { }

    /// <summary>
    /// Creates a header range from a two-element span.
    /// </summary>
    /// <param name="range">A span containing the start and end values.</param>
    /// <returns>The created header value.</returns>
    public static MagicHeader Create(ReadOnlySpan<uint> range)
    {
        return new MagicHeader(range[0], range[1]);
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
