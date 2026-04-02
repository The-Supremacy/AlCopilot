using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Handlers.Queries;

public sealed class GetDrinkByIdHandler(IDrinkRepository drinkRepository)
    : IRequestHandler<GetDrinkByIdQuery, DrinkDetailDto?>
{
    public async ValueTask<DrinkDetailDto?> Handle(
        GetDrinkByIdQuery request, CancellationToken cancellationToken)
    {
        return await drinkRepository.GetDetailByIdAsync(request.DrinkId, cancellationToken);
    }
}
