using AlCopilot.DrinkCatalog.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Features.Audit;

internal sealed class AuditLogEntryRepository(DrinkCatalogDbContext dbContext) : IAuditLogEntryRepository
{
    public void Add(AuditLogEntry entry) => dbContext.AuditLogEntries.Add(entry);

    public async Task<List<AuditLogEntry>> GetRecentAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogEntries
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .ThenByDescending(entry => entry.Id)
            .Take(200)
            .ToListAsync(cancellationToken);
    }
}
