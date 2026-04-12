using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class UpdateDrinkHandler(
    IDrinkRepository drinkRepository,
    ITagRepository tagRepository,
    AuditLogWriter auditLogWriter,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateDrinkCommand, bool>
{
    public async ValueTask<bool> Handle(UpdateDrinkCommand request, CancellationToken cancellationToken)
    {
        var drink = await drinkRepository.GetByIdAsync(request.DrinkId, cancellationToken);
        if (drink is null) return false;

        var name = DrinkName.Create(request.Name);

        if (await drinkRepository.ExistsByNameAsync(name, request.DrinkId, cancellationToken))
            throw new ConflictException($"A drink with the name '{name.Value}' already exists.");

        drink.Update(
            name,
            DrinkCategory.Create(request.Category),
            request.Description,
            request.Method,
            request.Garnish,
            ImageUrl.Create(request.ImageUrl));

        if (request.TagIds is { Count: > 0 })
        {
            var tags = await tagRepository.GetByIdsAsync(request.TagIds, cancellationToken);
            var foundIds = tags.Select(t => t.Id).ToHashSet();
            var missingId = request.TagIds.FirstOrDefault(id => !foundIds.Contains(id));
            if (missingId != default)
                throw new NotFoundException($"Tag '{missingId}' not found.");
            drink.SetTags(tags);
        }
        else
        {
            drink.SetTags([]);
        }

        var entries = (request.RecipeEntries ?? []).Select(re =>
            RecipeEntry.Create(drink.Id, re.IngredientId, Quantity.Create(re.Quantity), re.RecommendedBrand));
        drink.SetRecipeEntries(entries);

        auditLogWriter.Write("drink.update", "drink", drink.Id.ToString(), $"Updated drink '{drink.Name.Value}'.");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
