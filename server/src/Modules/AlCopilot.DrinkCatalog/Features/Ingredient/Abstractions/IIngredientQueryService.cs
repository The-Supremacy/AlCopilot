using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.DrinkCatalog.Features.Ingredient.Abstractions;

public interface IIngredientQueryService
{
    Task<List<IngredientDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
