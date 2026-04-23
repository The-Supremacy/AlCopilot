using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Shared.Models;

namespace AlCopilot.DrinkCatalog.Features.Drink.Abstractions;

public interface IDrinkQueryService
{
    Task<PagedResult<DrinkDto>> GetPagedAsync(DrinkFilter filter, CancellationToken cancellationToken = default);
    Task<List<DrinkDetailDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DrinkDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<FuzzyDrinkMatchDto>> FindFuzzyDrinkMatchesAsync(
        string searchText,
        int limit = 5,
        CancellationToken cancellationToken = default);
    Task<List<FuzzyIngredientMatchDto>> FindFuzzyIngredientMatchesAsync(
        string searchText,
        int limit = 5,
        CancellationToken cancellationToken = default);
}
