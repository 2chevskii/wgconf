namespace WgConf;

public record ParseError(
    int LineNumber,
    string Message,
    string? PropertyName = null,
    string? Section = null,
    string? LineText = null,
    string[]? SurroundingLines = null
)
{
    public override string ToString()
    {
        var message = $"Line {LineNumber}: {Message}";

        if (Section != null)
        {
            message += $" (in [{Section}] section)";
        }

        if (PropertyName != null)
        {
            message += $" [Property: {PropertyName}]";
        }

        if (SurroundingLines != null && SurroundingLines.Length > 0)
        {
            message += "\n\nContext:\n";
            var startLine = LineNumber - (SurroundingLines.Length / 2);
            for (var i = 0; i < SurroundingLines.Length; i++)
            {
                var lineNum = startLine + i;
                var marker = lineNum == LineNumber ? ">>> " : "    ";
                message += $"{marker}{lineNum, 4}: {SurroundingLines[i]}\n";
            }
        }

        return message;
    }
}
