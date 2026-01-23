namespace WgConf.Amnezia;

public readonly struct IntegerRange
{
    public required int Start { get; init; }
    public required int End { get; init; }

    public static IntegerRange Parse(string s)
    {
        return Parse(s.AsSpan());
    }

    public static IntegerRange Parse(ReadOnlySpan<char> s)
    {
        var dashIndex = s.LastIndexOf('-');
        if (dashIndex == -1)
        {
            throw new FormatException(
                "IntegerRange must contain '-' separator (format: 'start-end')"
            );
        }

        if (dashIndex == 0)
        {
            throw new FormatException(
                "IntegerRange must contain '-' separator (format: 'start-end')"
            );
        }

        var startPart = s[..dashIndex].Trim();
        var endPart = s[(dashIndex + 1)..].Trim();

        if (!int.TryParse(startPart, out var start))
        {
            throw new FormatException(
                $"Invalid start value in IntegerRange: '{startPart.ToString()}'"
            );
        }

        if (!int.TryParse(endPart, out var end))
        {
            throw new FormatException($"Invalid end value in IntegerRange: '{endPart.ToString()}'");
        }

        return new IntegerRange { Start = start, End = end };
    }

    public static bool TryParse(string s, out IntegerRange result)
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
        return $"{Start}-{End}";
    }
}
