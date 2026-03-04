namespace WgConf.Tests.Parsing;

using WgConf.Parsing;

public class LexerTests
{
    // Stack<T>.ToArray() returns elements in LIFO order (most recently pushed first).
    // Tokens are pushed in encounter order, so we reverse to get them in source order.
    private static Token[] Tokenize(string text)
    {
        var lexer = new Lexer();
        var stack = lexer.Tokenize(text.AsSpan());
        var arr = stack.ToArray();
        return arr;
    }

    #region Empty / EOF

    [Fact]
    public void Tokenize_EmptyString_ReturnsNoTokens()
    {
        var tokens = Tokenize("");
        Assert.Empty(tokens);
    }

    [Fact]
    public void Tokenize_OnlyWhitespace_ReturnsWhitespaceTokens()
    {
        var tokens = Tokenize("   ");
        Assert.Equal(3, tokens.Length);
        Assert.All(tokens, t => Assert.Equal(TokenType.Whitespace, t.Type));
    }

    [Fact]
    public void Tokenize_OnlyNewlines_ReturnsNewlineTokens()
    {
        var tokens = Tokenize("\n\n");
        Assert.Equal(2, tokens.Length);
        Assert.All(tokens, t => Assert.Equal(TokenType.Newline, t.Type));
    }

    #endregion

    #region Single-character tokens

