namespace AlCopilot.DrinkCatalog.Features.Audit;

public interface IAuditLogEntryRepository
{
    void Add(AuditLogEntry entry);
}
