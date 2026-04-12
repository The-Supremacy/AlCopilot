using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Models;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public interface IDrinkRepository : IRepository<Drink, Guid>
{
    Task<PagedResult<DrinkDto>> GetPagedAsync(DrinkFilter filter, CancellationToken cancellationToken = default);
    Task<List<DrinkDetailDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<Drink?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<DrinkDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
