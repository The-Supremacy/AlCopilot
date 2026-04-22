using AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;
using AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies.Abstractions;

namespace AlCopilot.DrinkCatalog.Features.ImportBatch.Abstractions;

public interface IImportBatchProcessingService
{
    Task<ImportBatchProcessingResult> ProcessAsync(
        NormalizedCatalogImport import,
        CancellationToken cancellationToken);

    ImportBatchApplyReadiness GetBatchApplyReadiness(ImportBatch batch);
}
