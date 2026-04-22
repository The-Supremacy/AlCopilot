using AlCopilot.DrinkCatalog.Contracts.Commands;

namespace AlCopilot.DrinkCatalog.Features.Drink.Abstractions;

public interface IDrinkRecipeIntegrityValidator
{
    Task ValidateAsync(
        IReadOnlyCollection<RecipeEntryInput> recipeEntries,
        CancellationToken cancellationToken = default);
}
