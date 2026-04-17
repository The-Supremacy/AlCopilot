namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed record ImportBatchProcessingResult(
    List<ImportDiagnostic> Diagnostics,
    ImportReviewSummary ReviewSummary,
    List<ImportReviewRow> ReviewRows)
{
    public ImportReviewResult ToReviewResult()
    {
        return new ImportReviewResult(ReviewSummary, ReviewRows);
    }
}
