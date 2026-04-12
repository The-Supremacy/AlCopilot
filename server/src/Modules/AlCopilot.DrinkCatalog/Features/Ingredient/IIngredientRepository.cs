using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Shared.Data;

namespace AlCopilot.DrinkCatalog.Features.Ingredient;

public interface IIngredientRepository : IRepository<Ingredient, Guid>
{
    Task<List<IngredientDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludingIngredientId = null, CancellationToken cancellationToken = default);
    Task<Ingredient?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsReferencedByActiveDrinksAsync(Guid ingredientId, CancellationToken cancellationToken = default);
}
