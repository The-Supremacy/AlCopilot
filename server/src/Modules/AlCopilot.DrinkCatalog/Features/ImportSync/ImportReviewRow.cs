namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed record ImportReviewRow(
    string TargetType,
    string TargetKey,
    string Action,
    string ChangeSummary,
    bool HasConflict,
    bool HasError);
