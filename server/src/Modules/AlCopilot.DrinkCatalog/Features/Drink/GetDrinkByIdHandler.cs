using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class GetDrinkByIdHandler(IDrinkQueryService drinkQueryService)
    : IRequestHandler<GetDrinkByIdQuery, DrinkDetailDto?>
{
    public async ValueTask<DrinkDetailDto?> Handle(
        GetDrinkByIdQuery request, CancellationToken cancellationToken)
    {
        return await drinkQueryService.GetDetailByIdAsync(request.DrinkId, cancellationToken);
    }
}
