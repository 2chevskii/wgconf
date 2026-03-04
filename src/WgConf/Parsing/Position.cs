namespace WgConf.Parsing;

public readonly struct Position
{
    public readonly int Start,
        End;

    public int Length => End - Start;

    public Position(int start, int end)
    {
        Start = start;
        End = end;
    }
}
