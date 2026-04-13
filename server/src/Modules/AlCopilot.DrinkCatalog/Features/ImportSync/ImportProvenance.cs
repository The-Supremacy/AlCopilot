namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed record ImportProvenance(
    string? SourceReference,
    string? DisplayName,
    string? ContentType,
    Dictionary<string, string?> Metadata,
    string? InitiatedByUserId = null,
    string? InitiatedByDisplayName = null)
{
    public static ImportProvenance Empty { get; } = new(null, null, null, []);
}
