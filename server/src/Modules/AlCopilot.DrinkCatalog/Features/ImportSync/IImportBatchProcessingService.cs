using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;

namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public interface IImportBatchProcessingService
{
    Task<ImportBatchProcessingResult> ProcessAsync(
        NormalizedCatalogImport import,
        CancellationToken cancellationToken);

    ImportBatchApplyReadiness GetBatchApplyReadiness(ImportBatch batch);
}
