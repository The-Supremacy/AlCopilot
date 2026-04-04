using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class GetDrinksHandler(IDrinkRepository drinkRepository)
    : IRequestHandler<GetDrinksQuery, PagedResult<DrinkDto>>
{
    public async ValueTask<PagedResult<DrinkDto>> Handle(
        GetDrinksQuery request, CancellationToken cancellationToken)
    {
        return await drinkRepository.GetPagedAsync(request.Filter, cancellationToken);
    }
}
