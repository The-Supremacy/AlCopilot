using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.IngredientCategory;
using AlCopilot.Shared.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Ingredient;

public sealed class CreateIngredientHandler(
    IIngredientRepository ingredientRepository,
    IIngredientCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateIngredientCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateIngredientCommand request, CancellationToken cancellationToken)
    {
        var name = IngredientName.Create(request.Name);

        if (await ingredientRepository.ExistsByNameAsync(name, cancellationToken))
            throw new InvalidOperationException($"An ingredient with the name '{name.Value}' already exists.");

        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new InvalidOperationException($"Ingredient category '{request.CategoryId}' not found.");

        var ingredient = Ingredient.Create(name, category.Id, request.NotableBrands);
        ingredientRepository.Add(ingredient);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ingredient.Id;
    }
}
