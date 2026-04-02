using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Domain.Aggregates;
using AlCopilot.DrinkCatalog.Domain.ValueObjects;
using AlCopilot.Shared.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Handlers.Commands;

public sealed class CreateDrinkHandler(
    IDrinkRepository drinkRepository,
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateDrinkCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateDrinkCommand request, CancellationToken cancellationToken)
    {
        var name = DrinkName.Create(request.Name);

        if (await drinkRepository.ExistsByNameAsync(name, cancellationToken: cancellationToken))
            throw new InvalidOperationException($"A drink with the name '{name.Value}' already exists.");

        var drink = Drink.Create(name, request.Description, ImageUrl.Create(request.ImageUrl));

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

        if (request.RecipeEntries is { Count: > 0 })
        {
            var entries = request.RecipeEntries.Select(re =>
                RecipeEntry.Create(drink.Id, re.IngredientId, Quantity.Create(re.Quantity), re.RecommendedBrand));
            drink.SetRecipeEntries(entries);
        }

        drinkRepository.Add(drink);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return drink.Id;
    }
}
