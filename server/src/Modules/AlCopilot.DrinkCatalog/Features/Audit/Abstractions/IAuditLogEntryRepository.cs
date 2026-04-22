namespace AlCopilot.DrinkCatalog.Features.Audit.Abstractions;

public interface IAuditLogEntryRepository
{
    void Add(AuditLogEntry entry);
}
