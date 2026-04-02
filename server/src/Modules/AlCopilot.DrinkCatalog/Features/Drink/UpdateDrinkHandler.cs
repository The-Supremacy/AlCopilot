using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class UpdateDrinkHandler(
    IDrinkRepository drinkRepository,
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateDrinkCommand, bool>
{
    public async ValueTask<bool> Handle(UpdateDrinkCommand request, CancellationToken cancellationToken)
    {
        var drink = await drinkRepository.GetByIdAsync(request.DrinkId, cancellationToken);
        if (drink is null) return false;

        var name = DrinkName.Create(request.Name);

        if (await drinkRepository.ExistsByNameAsync(name, request.DrinkId, cancellationToken))
            throw new InvalidOperationException($"A drink with the name '{name.Value}' already exists.");

        drink.Update(name, request.Description, ImageUrl.Create(request.ImageUrl));

        if (request.TagIds is { Count: > 0 })
        {
            var tags = await tagRepository.GetByIdsAsync(request.TagIds, cancellationToken);
            var foundIds = tags.Select(t => t.Id).ToHashSet();
            var missingId = request.TagIds.FirstOrDefault(id => !foundIds.Contains(id));
            if (missingId != default)
                throw new InvalidOperationException($"Tag '{missingId}' not found.");
            drink.SetTags(tags);
        }
        else
        {
            drink.SetTags([]);
        }

        var entries = (request.RecipeEntries ?? []).Select(re =>
            RecipeEntry.Create(drink.Id, re.IngredientId, Quantity.Create(re.Quantity), re.RecommendedBrand));
        drink.SetRecipeEntries(entries);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
