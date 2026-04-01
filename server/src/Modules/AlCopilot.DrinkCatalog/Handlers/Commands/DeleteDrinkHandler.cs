using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.Shared.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Handlers.Commands;

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
