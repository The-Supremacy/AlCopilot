using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Features.ImportSync;

internal sealed class ImportBatchRepository(DrinkCatalogDbContext dbContext) : IImportBatchRepository
{
    public async Task<ImportBatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ImportBatches.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public void Add(ImportBatch aggregate) => dbContext.ImportBatches.Add(aggregate);

    public void Remove(ImportBatch aggregate) => dbContext.ImportBatches.Remove(aggregate);

    public async Task<ImportBatch?> GetAppliedByStrategyAndFingerprintAsync(
        ImportStrategyKey strategyKey,
        string sourceFingerprint,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ImportBatches.FirstOrDefaultAsync(
            b => b.StrategyKey == strategyKey.ToWireValue()
                && b.SourceFingerprint == sourceFingerprint
                && b.Status == ImportBatchStatus.Completed,
            cancellationToken);
    }

    public async Task<List<ImportBatch>> GetHistoryAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ImportBatches
            .OrderByDescending(b => b.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
