namespace WgConf.Parsing;

public readonly struct Token
{
    public readonly TokenType Type;
    public readonly Position Position;
    public readonly Location Location;

    public Token(TokenType type, Position position, Location location)
    {
        Type = type;
        Position = position;
        Location = location;
    }
}
