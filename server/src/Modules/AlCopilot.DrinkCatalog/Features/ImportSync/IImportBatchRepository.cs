using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.Shared.Data;

namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public interface IImportBatchRepository : IRepository<ImportBatch, Guid>
{
    Task<ImportBatch?> GetAppliedByStrategyAndFingerprintAsync(
        ImportStrategyKey strategyKey,
        string sourceFingerprint,
        CancellationToken cancellationToken = default);
    Task<List<ImportBatch>> GetHistoryAsync(CancellationToken cancellationToken = default);
}
