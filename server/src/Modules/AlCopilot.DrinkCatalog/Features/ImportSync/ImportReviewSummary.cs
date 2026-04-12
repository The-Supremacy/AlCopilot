namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed record ImportReviewSummary(
    int CreateCount,
    int UpdateCount,
    int SkipCount);
