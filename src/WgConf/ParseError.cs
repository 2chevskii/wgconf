namespace WgConf;

/// <summary>
/// Represents a parsing error encountered while reading a WireGuard configuration.
/// </summary>
/// <param name="LineNumber">The line number where the error occurred.</param>
/// <param name="Message">The error message.</param>
/// <param name="PropertyName">The related property name, if available.</param>
/// <param name="Section">The related section name, if available.</param>
/// <param name="LineText">The original line text, if available.</param>
/// <param name="SurroundingLines">Context lines around the error, if available.</param>
public record ParseError(
    int LineNumber,
    string Message,
    string? PropertyName = null,
    string? Section = null,
    string? LineText = null,
    string[]? SurroundingLines = null
)
{
    /// <summary>
    /// Returns a formatted error message including context when available.
    /// </summary>
    /// <returns>The formatted error message.</returns>
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
