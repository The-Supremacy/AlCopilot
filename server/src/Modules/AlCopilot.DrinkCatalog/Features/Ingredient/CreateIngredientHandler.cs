using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Ingredient;

public sealed class CreateIngredientHandler(
    IIngredientRepository ingredientRepository,
    AuditLogWriter auditLogWriter,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateIngredientCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateIngredientCommand request, CancellationToken cancellationToken)
    {
        var name = IngredientName.Create(request.Name);

        if (await ingredientRepository.ExistsByNameAsync(name, cancellationToken: cancellationToken))
            throw new ConflictException($"An ingredient with the name '{name.Value}' already exists.");

        var ingredient = Ingredient.Create(name, request.NotableBrands);
        ingredientRepository.Add(ingredient);
        auditLogWriter.Write(
            "ingredient.create",
            "ingredient",
            ingredient.Id.ToString(),
            $"Created ingredient '{ingredient.Name.Value}'.");
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ingredient.Id;
    }
}
