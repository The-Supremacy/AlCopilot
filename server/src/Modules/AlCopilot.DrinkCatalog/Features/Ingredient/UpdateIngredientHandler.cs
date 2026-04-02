using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.Shared.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Handlers.Commands;

public sealed class UpdateIngredientHandler(
    IIngredientRepository ingredientRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateIngredientCommand, bool>
{
    public async ValueTask<bool> Handle(UpdateIngredientCommand request, CancellationToken cancellationToken)
    {
        var ingredient = await ingredientRepository.GetByIdAsync(request.IngredientId, cancellationToken);
        if (ingredient is null) return false;

        ingredient.UpdateBrands(request.NotableBrands);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
