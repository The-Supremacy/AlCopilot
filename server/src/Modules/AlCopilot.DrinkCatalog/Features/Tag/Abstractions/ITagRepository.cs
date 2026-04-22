using AlCopilot.Shared.Data;

namespace AlCopilot.DrinkCatalog.Features.Tag.Abstractions;

public interface ITagRepository : IRepository<Tag, Guid>
{
    Task<List<Tag>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default);
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsReferencedByDrinksAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeTagId = null, CancellationToken cancellationToken = default);
}
