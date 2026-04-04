using AlCopilot.Shared.Domain;

namespace AlCopilot.Shared.Data;

public interface IRepository<TRoot, in TId>
    where TRoot : AggregateRoot<TId>
    where TId : notnull
{
    Task<TRoot?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    void Add(TRoot aggregate);
    void Remove(TRoot aggregate);
}
