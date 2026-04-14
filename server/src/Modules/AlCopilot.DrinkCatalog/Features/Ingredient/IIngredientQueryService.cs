using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.DrinkCatalog.Features.Ingredient;

public interface IIngredientQueryService
{
    Task<List<IngredientDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
