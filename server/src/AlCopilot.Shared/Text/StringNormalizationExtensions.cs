namespace AlCopilot.Shared.Text;

public static class StringNormalizationExtensions
{
    public static string TrimOrEmpty(this string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    public static string? NullIfWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static string NormalizeWhitespace(this string? value)
    {
        return string.Join(
            " ",
            (value ?? string.Empty)
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    public static string? NormalizeNullableWhitespace(this string? value)
    {
        var normalized = value.NormalizeWhitespace();
        return normalized.Length == 0 ? null : normalized;
    }
}
