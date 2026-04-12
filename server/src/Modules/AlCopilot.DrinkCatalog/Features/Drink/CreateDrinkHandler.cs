using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class CreateDrinkHandler(
    IDrinkRepository drinkRepository,
    ITagRepository tagRepository,
    AuditLogWriter auditLogWriter,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateDrinkCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateDrinkCommand request, CancellationToken cancellationToken)
    {
        var name = DrinkName.Create(request.Name);

        if (await drinkRepository.ExistsByNameAsync(name, cancellationToken: cancellationToken))
            throw new ConflictException($"A drink with the name '{name.Value}' already exists.");

        var drink = Drink.Create(
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

        if (request.RecipeEntries is { Count: > 0 })
        {
            var entries = request.RecipeEntries.Select(re =>
                RecipeEntry.Create(drink.Id, re.IngredientId, Quantity.Create(re.Quantity), re.RecommendedBrand));
            drink.SetRecipeEntries(entries);
        }

        drinkRepository.Add(drink);
        auditLogWriter.Write("drink.create", "drink", drink.Id.ToString(), $"Created drink '{drink.Name.Value}'.");
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return drink.Id;
    }
}
