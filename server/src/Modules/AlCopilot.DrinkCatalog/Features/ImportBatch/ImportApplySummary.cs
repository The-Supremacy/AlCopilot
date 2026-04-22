namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public sealed record ImportApplySummary(
    int CreatedCount,
    int UpdatedCount,
    int SkippedCount,
    int RejectedCount);
