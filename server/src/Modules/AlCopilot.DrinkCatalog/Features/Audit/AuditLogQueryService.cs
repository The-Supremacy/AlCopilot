using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.DrinkCatalog.Features.Audit;

internal sealed class AuditLogQueryService(DrinkCatalogDbContext dbContext) : IAuditLogQueryService
{
    public async Task<List<AuditLogEntryDto>> GetRecentAsync(CancellationToken cancellationToken = default)
    {
        var entries = await dbContext.AuditLogEntries
            .AsNoTracking()
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .ThenByDescending(entry => entry.Id)
            .Take(200)
            .ToListAsync(cancellationToken);

        return entries.Select(entry => entry.ToDto()).ToList();
    }
}
