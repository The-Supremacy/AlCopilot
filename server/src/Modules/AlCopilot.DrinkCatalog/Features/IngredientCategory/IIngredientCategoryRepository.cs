using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Shared.Data;

namespace AlCopilot.DrinkCatalog.Features.IngredientCategory;

public interface IIngredientCategoryRepository : IRepository<IngredientCategory, Guid>
{
    Task<List<IngredientCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}
