using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Features.Drink.Abstractions;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class FindFuzzyDrinkMatchesHandler(IDrinkQueryService drinkQueryService)
    : IRequestHandler<FindFuzzyDrinkMatchesQuery, List<FuzzyDrinkMatchDto>>
{
    public async ValueTask<List<FuzzyDrinkMatchDto>> Handle(
        FindFuzzyDrinkMatchesQuery request,
        CancellationToken cancellationToken)
    {
        return await drinkQueryService.FindFuzzyDrinkMatchesAsync(
            request.SearchText,
            request.Limit,
            cancellationToken);
    }
}
