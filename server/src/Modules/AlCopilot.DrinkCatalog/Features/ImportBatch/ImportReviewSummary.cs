namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public sealed record ImportReviewSummary(
    int CreateCount,
    int UpdateCount,
    int SkipCount);
