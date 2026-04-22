using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class DeterministicRecommendationCandidateBuilder : IRecommendationCandidateBuilder
{
    public List<RecommendationGroupDto> Build(
        string customerRequest,
        RecommendationRequestIntent intent,
        CustomerProfileDto profile,
        IReadOnlyCollection<DrinkDetailDto> drinks)
    {
        var favorites = profile.FavoriteIngredientIds.ToHashSet();
        var prohibited = profile.ProhibitedIngredientIds.ToHashSet();
        var disliked = profile.DislikedIngredientIds.ToHashSet();
        var owned = profile.OwnedIngredientIds.ToHashSet();
        var scopedDrinks = ScopeDrinks(intent, drinks);
        var ranked = scopedDrinks
            .Where(drink => !drink.RecipeEntries.Any(entry => prohibited.Contains(entry.Ingredient.Id)))
            .Select(drink => Score(drink, intent, favorites, disliked, owned))
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Drink.Name, StringComparer.Ordinal)
            .Take(8)
            .ToList();

        return
        [
            new RecommendationGroupDto(
                "make-now",
                "Available Now",
                ranked.Where(item => item.MissingIngredientNames.Count == 0).Take(4).Select(item => ToDto(item, owned)).ToList()),
            new RecommendationGroupDto(
                "buy-next",
                "Consider for Restock",
                ranked.Where(item => item.MissingIngredientNames.Count > 0).Take(4).Select(item => ToDto(item, owned)).ToList()),
        ];
    }

    private static IReadOnlyCollection<DrinkDetailDto> ScopeDrinks(
        RecommendationRequestIntent intent,
        IReadOnlyCollection<DrinkDetailDto> drinks)
    {
        if (intent.IsRecipeLookup)
        {
            var requestedDrinkName = intent.RequestedDrinkName!;
            var matches = RecommendationCatalogMatcher.SearchDrinksByName(drinks, requestedDrinkName, 8);
            if (matches.Count > 0)
            {
                return matches;
            }
        }

        if (intent.IsIngredientLed)
        {
            var requestedIngredientName = intent.RequestedIngredientName!;
            var matches = RecommendationCatalogMatcher.FindDrinksByIngredient(drinks, requestedIngredientName, 8);
            if (matches.Count > 0)
            {
                return matches;
            }
        }

        return drinks;
    }

    private static CandidateScore Score(
        DrinkDetailDto drink,
        RecommendationRequestIntent intent,
        HashSet<Guid> favorites,
        HashSet<Guid> disliked,
        HashSet<Guid> owned)
    {
        var totalRecipeIngredientCount = drink.RecipeEntries
            .Select(entry => entry.Ingredient.Id)
            .Distinct()
            .Count();
        var ownedIngredientCount = drink.RecipeEntries
            .Select(entry => entry.Ingredient.Id)
            .Distinct()
            .Count(owned.Contains);
        var missingIngredientNames = drink.RecipeEntries
            .Where(entry => !owned.Contains(entry.Ingredient.Id))
            .Select(entry => entry.Ingredient.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var dislikedCount = drink.RecipeEntries.Count(entry => disliked.Contains(entry.Ingredient.Id));
        var favoriteCount = drink.RecipeEntries.Count(entry => favorites.Contains(entry.Ingredient.Id));
        var matchedSignals = FindMatchedSignals(drink, intent);

        var score = missingIngredientNames.Count == 0 ? 100 : 70 - missingIngredientNames.Count * 5;
        score += Math.Min(favoriteCount, 2) * 6;
        score -= dislikedCount * 8;
        score += matchedSignals.Count * 8;
        score += ownedIngredientCount * 3;

        if (totalRecipeIngredientCount > 0)
        {
            var coverageRatio = (double)ownedIngredientCount / totalRecipeIngredientCount;
            score += (int)Math.Round(coverageRatio * 12, MidpointRounding.AwayFromZero);
        }

        if (intent.IsIngredientLed && RecommendationCatalogMatcher.DrinkContainsIngredient(drink, intent.RequestedIngredientName!))
        {
            score += 24;
        }

        if (intent.IsRecipeLookup && drink.Name.Contains(intent.RequestedDrinkName!, StringComparison.OrdinalIgnoreCase))
        {
            score += 40;
        }

        return new CandidateScore(
            drink,
            missingIngredientNames,
            matchedSignals,
            score);
    }

    private static List<string> FindMatchedSignals(DrinkDetailDto drink, RecommendationRequestIntent intent)
    {
        var searchCorpus = string.Join(
            " | ",
            new[]
            {
                drink.Name,
                drink.Category,
                drink.Description,
                drink.Method,
                drink.Garnish,
                string.Join(", ", drink.Tags.Select(tag => tag.Name)),
                string.Join(", ", drink.RecipeEntries.Select(entry => entry.Ingredient.Name)),
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

        var matchedSignals = intent.PreferenceSignals
            .Where(signal => searchCorpus.Contains(signal, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(signal => signal, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (intent.IsIngredientLed)
        {
            matchedSignals.Add(intent.RequestedIngredientName!);
        }

        if (intent.IsRecipeLookup)
        {
            matchedSignals.Add(intent.RequestedDrinkName!);
        }

        return matchedSignals
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(signal => signal, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static RecommendationItemDto ToDto(CandidateScore result, HashSet<Guid> owned)
    {
        return new RecommendationItemDto(
            result.Drink.Id,
            result.Drink.Name,
            result.Drink.Description,
            result.MissingIngredientNames,
            result.MatchedSignals,
            result.Score,
            result.Drink.RecipeEntries
                .Select(entry => new RecommendationRecipeEntryDto(
                    entry.Ingredient.Name,
                    entry.Quantity,
                    owned.Contains(entry.Ingredient.Id)))
                .ToList());
    }

    private sealed record CandidateScore(
        DrinkDetailDto Drink,
        List<string> MissingIngredientNames,
        List<string> MatchedSignals,
        int Score);
}
