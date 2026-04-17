namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public sealed record ImportReviewResult(
    ImportReviewSummary Summary,
    List<ImportReviewRow> Rows);
