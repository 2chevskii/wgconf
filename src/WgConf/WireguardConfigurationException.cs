namespace WgConf;

/// <summary>
/// Represents errors that occur while parsing a WireGuard configuration.
/// </summary>
/// <param name="errors">The parse errors that caused the exception.</param>
public class WireguardConfigurationException(IReadOnlyList<ParseError> errors)
    : Exception(BuildMessage(errors))
{
    /// <summary>
    /// Gets the parse errors that caused the exception.
    /// </summary>
    public IReadOnlyList<ParseError> Errors { get; } = errors;

    /// <summary>
    /// Builds the exception message from a list of parse errors.
    /// </summary>
    /// <param name="errors">The parse errors to include in the message.</param>
    /// <returns>The formatted exception message.</returns>
    private static string BuildMessage(IReadOnlyList<ParseError> errors)
    {
        if (errors.Count == 0)
        {
            return "Configuration parsing failed";
        }

        if (errors.Count == 1)
        {
            return $"Configuration parsing failed with 1 error:\n{errors[0]}";
        }

        var message = $"Configuration parsing failed with {errors.Count} errors:\n\n";
        for (var i = 0; i < errors.Count; i++)
        {
            message += $"Error {i + 1}:\n{errors[i]}\n\n";
        }

        return message;
    }
}
