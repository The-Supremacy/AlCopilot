using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Ingredient;

public sealed class UpdateIngredientHandler(
    IIngredientRepository ingredientRepository,
    IAuditLogWriter auditLogWriter,
    IDrinkCatalogUnitOfWork unitOfWork) : IRequestHandler<UpdateIngredientCommand, bool>
{
    public async ValueTask<bool> Handle(UpdateIngredientCommand request, CancellationToken cancellationToken)
    {
        var ingredient = await ingredientRepository.GetByIdAsync(request.IngredientId, cancellationToken);
        if (ingredient is null) return false;

        var name = IngredientName.Create(request.Name);
        if (await ingredientRepository.ExistsByNameAsync(name, request.IngredientId, cancellationToken))
            throw new ConflictException($"Ingredient '{name.Value}' already exists.");

        ingredient.Update(name, request.NotableBrands);
        auditLogWriter.Write(
            "ingredient.update",
            "ingredient",
            ingredient.Id.ToString(),
            $"Updated ingredient '{ingredient.Name.Value}'.");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
