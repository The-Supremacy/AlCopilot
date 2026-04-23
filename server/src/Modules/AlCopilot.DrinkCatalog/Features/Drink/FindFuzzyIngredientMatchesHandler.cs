using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Features.Drink.Abstractions;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class FindFuzzyIngredientMatchesHandler(IDrinkQueryService drinkQueryService)
    : IRequestHandler<FindFuzzyIngredientMatchesQuery, List<FuzzyIngredientMatchDto>>
{
    public async ValueTask<List<FuzzyIngredientMatchDto>> Handle(
        FindFuzzyIngredientMatchesQuery request,
        CancellationToken cancellationToken)
    {
        return await drinkQueryService.FindFuzzyIngredientMatchesAsync(
            request.SearchText,
            request.Limit,
            cancellationToken);
    }
}
