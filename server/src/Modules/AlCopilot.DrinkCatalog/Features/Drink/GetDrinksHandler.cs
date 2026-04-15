using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class GetDrinksHandler(IDrinkQueryService drinkQueryService)
    : IRequestHandler<GetDrinksQuery, PagedResult<DrinkDto>>
{
    public async ValueTask<PagedResult<DrinkDto>> Handle(
        GetDrinksQuery request, CancellationToken cancellationToken)
    {
        return await drinkQueryService.GetPagedAsync(request.Filter, cancellationToken);
    }
}
