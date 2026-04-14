using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.Shared.Errors;

namespace AlCopilot.DrinkCatalog.Features.Drink;

public sealed class DrinkRecipeIntegrityValidator(IIngredientRepository ingredientRepository)
    : IDrinkRecipeIntegrityValidator
{
    public async Task ValidateAsync(
        IReadOnlyCollection<RecipeEntryInput> recipeEntries,
        CancellationToken cancellationToken = default)
    {
        if (recipeEntries.Count == 0)
        {
            return;
        }

        var ingredientIds = recipeEntries
            .Select(entry => entry.IngredientId)
            .Distinct()
            .ToArray();

        var existingIngredientIds = await ingredientRepository.GetExistingIdsAsync(ingredientIds, cancellationToken);
        if (existingIngredientIds.Count != ingredientIds.Length)
        {
            var missingIngredientId = ingredientIds.First(id => !existingIngredientIds.Contains(id));
            throw new NotFoundException($"Ingredient '{missingIngredientId}' not found.");
        }
    }
}
