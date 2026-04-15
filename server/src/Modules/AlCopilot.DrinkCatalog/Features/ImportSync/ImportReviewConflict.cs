namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed record ImportReviewConflict(
    string TargetType,
    string TargetKey,
    string Action,
    string Summary);
