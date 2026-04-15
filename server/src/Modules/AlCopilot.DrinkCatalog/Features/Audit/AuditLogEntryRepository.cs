using AlCopilot.DrinkCatalog.Data;

namespace AlCopilot.DrinkCatalog.Features.Audit;

internal sealed class AuditLogEntryRepository(DrinkCatalogDbContext dbContext) : IAuditLogEntryRepository
{
    public void Add(AuditLogEntry entry) => dbContext.AuditLogEntries.Add(entry);
}
