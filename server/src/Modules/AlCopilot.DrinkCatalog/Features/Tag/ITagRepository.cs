using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Shared.Data;

namespace AlCopilot.DrinkCatalog.Features.Tag;

public interface ITagRepository : IRepository<Tag, Guid>
{
    Task<List<TagDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Tag>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default);
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsReferencedByDrinksAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeTagId = null, CancellationToken cancellationToken = default);
}
