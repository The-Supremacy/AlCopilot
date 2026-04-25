using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Shared.Text;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal static class RecommendationCatalogMatcher
{
    // Keep this matcher cheap and lexical. Typo recovery happens in fuzzy lookup, and broader
    // relevance recovery happens in semantic search.
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

        var normalizedName = drinkName.TrimOrEmpty();
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
        var normalizedQuery = query.TrimOrEmpty();

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

    internal static IReadOnlyCollection<DrinkDetailDto> FindDrinksByIngredients(
        IReadOnlyCollection<DrinkDetailDto> drinks,
        IReadOnlyCollection<string> ingredientNames,
        int maxResults)
    {
        if (ingredientNames.Count == 0)
        {
            return [];
        }

        var normalizedIngredientNames = ingredientNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (normalizedIngredientNames.Count == 0)
        {
            return [];
        }

        var cappedMaxResults = Math.Clamp(maxResults, 1, 10);
        var allMatches = drinks
            .Where(drink => normalizedIngredientNames.All(requestedIngredientName => DrinkContainsIngredient(drink, requestedIngredientName)))
            .OrderBy(drink => drink.Name, StringComparer.OrdinalIgnoreCase)
            .Take(cappedMaxResults)
            .ToList();
        if (allMatches.Count > 0)
        {
            return allMatches;
        }

        return drinks
            .Where(drink => normalizedIngredientNames.Any(requestedIngredientName => DrinkContainsIngredient(drink, requestedIngredientName)))
            .OrderByDescending(drink => normalizedIngredientNames.Count(requestedIngredientName => DrinkContainsIngredient(drink, requestedIngredientName)))
            .ThenBy(drink => drink.Name, StringComparer.OrdinalIgnoreCase)
            .Take(cappedMaxResults)
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

    internal static IReadOnlyCollection<string> GetMatchedIngredientNames(
        DrinkDetailDto drink,
        IReadOnlyCollection<string> ingredientNames)
    {
        return ingredientNames
            .SelectMany(ingredientName => GetMatchedIngredientNames(drink, ingredientName))
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

    internal static IReadOnlyCollection<string> FindMentionedIngredientNames(
        IReadOnlyCollection<DrinkDetailDto> drinks,
        string customerMessage)
    {
        return drinks
            .SelectMany(drink => drink.RecipeEntries)
            .Select(entry => entry.Ingredient.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(name => name.Length)
            .Where(name => customerMessage.Contains(name, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    internal static bool IngredientMatches(string ingredientName, string requestedIngredientName)
    {
        return ingredientName.Contains(requestedIngredientName, StringComparison.OrdinalIgnoreCase)
            || requestedIngredientName.Contains(ingredientName, StringComparison.OrdinalIgnoreCase);
    }

    internal static string? ExtractDrinkSearchText(string customerMessage, bool looksLikeRecipeLookup)
    {
        if (!looksLikeRecipeLookup)
        {
            return null;
        }

        var normalized = customerMessage
            .Replace("?", string.Empty, StringComparison.Ordinal)
            .Replace(".", string.Empty, StringComparison.Ordinal)
            .Replace("!", string.Empty, StringComparison.Ordinal)
            .TrimOrEmpty();
        var lowered = normalized.ToLowerInvariant();
        var prefixes = new[]
        {
            "how do i make",
            "how to make",
            "what's in",
            "what is in",
            "ingredients for",
            "method for",
            "instructions for",
            "recipe for",
        };

        foreach (var prefix in prefixes)
        {
            if (lowered.StartsWith(prefix, StringComparison.Ordinal))
            {
                var candidate = normalized[prefix.Length..].TrimOrEmpty();
                return TrimLeadingArticle(candidate);
            }
        }

        return TrimLeadingArticle(normalized);
    }

    internal static IReadOnlyCollection<string> ExtractIngredientSearchTexts(string customerMessage)
    {
        var normalized = customerMessage
            .Replace("?", string.Empty, StringComparison.Ordinal)
            .Replace(".", string.Empty, StringComparison.Ordinal)
            .Replace("!", string.Empty, StringComparison.Ordinal)
            .TrimOrEmpty();
        var lowered = normalized.ToLowerInvariant();
        var markers = new[]
        {
            " with ",
            " using ",
            " contains ",
            " containing ",
            " made with ",
        };

        foreach (var marker in markers)
        {
            var index = lowered.IndexOf(marker, StringComparison.Ordinal);
            if (index >= 0)
            {
                return SplitIngredientCandidates(normalized[(index + marker.Length)..].TrimOrEmpty());
            }
        }

        return [];
    }

    private static string? TrimLeadingArticle(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.TrimOrEmpty();
        foreach (var prefix in new[] { "a ", "an ", "the " })
        {
            if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed[prefix.Length..].TrimOrEmpty();
                break;
            }
        }

        return trimmed.NullIfWhiteSpace();
    }

    private static IReadOnlyCollection<string> SplitIngredientCandidates(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Replace(" and ", ",", StringComparison.OrdinalIgnoreCase)
            .Replace("&", ",", StringComparison.Ordinal)
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(TrimLeadingArticle)
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToList();
    }
}
