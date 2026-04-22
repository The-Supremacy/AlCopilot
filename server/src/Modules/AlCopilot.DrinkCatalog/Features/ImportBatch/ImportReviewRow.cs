namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public sealed record ImportReviewRow(
    string TargetType,
    string TargetKey,
    string Action,
    string ChangeSummary,
    bool RequiresReview,
    bool HasError);
