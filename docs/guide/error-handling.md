# Error Handling

Parsing failures return structured errors so you can present friendly feedback to users.

## ParseError

`ParseError` carries context about a single issue:

- `LineNumber` - Line number where the error was detected. May be `0` for validation errors that are not tied to a specific line.
- `Message` - Human readable message.
- `PropertyName` - The property name, when known.
- `Section` - The current section, when known.
- `LineText` - The original line text, when known.
- `SurroundingLines` - Up to two lines before and after for context.

`ToString()` formats a readable error message and includes surrounding lines if available.

## WireguardConfigurationException

`Read()` and `Parse()` throw `WireguardConfigurationException` when any errors are present. The exception contains the full error list in the `Errors` property.

## Example

```csharp
try
{
    var config = WireguardConfiguration.Parse(text);
}
catch (WireguardConfigurationException ex)
{
    foreach (var error in ex.Errors)
    {
        Console.WriteLine(error);
    }
}
```

## Non-throwing APIs

Use `TryParse`, `TryRead`, or `TryReadAsync` to avoid exceptions and capture errors explicitly.
