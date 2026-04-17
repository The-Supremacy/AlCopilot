namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public interface IImportBatchApplyService
{
    Task<ImportApplySummary> ApplyAsync(
        ImportBatch batch,
        CancellationToken cancellationToken);
}
