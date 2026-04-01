using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Handlers.Queries;

public sealed class GetDrinksHandler(IDrinkRepository drinkRepository)
    : IRequestHandler<GetDrinksQuery, PagedResult<DrinkDto>>
{
    public async ValueTask<PagedResult<DrinkDto>> Handle(
        GetDrinksQuery request, CancellationToken cancellationToken)
    {
        return await drinkRepository.GetPagedAsync(
            request.TagIds, request.Page, request.PageSize, cancellationToken);
    }
}
