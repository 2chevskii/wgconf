namespace WgConf.Parsing;

public enum TokenType
{
    EOF,
    Newline,
    Whitespace,
    Eq,
    BrOpen,
    BrClose,
    Comma,
    Number,
    String,
    Comment,
}
