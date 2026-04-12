using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Audit;

public sealed class GetAuditLogEntriesHandler(IAuditLogEntryRepository repository)
    : IRequestHandler<GetAuditLogEntriesQuery, List<AuditLogEntryDto>>
{
    public async ValueTask<List<AuditLogEntryDto>> Handle(
        GetAuditLogEntriesQuery request,
        CancellationToken cancellationToken)
    {
        return (await repository.GetRecentAsync(cancellationToken))
            .Select(entry => entry.ToDto())
            .ToList();
    }
}
