using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class GetRecommendationCatalogHandler(IDrinkQueryService drinkQueryService)
    : IRequestHandler<GetRecommendationCatalogQuery, List<DrinkDetailDto>>
{
    public async ValueTask<List<DrinkDetailDto>> Handle(
        GetRecommendationCatalogQuery request,
        CancellationToken cancellationToken)
    {
        return await drinkQueryService.GetAllAsync(cancellationToken);
    }
}
