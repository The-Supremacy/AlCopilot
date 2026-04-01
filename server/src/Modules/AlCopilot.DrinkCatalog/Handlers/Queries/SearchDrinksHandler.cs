using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Handlers.Queries;

public sealed class SearchDrinksHandler(IDrinkRepository drinkRepository)
    : IRequestHandler<SearchDrinksQuery, PagedResult<DrinkDto>>
{
    public async ValueTask<PagedResult<DrinkDto>> Handle(
        SearchDrinksQuery request, CancellationToken cancellationToken)
    {
        return await drinkRepository.SearchAsync(
            request.Query, request.Page, request.PageSize, cancellationToken);
    }
}
