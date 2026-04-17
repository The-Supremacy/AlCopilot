using AlCopilot.DrinkCatalog.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

internal sealed class ImportBatchRepository(DrinkCatalogDbContext dbContext) : IImportBatchRepository
{
    public async Task<ImportBatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ImportBatches.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public void Add(ImportBatch aggregate) => dbContext.ImportBatches.Add(aggregate);

    public void Remove(ImportBatch aggregate) => dbContext.ImportBatches.Remove(aggregate);

    public void Update(ImportBatch aggregate) => dbContext.ImportBatches.Update(aggregate);

    public async Task<List<ImportBatch>> GetHistoryAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ImportBatches
            .OrderByDescending(b => b.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
