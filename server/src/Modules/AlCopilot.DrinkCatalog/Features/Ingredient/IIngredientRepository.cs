using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Shared.Data;

namespace AlCopilot.DrinkCatalog.Features.Ingredient;

public interface IIngredientRepository : IRepository<Ingredient, Guid>
{
    Task<List<IngredientDto>> GetAllAsync(Guid? categoryId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}
