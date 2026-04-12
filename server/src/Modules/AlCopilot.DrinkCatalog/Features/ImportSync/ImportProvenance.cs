namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed record ImportProvenance(
    string? SourceReference,
    string? DisplayName,
    string? ContentType,
    Dictionary<string, string?> Metadata)
{
    public static ImportProvenance Empty { get; } = new(null, null, null, []);
}
