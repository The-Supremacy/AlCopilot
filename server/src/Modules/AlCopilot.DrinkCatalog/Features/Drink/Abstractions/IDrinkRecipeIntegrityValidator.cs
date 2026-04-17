using AlCopilot.DrinkCatalog.Contracts.Commands;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public interface IDrinkRecipeIntegrityValidator
{
    Task ValidateAsync(
        IReadOnlyCollection<RecipeEntryInput> recipeEntries,
        CancellationToken cancellationToken = default);
}
