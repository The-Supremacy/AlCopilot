using AlCopilot.Shared.Domain;

namespace AlCopilot.Shared.Data;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
