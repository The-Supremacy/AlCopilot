namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed record ImportApplySummary(
    int CreatedCount,
    int UpdatedCount,
    int SkippedCount,
    int RejectedCount);
