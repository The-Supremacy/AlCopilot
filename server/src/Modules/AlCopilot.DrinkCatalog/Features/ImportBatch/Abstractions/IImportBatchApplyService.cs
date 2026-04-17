namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public interface IImportBatchApplyService
{
    Task<ImportApplySummary> ApplyAsync(
        ImportBatch batch,
        CancellationToken cancellationToken);
}
