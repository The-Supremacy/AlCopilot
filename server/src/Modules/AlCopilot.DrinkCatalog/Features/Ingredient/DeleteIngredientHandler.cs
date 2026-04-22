using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Ingredient;

public sealed class DeleteIngredientHandler(
    IIngredientRepository ingredientRepository,
    IAuditLogWriter auditLogWriter,
    IDrinkCatalogUnitOfWork unitOfWork) : IRequestHandler<DeleteIngredientCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteIngredientCommand request, CancellationToken cancellationToken)
    {
        var ingredient = await ingredientRepository.GetByIdAsync(request.IngredientId, cancellationToken);
        if (ingredient is null)
            return false;

        if (await ingredientRepository.IsReferencedByActiveDrinksAsync(request.IngredientId, cancellationToken))
        {
            throw new ConflictException(
                $"Ingredient '{ingredient.Name.Value}' is referenced by active drinks and cannot be deleted.");
        }

        ingredientRepository.Remove(ingredient);
        auditLogWriter.Write(
            "ingredient.delete",
            "ingredient",
            ingredient.Id.ToString(),
            $"Deleted ingredient '{ingredient.Name.Value}'.");
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
