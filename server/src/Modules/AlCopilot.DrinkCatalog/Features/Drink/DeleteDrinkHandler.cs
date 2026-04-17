using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.Shared.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class DeleteDrinkHandler(
    IDrinkRepository drinkRepository,
    IAuditLogWriter auditLogWriter,
    IDrinkCatalogUnitOfWork unitOfWork) : IRequestHandler<DeleteDrinkCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteDrinkCommand request, CancellationToken cancellationToken)
    {
        var drink = await drinkRepository.GetByIdAsync(request.DrinkId, cancellationToken);
        if (drink is null) return false;

        drink.SoftDelete();
        auditLogWriter.Write("drink.delete", "drink", drink.Id.ToString(), $"Deleted drink '{drink.Name.Value}'.");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
