using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationRunContextBuilder : IRecommendationRunContextBuilder
{
    public RecommendationRunContext Build(
        RecommendationRequestIntent intent,
        CustomerProfileDto profile,
        IReadOnlyCollection<DrinkDetailDto> drinks,
        IReadOnlyCollection<RecommendationGroupDto> groups,
        RecommendationSemanticSearchResult semanticSearchResult)
    {
        var ownedIngredientIds = profile.OwnedIngredientIds.ToHashSet();
        var dislikedIngredientIds = profile.DislikedIngredientIds.ToHashSet();
        var drinkLookup = drinks.ToDictionary(drink => drink.Id);
        var ingredientNames = drinks
            .SelectMany(drink => drink.RecipeEntries)
            .Select(entry => entry.Ingredient)
            .GroupBy(ingredient => ingredient.Id)
            .ToDictionary(
                group => group.Key,
                group => group.Select(ingredient => ingredient.Name)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .First());
        var runContextGroups = groups
            .Select(group => new RecommendationRunContextGroup(
                group.Key,
                group.Label,
                group.Items
                    .Select(item => BuildRunContextItem(item, drinkLookup, ownedIngredientIds, dislikedIngredientIds, semanticSearchResult))
                    .ToList()))
            .ToList();
        var semanticSummaryHints = semanticSearchResult.ByDrinkId.Values
            .OrderByDescending(signal => signal.WeightedScore)
            .Take(5)
            .SelectMany(signal => signal.SummaryHints)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new RecommendationRunContext(intent, profile, groups, ingredientNames, runContextGroups, semanticSummaryHints);
    }

    private static RecommendationRunContextItem BuildRunContextItem(
        RecommendationItemDto item,
        IReadOnlyDictionary<Guid, DrinkDetailDto> drinkLookup,
        HashSet<Guid> ownedIngredientIds,
        HashSet<Guid> dislikedIngredientIds,
        RecommendationSemanticSearchResult semanticSearchResult)
    {
        var semanticHints = semanticSearchResult.Find(item.DrinkId)?.SummaryHints ?? [];
        if (!drinkLookup.TryGetValue(item.DrinkId, out var drink))
        {
            return new RecommendationRunContextItem(
                item.DrinkId,
                item.DrinkName,
                item.Description,
                [],
                item.MissingIngredientNames,
                [],
                [],
                null,
                null,
                item.MatchedSignals,
                semanticHints,
                item.Score);
        }

        var ownedIngredientNames = drink.RecipeEntries
            .Where(entry => ownedIngredientIds.Contains(entry.Ingredient.Id))
            .Select(entry => entry.Ingredient.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var recipeIngredientNames = drink.RecipeEntries
            .Select(entry => entry.Ingredient.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var dislikedIngredientNames = drink.RecipeEntries
            .Where(entry => dislikedIngredientIds.Contains(entry.Ingredient.Id))
            .Select(entry => entry.Ingredient.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new RecommendationRunContextItem(
            drink.Id,
            drink.Name,
            string.IsNullOrWhiteSpace(item.Description) ? drink.Description : item.Description,
            ownedIngredientNames,
            item.MissingIngredientNames,
            dislikedIngredientNames,
            recipeIngredientNames,
            drink.Method,
            drink.Garnish,
            item.MatchedSignals,
            semanticHints,
            item.Score);
    }
}