    [Fact]
    public void Tokenize_BracketOpen_ReturnsBrOpenToken()
    {
        var tokens = Tokenize("[");
        Assert.Single(tokens);
        Assert.Equal(TokenType.BrOpen, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_BracketClose_ReturnsBrCloseToken()
    {
        var tokens = Tokenize("]");
        Assert.Single(tokens);
        Assert.Equal(TokenType.BrClose, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_Equals_ReturnsEqToken()
    {
        var tokens = Tokenize("=");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Eq, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_Comma_ReturnsCommaToken()
    {
        var tokens = Tokenize(",");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Comma, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_Space_ReturnsWhitespaceToken()
    {
        var tokens = Tokenize(" ");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Whitespace, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_Tab_ReturnsWhitespaceToken()
    {
        var tokens = Tokenize("\t");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Whitespace, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_LineFeed_ReturnsNewlineToken()
    {
        var tokens = Tokenize("\n");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Newline, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_CarriageReturn_ReturnsNewlineToken()
    {
        var tokens = Tokenize("\r");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Newline, tokens[0].Type);
    }

    #endregion

    #region Newline variants

    [Fact]
    public void Tokenize_CrLf_ReturnsSingleNewlineToken()
    {
        // \r\n should be treated as a single newline token
        var tokens = Tokenize("\r\n");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Newline, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_CrLf_HasLengthTwo()
    {
        var tokens = Tokenize("\r\n");
        Assert.Equal(2, tokens[0].Position.Length);
    }

    [Fact]
    public void Tokenize_CrFollowedByNonLf_ReturnsSingleCharNewline()
    {
        var tokens = Tokenize("\ra");
        Assert.Equal(2, tokens.Length);
        Assert.Equal(TokenType.Newline, tokens[0].Type);
        Assert.Equal(1, tokens[0].Position.Length);
        Assert.Equal(TokenType.String, tokens[1].Type);
    }

    [Fact]
    public void Tokenize_MultipleMixedNewlines_ReturnsCorrectCount()
    {
        var tokens = Tokenize("\n\r\n\r");
        // \n, \r\n, \r → 3 newline tokens
        Assert.Equal(3, tokens.Length);
        Assert.All(tokens, t => Assert.Equal(TokenType.Newline, t.Type));
    }

    #endregion

    #region Numbers and strings

    [Fact]
    public void Tokenize_PureDigits_ReturnsNumberToken()
    {
        var tokens = Tokenize("51820");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Number, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_PureLetters_ReturnsStringToken()
    {
        var tokens = Tokenize("Interface");
        Assert.Single(tokens);
        Assert.Equal(TokenType.String, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_DigitsFollowedByLetters_ReturnsStringToken()
    {
        var tokens = Tokenize("10abc");
        Assert.Single(tokens);
        Assert.Equal(TokenType.String, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_LettersFollowedByDigits_ReturnsStringToken()
    {
        var tokens = Tokenize("abc123");
        Assert.Single(tokens);
        Assert.Equal(TokenType.String, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_SingleDigit_ReturnsNumberToken()
    {
        var tokens = Tokenize("0");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Number, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_SingleLetter_ReturnsStringToken()
    {
        var tokens = Tokenize("x");
        Assert.Single(tokens);
        Assert.Equal(TokenType.String, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_MixedAlphanumericStartingWithLetter_ReturnsStringToken()
    {
        var tokens = Tokenize("wg0");
        Assert.Single(tokens);
        Assert.Equal(TokenType.String, tokens[0].Type);
    }

    #endregion

    #region Comments

    [Fact]
    public void Tokenize_HashOnly_ReturnsCommentToken()
    {
        var tokens = Tokenize("#");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Comment, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_CommentWithText_ReturnsCommentToken()
    {
        var tokens = Tokenize("# this is a comment");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Comment, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_CommentStopsAtLineFeed_ExcludesNewlineFromComment()
    {
        var tokens = Tokenize("# comment\nnext");
        Assert.Equal(3, tokens.Length);
        Assert.Equal(TokenType.Comment, tokens[0].Type);
        Assert.Equal(TokenType.Newline, tokens[1].Type);
        Assert.Equal(TokenType.String, tokens[2].Type);
    }

    [Fact]
    public void Tokenize_CommentStopsAtCarriageReturn_ExcludesNewlineFromComment()
    {
        var tokens = Tokenize("# comment\rnext");
        Assert.Equal(3, tokens.Length);
        Assert.Equal(TokenType.Comment, tokens[0].Type);
        Assert.Equal(TokenType.Newline, tokens[1].Type);
        Assert.Equal(TokenType.String, tokens[2].Type);
    }

    [Fact]
    public void Tokenize_CommentAtEndOfFile_ConsumesEntireComment()
    {
        const string text = "# end of file comment";
        var tokens = Tokenize(text);
        Assert.Single(tokens);
        Assert.Equal(TokenType.Comment, tokens[0].Type);
        Assert.Equal(text.Length, tokens[0].Position.Length);
    }

    #endregion

    #region Position accuracy

    [Fact]
    public void Tokenize_SingleToken_PositionStartIsZero()
    {
        var tokens = Tokenize("abc");
        Assert.Equal(0, tokens[0].Position.Start);
    }

    [Fact]
    public void Tokenize_SingleToken_PositionEndEqualsLength()
    {
        var tokens = Tokenize("Interface");
        Assert.Equal(9, tokens[0].Position.End);
        Assert.Equal(9, tokens[0].Position.Length);
    }

    [Fact]
    public void Tokenize_BrOpen_HasLengthOne()
    {
        var tokens = Tokenize("[");
        Assert.Equal(0, tokens[0].Position.Start);
        Assert.Equal(1, tokens[0].Position.End);
        Assert.Equal(1, tokens[0].Position.Length);
    }

    [Fact]
    public void Tokenize_BrClose_HasLengthOne()
    {
        var tokens = Tokenize("]");
        Assert.Equal(1, tokens[0].Position.End);
        Assert.Equal(1, tokens[0].Position.Length);
    }

    [Fact]
    public void Tokenize_MultipleTokens_PositionsAreContiguous()
    {
        // "[Interface]"  → positions: [0,1), [1,10), [10,11)
        var tokens = Tokenize("[Interface]");
        Assert.Equal(3, tokens.Length);
        Assert.Equal(0, tokens[0].Position.Start);
        Assert.Equal(1, tokens[0].Position.End);
        Assert.Equal(1, tokens[1].Position.Start);
        Assert.Equal(10, tokens[1].Position.End);
        Assert.Equal(10, tokens[2].Position.Start);
        Assert.Equal(11, tokens[2].Position.End);
    }

    [Fact]
    public void Tokenize_NumberToken_PositionMatchesDigitSpan()
    {
        // "key = 12345" → String(0,3), WS(3,4), Eq(4,5), WS(5,6), Number(6,11)
        var tokens = Tokenize("key = 12345");
        var number = tokens[4];
        Assert.Equal(TokenType.Number, number.Type);
        Assert.Equal(6, number.Position.Start);
        Assert.Equal(11, number.Position.End);
        Assert.Equal(5, number.Position.Length);
    }

    [Fact]
    public void Tokenize_CrLfNewline_PositionSpansTwoChars()
    {
        var tokens = Tokenize("\r\n");
        Assert.Equal(0, tokens[0].Position.Start);
        Assert.Equal(2, tokens[0].Position.End);
    }

    [Fact]
    public void Tokenize_CommentFollowedByNewline_CommentPositionExcludesNewline()
    {
        // "# hi\n" → Comment(0,4), Newline(4,5)
        var tokens = Tokenize("# hi\n");
        Assert.Equal(0, tokens[0].Position.Start);
        Assert.Equal(4, tokens[0].Position.End);
        Assert.Equal(4, tokens[1].Position.Start);
        Assert.Equal(5, tokens[1].Position.End);
    }

    #endregion

    #region Composite: WireGuard config patterns

    [Fact]
    public void Tokenize_SectionHeader_ReturnsCorrectSequence()
    {
        // "[Interface]" → BrOpen, String, BrClose
        var tokens = Tokenize("[Interface]");
        Assert.Equal(3, tokens.Length);
        Assert.Equal(TokenType.BrOpen, tokens[0].Type);
        Assert.Equal(TokenType.String, tokens[1].Type);
        Assert.Equal(TokenType.BrClose, tokens[2].Type);
    }

    [Fact]
    public void Tokenize_PeerSectionHeader_ReturnsCorrectSequence()
    {
        var tokens = Tokenize("[Peer]");
        Assert.Equal(3, tokens.Length);
        Assert.Equal(TokenType.BrOpen, tokens[0].Type);
        Assert.Equal(TokenType.String, tokens[1].Type);
        Assert.Equal(TokenType.BrClose, tokens[2].Type);
    }

    [Fact]
    public void Tokenize_KeyEqualsValue_ReturnsCorrectSequence()
    {
        // "ListenPort = 51820" → String, WS, Eq, WS, Number
        var tokens = Tokenize("ListenPort = 51820");
        Assert.Equal(5, tokens.Length);
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal(TokenType.Whitespace, tokens[1].Type);
        Assert.Equal(TokenType.Eq, tokens[2].Type);
        Assert.Equal(TokenType.Whitespace, tokens[3].Type);
        Assert.Equal(TokenType.Number, tokens[4].Type);
    }

    [Fact]
    public void Tokenize_KeyEqualsStringValue_ReturnsCorrectSequence()
    {
        // "PrivateKey = abc123" → String, WS, Eq, WS, String
        var tokens = Tokenize("PrivateKey = abc123");
        Assert.Equal(5, tokens.Length);
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal(TokenType.Whitespace, tokens[1].Type);
        Assert.Equal(TokenType.Eq, tokens[2].Type);
        Assert.Equal(TokenType.Whitespace, tokens[3].Type);
        Assert.Equal(TokenType.String, tokens[4].Type);
    }

    [Fact]
    public void Tokenize_SectionHeaderWithNewline_ReturnsNewlineAfterBrClose()
    {
        var tokens = Tokenize("[Interface]\n");
        Assert.Equal(4, tokens.Length);
        Assert.Equal(TokenType.BrOpen, tokens[0].Type);
        Assert.Equal(TokenType.String, tokens[1].Type);
        Assert.Equal(TokenType.BrClose, tokens[2].Type);
        Assert.Equal(TokenType.Newline, tokens[3].Type);
    }

    [Fact]
    public void Tokenize_CommentLine_ReturnsCommentThenNewline()
    {
        var tokens = Tokenize("# WireGuard config\n");
        Assert.Equal(2, tokens.Length);
        Assert.Equal(TokenType.Comment, tokens[0].Type);
        Assert.Equal(TokenType.Newline, tokens[1].Type);
    }

    [Fact]
    public void Tokenize_KeyValueWithTab_RecognizesTabAsWhitespace()
    {
        // "Key\t=\tValue"
        var tokens = Tokenize("Key\t=\tValue");
        Assert.Equal(5, tokens.Length);
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal(TokenType.Whitespace, tokens[1].Type);
        Assert.Equal(TokenType.Eq, tokens[2].Type);
        Assert.Equal(TokenType.Whitespace, tokens[3].Type);
        Assert.Equal(TokenType.String, tokens[4].Type);
    }

    [Fact]
    public void Tokenize_MultipleEqualSigns_EachBecomesItsOwnToken()
    {
        var tokens = Tokenize("a = b = c");
        Assert.Equal(9, tokens.Length);
        Assert.Equal(TokenType.Eq, tokens[2].Type);
        Assert.Equal(TokenType.Eq, tokens[6].Type);
    }

    [Fact]
    public void Tokenize_EmptySectionName_ReturnsBrOpenAndBrClose()
    {
        // "[]" — no section name string token between brackets
        var tokens = Tokenize("[]");
        Assert.Equal(2, tokens.Length);
        Assert.Equal(TokenType.BrOpen, tokens[0].Type);
        Assert.Equal(TokenType.BrClose, tokens[1].Type);
    }

    [Fact]
    public void Tokenize_ListenPortLine_NumberTokenContainsPortValue()
    {
        var tokens = Tokenize("ListenPort = 51820");
        var number = tokens.Single(t => t.Type == TokenType.Number);
        Assert.Equal(5, number.Position.Length); // "51820" is 5 chars
    }

    #endregion

    #region Multi-line inputs

    [Fact]
    public void Tokenize_TwoKeyValueLines_ReturnsTokensForBothLines()
    {
        const string text = "PortA = 100\nPortB = 200";
        var tokens = Tokenize(text);

        // "PortA", " ", "=", " ", "100", "\n", "PortB", " ", "=", " ", "200"
        Assert.Equal(11, tokens.Length);
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal(TokenType.Number, tokens[4].Type);
        Assert.Equal(TokenType.Newline, tokens[5].Type);
        Assert.Equal(TokenType.String, tokens[6].Type);
        Assert.Equal(TokenType.Number, tokens[10].Type);
    }

    [Fact]
    public void Tokenize_BlankLineBetweenSections_ReturnsDoubleNewline()
    {
        var tokens = Tokenize("[Interface]\n\n[Peer]");
        var newlines = tokens.Where(t => t.Type == TokenType.Newline).ToArray();
        Assert.Equal(2, newlines.Length);
    }

    [Fact]
    public void Tokenize_CommentBeforeSectionHeader_CorrectOrder()
    {
        var tokens = Tokenize("# comment\n[Interface]");
        Assert.Equal(TokenType.Comment, tokens[0].Type);
        Assert.Equal(TokenType.Newline, tokens[1].Type);
        Assert.Equal(TokenType.BrOpen, tokens[2].Type);
        Assert.Equal(TokenType.String, tokens[3].Type);
        Assert.Equal(TokenType.BrClose, tokens[4].Type);
    }

    [Fact]
    public void Tokenize_WindowsLineEndings_SectionsTokenizeCorrectly()
    {
        var tokens = Tokenize("[Interface]\r\n[Peer]");
        Assert.Equal(TokenType.BrOpen, tokens[0].Type);
        Assert.Equal(TokenType.String, tokens[1].Type);
        Assert.Equal(TokenType.BrClose, tokens[2].Type);
        Assert.Equal(TokenType.Newline, tokens[3].Type);
        Assert.Equal(1, tokens.Count(t => t.Type == TokenType.Newline));
    }

    [Fact]
    public void Tokenize_NumbersOnSeparateLines_EachIsNumberToken()
    {
        var tokens = Tokenize("100\n200\n300");
        var numbers = tokens.Where(t => t.Type == TokenType.Number).ToArray();
        Assert.Equal(3, numbers.Length);
    }

    #endregion

    // Does NOT reverse — used to verify the natural enumeration order of Tokenize()'s return value.
    // Currently fails (LIFO) because Tokenize() returns Stack<Token>; intended to pass once the
    // return type becomes IEnumerable<Token> in source order.
    // NOTE: once that change is made, the existing Tokenize() helper above must also drop its
    //       Array.Reverse() call, or all existing tests will start returning tokens backwards.
    private static Token[] TokenizeInOrder(string text)
    {
        var lexer = new Lexer();
        return [.. lexer.Tokenize(text.AsSpan())];
    }

    #region FIFO ordering

    [Fact]
    public void Tokenize_SectionHeader_FirstTokenIsBrOpen()
    {
        // With Stack<Token> (LIFO), first element of ToArray() is BrClose — test must fail until FIFO.
        var tokens = TokenizeInOrder("[Interface]");
        Assert.Equal(TokenType.BrOpen, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_SectionHeader_LastTokenIsBrClose()
    {
        var tokens = TokenizeInOrder("[Interface]");
        Assert.Equal(TokenType.BrClose, tokens[^1].Type);
    }

    [Fact]
    public void Tokenize_SectionHeader_MiddleTokenIsString()
    {
        var tokens = TokenizeInOrder("[Interface]");
        Assert.Equal(TokenType.String, tokens[1].Type);
    }

    [Fact]
    public void Tokenize_KeyValueLine_TokensAreInSourceOrder()
    {
        // Expected: String, Whitespace, Eq, Whitespace, Number
        var tokens = TokenizeInOrder("Port = 51820");
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal(TokenType.Whitespace, tokens[1].Type);
        Assert.Equal(TokenType.Eq, tokens[2].Type);
        Assert.Equal(TokenType.Whitespace, tokens[3].Type);
        Assert.Equal(TokenType.Number, tokens[4].Type);
    }

    [Fact]
    public void Tokenize_MultiLine_SectionHeadersAreInEncounterOrder()
    {
        var tokens = TokenizeInOrder("[Interface]\n[Peer]");
        // First BrOpen must come before the second BrOpen
        var opens = tokens
            .Select((t, i) => (t, i))
            .Where(x => x.t.Type == TokenType.BrOpen)
            .ToArray();
        Assert.Equal(2, opens.Length);
        Assert.True(opens[0].i < opens[1].i);
    }

    [Fact]
    public void Tokenize_CommentThenSection_CommentIsFirst()
    {
        var tokens = TokenizeInOrder("# comment\n[Interface]");
        Assert.Equal(TokenType.Comment, tokens[0].Type);
        Assert.Equal(TokenType.Newline, tokens[1].Type);
        Assert.Equal(TokenType.BrOpen, tokens[2].Type);
    }

    #endregion

    #region Location tracking

    // ── These tests will not compile until Token gains a `Location Location` field. ────────────

    [Fact]
    public void Tokenize_FirstToken_IsAtLine1Column1()
    {
        var tokens = TokenizeInOrder("[");
        Assert.Equal(1, tokens[0].Location.Line);
        Assert.Equal(1, tokens[0].Location.Column);
    }

    [Fact]
    public void Tokenize_SectionHeader_StringTokenStartsAtColumn2()
    {
        // "[Interface]" — 'I' is at column 2
        var tokens = TokenizeInOrder("[Interface]");
        var str = tokens[1];
        Assert.Equal(TokenType.String, str.Type);
        Assert.Equal(1, str.Location.Line);
        Assert.Equal(2, str.Location.Column);
    }

    [Fact]
    public void Tokenize_SectionHeader_BrCloseStartsAtCorrectColumn()
    {
        // "[Interface]" — ']' is at column 11
        var tokens = TokenizeInOrder("[Interface]");
        var close = tokens[2];
        Assert.Equal(TokenType.BrClose, close.Type);
        Assert.Equal(1, close.Location.Line);
        Assert.Equal(11, close.Location.Column);
    }

    [Fact]
    public void Tokenize_TokenAfterLineFeed_IsOnLine2Column1()
    {
        var tokens = TokenizeInOrder("[Interface]\n[Peer]");
        // tokens: BrOpen(1,1), String(1,2), BrClose(1,11), Newline(1,12), BrOpen(2,1), ...
        var secondOpen = tokens[4];
        Assert.Equal(TokenType.BrOpen, secondOpen.Type);
        Assert.Equal(2, secondOpen.Location.Line);
        Assert.Equal(1, secondOpen.Location.Column);
    }

    [Fact]
    public void Tokenize_TokenAfterCrLf_IsOnLine2Column1()
    {
        var tokens = TokenizeInOrder("[Interface]\r\n[Peer]");
        // \r\n is a single Newline token, next token is on line 2
        var secondOpen = tokens[4];
        Assert.Equal(TokenType.BrOpen, secondOpen.Type);
        Assert.Equal(2, secondOpen.Location.Line);
        Assert.Equal(1, secondOpen.Location.Column);
    }

    [Fact]
    public void Tokenize_ThreeLines_ThirdSectionIsOnLine3()
    {
        var tokens = TokenizeInOrder("[Interface]\n\n[Peer]");
        // BrOpen(1,1) String(1,2) BrClose(1,11) NL(1,12) NL(2,1) BrOpen(3,1) ...
        var thirdOpen = tokens.Where(t => t.Type == TokenType.BrOpen).ElementAt(1);
        Assert.Equal(3, thirdOpen.Location.Line);
        Assert.Equal(1, thirdOpen.Location.Column);
    }

    [Fact]
    public void Tokenize_KeyValueLine_EachTokenHasCorrectColumn()
    {
        // "Key = 42"
        // K(1,1) e(1,2) y(1,3) = "Key" at col 1, length 3
        // ' '  at col 4
        // '='  at col 5
        // ' '  at col 6
        // "42" at col 7
        var tokens = TokenizeInOrder("Key = 42");
        Assert.Equal(1, tokens[0].Location.Column); // "Key"
        Assert.Equal(4, tokens[1].Location.Column); // ' '
        Assert.Equal(5, tokens[2].Location.Column); // '='
        Assert.Equal(6, tokens[3].Location.Column); // ' '
        Assert.Equal(7, tokens[4].Location.Column); // "42"
    }

    [Fact]
    public void Tokenize_CommentLine_CommentTokenStartsAtColumn1()
    {
        var tokens = TokenizeInOrder("# this is a comment");
        Assert.Equal(1, tokens[0].Location.Line);
        Assert.Equal(1, tokens[0].Location.Column);
    }

    [Fact]
    public void Tokenize_CommentMidLine_CommentTokenHasCorrectColumn()
    {
        // "key = value # comment" — comment starts after the value
        // Exact column depends on how many chars precede it; test structure, not magic number
        var tokens = TokenizeInOrder("ab # note");
        var comment = tokens.Single(t => t.Type == TokenType.Comment);
        Assert.Equal(1, comment.Location.Line);
        Assert.True(comment.Location.Column > 1);
    }

    [Fact]
    public void Tokenize_NewlineToken_HasLocationOfItsOwnPosition()
    {
        var tokens = TokenizeInOrder("ab\ncd");
        var nl = tokens.Single(t => t.Type == TokenType.Newline);
        Assert.Equal(1, nl.Location.Line);
        Assert.Equal(3, nl.Location.Column);
    }

    #endregion

    #region Invalid / edge-case WireGuard inputs

    [Fact]
    public void Tokenize_OnlyComment_ReturnsCommentTokenWithFullLength()
    {
        const string text = "# no newline at end";
        var tokens = Tokenize(text);
        Assert.Single(tokens);
        Assert.Equal(TokenType.Comment, tokens[0].Type);
        Assert.Equal(text.Length, tokens[0].Position.Length);
    }

    [Fact]
    public void Tokenize_MultipleEqSignsNoValue_AllEqsPresent()
    {
        // Semantically invalid WireGuard, but lexically fine
        var tokens = Tokenize("key==");
        Assert.Equal(3, tokens.Length);
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal(TokenType.Eq, tokens[1].Type);
        Assert.Equal(TokenType.Eq, tokens[2].Type);
    }

    [Fact]
    public void Tokenize_UnclosedSectionHeader_NoBrCloseToken()
    {
        // "[Interface" (no closing bracket) — semantically invalid but lexically produces BrOpen + String
        var tokens = Tokenize("[Interface");
        Assert.Equal(2, tokens.Length);
        Assert.Equal(TokenType.BrOpen, tokens[0].Type);
        Assert.Equal(TokenType.String, tokens[1].Type);
        Assert.DoesNotContain(tokens, t => t.Type == TokenType.BrClose);
    }

    [Fact]
    public void Tokenize_ValueWithOnlyNumber_ReturnsNumberNotString()
    {
        var tokens = Tokenize("Field = 0");
        var value = tokens.Last();
        Assert.Equal(TokenType.Number, value.Type);
    }

    [Fact]
    public void Tokenize_ZeroPaddedNumber_ReturnsNumberToken()
    {
        var tokens = Tokenize("0051820");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Number, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_MissingValueAfterEquals_ReturnsEqWithNoFollowingValueToken()
    {
        // "Key = " — value is just whitespace
        var tokens = Tokenize("Key = ");
        Assert.Equal(4, tokens.Length);
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal(TokenType.Whitespace, tokens[1].Type);
        Assert.Equal(TokenType.Eq, tokens[2].Type);
        Assert.Equal(TokenType.Whitespace, tokens[3].Type);
    }

    [Fact]
    public void Tokenize_ConsecutiveCommas_EachIsOwnToken()
    {
        var tokens = Tokenize(",,,");
        Assert.Equal(3, tokens.Length);
        Assert.All(tokens, t => Assert.Equal(TokenType.Comma, t.Type));
    }

    [Fact]
    public void Tokenize_NumberInSectionName_ReturnsStringToken()
    {
        // [Section1] — section name contains digit → String (starts with letter)
        var tokens = Tokenize("[Section1]");
        Assert.Equal(3, tokens.Length);
        Assert.Equal(TokenType.String, tokens[1].Type);
    }

    [Fact]
    public void Tokenize_LargeNumber_ReturnsNumberToken()
    {
        var tokens = Tokenize("4294967295");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal(10, tokens[0].Position.Length);
    }

    [Fact]
    public void Tokenize_AlphanumericKey_ReturnsStringToken()
    {
        // Keys like "wg0" or "eth1" are alphanumeric but start with letter
        var tokens = Tokenize("wg0");
        Assert.Single(tokens);
        Assert.Equal(TokenType.String, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_RepeatedSections_TokenCountIsAccumulated()
    {
        const string text = "[Interface]\n[Peer]\n[Peer]";
        var tokens = Tokenize(text);
        var brOpens = tokens.Count(t => t.Type == TokenType.BrOpen);
        var brCloses = tokens.Count(t => t.Type == TokenType.BrClose);
        Assert.Equal(3, brOpens);
        Assert.Equal(3, brCloses);
    }

    #endregion

    #region Real WireGuard configuration scenarios

    // ── These tests use a short timeout so the known infinite-loop bug on unrecognised characters
    //    fails cleanly rather than hanging the test runner. They will pass once String() is fixed
    //    to consume any character that is not a recognised delimiter. ──────────────────────────────

    [Fact(Timeout = 3000)]
    public void Tokenize_IpAddress_CompletesAndReturnsTokens()
    {
        // '.' is not handled by any branch — currently causes infinite loop
        var tokens = TokenizeInOrder("10.0.0.1");
        Assert.NotEmpty(tokens);
    }

    [Fact(Timeout = 3000)]
    public void Tokenize_CidrNotation_CompletesAndReturnsTokens()
    {
        // '.' and '/' are both unhandled
        var tokens = TokenizeInOrder("10.0.0.1/24");
        Assert.NotEmpty(tokens);
    }

    [Fact(Timeout = 3000)]
    public void Tokenize_DnsHostname_CompletesAndReturnsTokens()
    {
        // '.' and '-' are unhandled
        var tokens = TokenizeInOrder("vpn.example.com");
        Assert.NotEmpty(tokens);
    }

    [Fact(Timeout = 3000)]
    public void Tokenize_Base64Key_CompletesAndReturnsTokens()
    {
        // '+' and '/' are unhandled; trailing '=' is the Eq token — value ends prematurely today
        const string key = "YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=";
        var tokens = TokenizeInOrder(key);
        Assert.NotEmpty(tokens);
    }

    [Fact(Timeout = 3000)]
    public void Tokenize_AddressLine_ProducesKeyEqAndValueTokens()
    {
        // "Address = 10.0.0.1/24"
        // Expected: String("Address"), WS, Eq, WS, <value tokens for "10.0.0.1/24">
        var tokens = TokenizeInOrder("Address = 10.0.0.1/24");
        Assert.Equal(TokenType.String, tokens[0].Type); // "Address"
        Assert.Equal(TokenType.Whitespace, tokens[1].Type);
        Assert.Equal(TokenType.Eq, tokens[2].Type);
        Assert.Equal(TokenType.Whitespace, tokens[3].Type);
        // The IP+prefix should appear as at least one token after the '='
        Assert.True(tokens.Length > 4);
    }

    [Fact(Timeout = 3000)]
    public void Tokenize_EndpointLine_ProducesKeyEqAndValueTokens()
    {
        // "Endpoint = vpn.example.com:51820"
        var tokens = TokenizeInOrder("Endpoint = vpn.example.com:51820");
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal(TokenType.Eq, tokens[2].Type);
        Assert.True(tokens.Length > 4);
    }

    [Fact(Timeout = 3000)]
    public void Tokenize_AllowedIpsLine_ProducesKeyEqAndValueTokens()
    {
        // "AllowedIPs = 10.0.0.2/32, 10.0.0.3/32"
        var tokens = TokenizeInOrder("AllowedIPs = 10.0.0.2/32, 10.0.0.3/32");
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal(TokenType.Eq, tokens[2].Type);
        Assert.True(tokens.Length > 4);
    }

    [Fact(Timeout = 5000)]
    public void Tokenize_FullInterfaceSection_CompletesSuccessfully()
    {
        const string section = """
            [Interface]
            PrivateKey = YAnz5TF+lXXJte14tji3zlMNftqN9xFSeRCFKtheBGY=
            ListenPort = 51820
            Address = 10.0.0.1/24
            """;

        var tokens = TokenizeInOrder(section);
        Assert.NotEmpty(tokens);
        // Must contain at least the section header structure
        Assert.Contains(tokens, t => t.Type == TokenType.BrOpen);
        Assert.Contains(tokens, t => t.Type == TokenType.BrClose);
        Assert.Contains(tokens, t => t.Type == TokenType.Eq);
    }

    [Fact(Timeout = 5000)]
    public void Tokenize_FullPeerSection_CompletesSuccessfully()
    {
        const string section = """
            [Peer]
            PublicKey = xTIBA5rboUvnH4htodjb6e697QjLERt1NAB4mZqp8Dg=
            AllowedIPs = 10.0.0.2/32
            Endpoint = vpn.example.com:51820
            """;

        var tokens = TokenizeInOrder(section);
        Assert.NotEmpty(tokens);
        Assert.Contains(tokens, t => t.Type == TokenType.BrOpen);
        Assert.Contains(tokens, t => t.Type == TokenType.Eq);
    }

    #endregion
}
