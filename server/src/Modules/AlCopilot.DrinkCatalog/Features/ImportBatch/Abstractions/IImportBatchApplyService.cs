namespace AlCopilot.DrinkCatalog.Features.ImportBatch.Abstractions;

public interface IImportBatchApplyService
{
    Task<ImportApplySummary> ApplyAsync(
        ImportBatch batch,
        CancellationToken cancellationToken);
}
