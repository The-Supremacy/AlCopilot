using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.Shared.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.IngredientCategory;

public sealed class CreateIngredientCategoryHandler(
    IIngredientCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateIngredientCategoryCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateIngredientCategoryCommand request, CancellationToken cancellationToken)
    {
        var name = CategoryName.Create(request.Name);

        if (await categoryRepository.ExistsByNameAsync(name, cancellationToken))
            throw new InvalidOperationException($"An ingredient category with the name '{name.Value}' already exists.");

        var category = IngredientCategory.Create(name);
        categoryRepository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
