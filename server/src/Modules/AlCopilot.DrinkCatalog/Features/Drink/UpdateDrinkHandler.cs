using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Domain.Aggregates;
using AlCopilot.DrinkCatalog.Domain.ValueObjects;
using AlCopilot.Shared.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Handlers.Commands;

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
            var tags = new List<Tag>();
            foreach (var tagId in request.TagIds)
            {
                var tag = await tagRepository.GetByIdAsync(tagId, cancellationToken)
                    ?? throw new InvalidOperationException($"Tag '{tagId}' not found.");
                tags.Add(tag);
            }
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
