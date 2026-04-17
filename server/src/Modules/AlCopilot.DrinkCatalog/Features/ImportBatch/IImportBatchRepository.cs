using AlCopilot.Shared.Data;

namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public interface IImportBatchRepository : IRepository<ImportBatch, Guid>
{
    Task<List<ImportBatch>> GetHistoryAsync(CancellationToken cancellationToken = default);
    void Update(ImportBatch aggregate);
}
