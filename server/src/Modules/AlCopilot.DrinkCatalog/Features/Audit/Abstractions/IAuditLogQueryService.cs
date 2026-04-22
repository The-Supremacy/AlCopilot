using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.DrinkCatalog.Features.Audit.Abstractions;

public interface IAuditLogQueryService
{
    Task<List<AuditLogEntryDto>> GetRecentAsync(CancellationToken cancellationToken = default);
}
