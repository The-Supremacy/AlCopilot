using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.DrinkCatalog.Features.Audit;

public interface IAuditLogQueryService
{
    Task<List<AuditLogEntryDto>> GetRecentAsync(CancellationToken cancellationToken = default);
}
