using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.DrinkCatalog.Features.Drink;

internal static class DrinkQueryMappings
{
    public static DrinkDto ToDto(
        this DrinkListReadModel drink,
        IReadOnlyDictionary<Guid, int> tagCounts)
    {
        return new DrinkDto(
            drink.Id,
            drink.Name,
            drink.Category,
            drink.Description,
            drink.Method,
            drink.Garnish,
            drink.ImageUrl,
            drink.Tags.Select(tag => tag.ToDto(tagCounts)).ToList());
    }

    public static DrinkDetailDto ToDto(
        this DrinkDetailReadModel drink,
        IReadOnlyDictionary<Guid, int> tagCounts,
        IReadOnlyDictionary<Guid, IngredientReadModel> ingredients)
    {
        return new DrinkDetailDto(
            drink.Id,
            drink.Name,
            drink.Category,
            drink.Description,
            drink.Method,
            drink.Garnish,
            drink.ImageUrl,
            drink.Tags.Select(tag => tag.ToDto(tagCounts)).ToList(),
            drink.RecipeEntries.Select(entry => entry.ToDto(ingredients)).ToList());
    }

    public static TagDto ToDto(
        this TagReadModel tag,
        IReadOnlyDictionary<Guid, int> tagCounts)
    {
        return new TagDto(
            tag.Id,
            tag.Name,
            tagCounts.TryGetValue(tag.Id, out var drinkCount) ? drinkCount : 0);
    }

    public static IngredientDto ToDto(this IngredientReadModel ingredient)
    {
        return new IngredientDto(
            ingredient.Id,
            ingredient.Name,
            ingredient.NotableBrands);
    }

    public static RecipeEntryDto ToDto(
        this RecipeEntryReadModel recipeEntry,
        IReadOnlyDictionary<Guid, IngredientReadModel> ingredients)
    {
        return new RecipeEntryDto(
            ingredients[recipeEntry.IngredientId].ToDto(),
            recipeEntry.Quantity,
            recipeEntry.RecommendedBrand);
    }
}
