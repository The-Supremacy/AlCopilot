using AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;

namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public interface IImportBatchProcessingService
{
    Task<ImportBatchProcessingResult> ProcessAsync(
        NormalizedCatalogImport import,
        CancellationToken cancellationToken);

    ImportBatchApplyReadiness GetBatchApplyReadiness(ImportBatch batch);
}
