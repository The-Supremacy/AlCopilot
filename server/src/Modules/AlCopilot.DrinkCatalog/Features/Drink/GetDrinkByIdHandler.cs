using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class GetDrinkByIdHandler(IDrinkRepository drinkRepository)
    : IRequestHandler<GetDrinkByIdQuery, DrinkDetailDto?>
{
    public async ValueTask<DrinkDetailDto?> Handle(
        GetDrinkByIdQuery request, CancellationToken cancellationToken)
    {
        return await drinkRepository.GetDetailByIdAsync(request.DrinkId, cancellationToken);
    }
}
