using System.Buffers;

namespace WgConf.Parsing;

public ref struct Lexer
{
    private static readonly SearchValues<char> s_nonNumberOrStringChars = SearchValues.Create([
        ' ',
        '\t',
        ',',
        '=',
        '\r',
        '\n',
        '#',
        char.MaxValue,
        '[',
        ']',
    ]);

    private List<Token> _tokens;
    private ReadOnlySpan<char> _text;
    private int _index,
        _tokenStart;
    private Location _loc;

    public IReadOnlyList<Token> Tokenize(ReadOnlySpan<char> text)
    {
        Initialize(text);

        Token token;
        while ((token = NextToken()).Type != TokenType.EOF)
        {
            _tokens.Add(token);
            if (token.Type == TokenType.Newline)
            {
                _loc = _loc.Newline();
            }
            else
            {
                _loc = _loc.Advance(token.Position.Length);
            }
        }

        return _tokens;
    }

    private static bool IsNumberOrStringChar(char c) => !s_nonNumberOrStringChars.Contains(c);

    private Token NextToken()
    {
        _tokenStart = _index;
        return Peek() switch
        {
            char.MaxValue => Eof(),
            '\n' or '\r' => Newline(),
            ' ' or '\t' => Whitespace(),
            '[' => BrOpen(),
            ']' => BrClose(),
            '=' => Eq(),
            ',' => Comma(),
            '#' => Comment(),
            var c when char.IsDigit(c) => NumberOrString(),
            var _ => String(),
        };
    }

    private Token Eof()
    {
        return new Token(TokenType.EOF, new Position(_tokenStart, _index), _loc);
    }

    private Token Comment()
    {
        while (Peek() is not '\r' and not '\n' and not char.MaxValue)
        {
            Consume();
        }

        return new Token(TokenType.Comment, new Position(_tokenStart, _index), _loc);
    }

    private Token String()
    {
        while (IsNumberOrStringChar(Peek()))
            Consume();

        return new Token(TokenType.String, new Position(_tokenStart, _index), _loc);
    }

    private Token NumberOrString()
    {
        bool isNumber = true;
        char c;
        while (IsNumberOrStringChar(c = Peek()))
        {
            if (!char.IsDigit(c))
            {
                isNumber = false;
            }
            Consume();
        }

        return new Token(
            isNumber ? TokenType.Number : TokenType.String,
            new Position(_tokenStart, _index),
            _loc
        );
    }

    private Token Comma()
    {
        Consume();
        return new Token(TokenType.Comma, new Position(_tokenStart, _index), _loc);
    }

    private Token Eq()
    {
        Consume();
        return new Token(TokenType.Eq, new Position(_tokenStart, _index), _loc);
    }

    private Token BrOpen()
    {
        Consume();
        return new Token(TokenType.BrOpen, new Position(_tokenStart, _index), _loc);
    }

    private Token BrClose()
    {
        Consume();
        return new Token(TokenType.BrClose, new Position(_tokenStart, _index), _loc);
    }

    private Token Whitespace()
    {
        Consume();
        return new Token(TokenType.Whitespace, new Position(_tokenStart, _index), _loc);
    }

    private Token Newline()
    {
        var firstChar = Consume();
        if (firstChar == '\r' && Peek() == '\n')
        {
            Consume();
        }

        return new Token(TokenType.Newline, new Position(_tokenStart, _index), _loc);
    }

    private char Peek()
    {
        if (_index >= _text.Length)
            return char.MaxValue;

        return _text[_index];
    }

    private char Consume()
    {
        return _text[_index++];
    }

    private void Initialize(ReadOnlySpan<char> text)
    {
        _tokens = [];
        _text = text;
        _index = _tokenStart = 0;
        _loc = new Location(1, 1);
    }
}
