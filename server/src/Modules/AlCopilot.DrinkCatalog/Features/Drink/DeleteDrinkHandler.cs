using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.Shared.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class DeleteDrinkHandler(
    IDrinkRepository drinkRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteDrinkCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteDrinkCommand request, CancellationToken cancellationToken)
    {
        var drink = await drinkRepository.GetByIdAsync(request.DrinkId, cancellationToken);
        if (drink is null) return false;

        drink.SoftDelete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
