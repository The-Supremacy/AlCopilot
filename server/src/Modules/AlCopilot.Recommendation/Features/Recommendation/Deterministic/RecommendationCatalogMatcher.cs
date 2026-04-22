using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal static class RecommendationCatalogMatcher
{
    internal static DrinkDetailDto? FindDrink(
        IReadOnlyCollection<DrinkDetailDto> drinks,
        string? drinkId,
        string? drinkName)
    {
        if (Guid.TryParse(drinkId, out var parsedDrinkId))
        {
            var byId = drinks.FirstOrDefault(drink => drink.Id == parsedDrinkId);
            if (byId is not null)
            {
                return byId;
            }
        }

        if (string.IsNullOrWhiteSpace(drinkName))
        {
            return null;
        }

        var normalizedName = drinkName.Trim();
        var exactMatch = drinks.FirstOrDefault(drink =>
            string.Equals(drink.Name, normalizedName, StringComparison.OrdinalIgnoreCase));
        if (exactMatch is not null)
        {
            return exactMatch;
        }

        var partialMatches = drinks
            .Where(drink => drink.Name.Contains(normalizedName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(drink => drink.Name.Length)
            .ToList();

        return partialMatches.Count == 1 ? partialMatches[0] : null;
    }

    internal static IReadOnlyCollection<DrinkDetailDto> SearchDrinksByName(
        IReadOnlyCollection<DrinkDetailDto> drinks,
        string query,
        int maxResults)
    {
        var normalizedQuery = query.Trim();

        return drinks
            .Where(drink => drink.Name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(drink => string.Equals(drink.Name, normalizedQuery, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(drink => drink.Name.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            .ThenBy(drink => drink.Name, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Clamp(maxResults, 1, 10))
            .ToList();
    }

    internal static IReadOnlyCollection<DrinkDetailDto> FindDrinksByIngredient(
        IReadOnlyCollection<DrinkDetailDto> drinks,
        string ingredientName,
        int maxResults)
    {
        return drinks
            .Where(drink => DrinkContainsIngredient(drink, ingredientName))
            .OrderBy(drink => drink.Name, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Clamp(maxResults, 1, 10))
            .ToList();
    }

    internal static bool DrinkContainsIngredient(DrinkDetailDto drink, string ingredientName)
    {
        return drink.RecipeEntries.Any(entry => IngredientMatches(entry.Ingredient.Name, ingredientName));
    }

    internal static IReadOnlyCollection<string> GetMatchedIngredientNames(
        DrinkDetailDto drink,
        string ingredientName)
    {
        return drink.RecipeEntries
            .Select(entry => entry.Ingredient.Name)
            .Where(name => IngredientMatches(name, ingredientName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    internal static string? FindMentionedDrinkName(
        IReadOnlyCollection<DrinkDetailDto> drinks,
        string customerMessage)
    {
        return drinks
            .OrderByDescending(drink => drink.Name.Length)
            .Select(drink => drink.Name)
            .FirstOrDefault(name => customerMessage.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    internal static string? FindMentionedIngredientName(
        IReadOnlyCollection<DrinkDetailDto> drinks,
        string customerMessage)
    {
        return drinks
            .SelectMany(drink => drink.RecipeEntries)
            .Select(entry => entry.Ingredient.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(name => name.Length)
            .FirstOrDefault(name => customerMessage.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    internal static bool IngredientMatches(string ingredientName, string requestedIngredientName)
    {
        return ingredientName.Contains(requestedIngredientName, StringComparison.OrdinalIgnoreCase)
            || requestedIngredientName.Contains(ingredientName, StringComparison.OrdinalIgnoreCase);
    }
}
