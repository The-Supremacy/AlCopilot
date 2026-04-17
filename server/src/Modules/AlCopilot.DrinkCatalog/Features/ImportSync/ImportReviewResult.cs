namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed record ImportReviewResult(
    ImportReviewSummary Summary,
    List<ImportReviewRow> Rows);
