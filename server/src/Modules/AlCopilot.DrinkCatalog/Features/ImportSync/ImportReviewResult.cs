namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed record ImportReviewResult(
    ImportReviewSummary Summary,
    List<ImportReviewConflict> Conflicts,
    List<ImportReviewRow> Rows);
