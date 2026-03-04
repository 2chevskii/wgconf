namespace WgConf.Parsing;

public readonly struct Location
{
    public readonly int Line,
        Column;

    public Location(int line, int column)
    {
        Line = line;
        Column = column;
    }

    public Location Newline()
    {
        return new Location(Line + 1, 1);
    }

    public Location Advance(int columns = 1)
    {
        return new Location(Line, Column + columns);
    }
}
