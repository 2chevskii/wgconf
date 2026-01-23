namespace WgConf;

public class WireguardConfigurationException(IReadOnlyList<ParseError> errors)
    : Exception(BuildMessage(errors))
{
    public IReadOnlyList<ParseError> Errors { get; } = errors;

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
