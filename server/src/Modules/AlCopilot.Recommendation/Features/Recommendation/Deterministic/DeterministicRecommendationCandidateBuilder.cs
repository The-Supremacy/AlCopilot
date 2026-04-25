using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class DeterministicRecommendationCandidateBuilder : IRecommendationCandidateBuilder
{
    public List<RecommendationGroupDto> Build(
        string customerRequest,
        RecommendationRequestIntent intent,
        CustomerProfileDto profile,
        IReadOnlyCollection<DrinkDetailDto> drinks,
        RecommendationSemanticSearchResult semanticSearchResult)
    {
        var favorites = profile.FavoriteIngredientIds.ToHashSet();
        var prohibited = profile.ProhibitedIngredientIds.ToHashSet();
        var disliked = profile.DislikedIngredientIds.ToHashSet();
        var owned = profile.OwnedIngredientIds.ToHashSet();
        var scopedDrinks = ScopeDrinks(intent, drinks, semanticSearchResult);
        var ranked = scopedDrinks
            .Where(drink => intent.IsDrinkDetailsRequest || !drink.RecipeEntries.Any(entry => prohibited.Contains(entry.Ingredient.Id)))
            .Select(drink => Score(drink, intent, favorites, disliked, owned, semanticSearchResult))
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Drink.Name, StringComparer.Ordinal)
            .Take(8)
            .ToList();

        if (intent.IsDrinkDetailsRequest)
        {
            return
            [
                new RecommendationGroupDto(
                    "drink-details",
                    "Drink Details",
                    ranked.Take(3).Select(item => ToDto(item, owned)).ToList()),
            ];
        }

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
        IReadOnlyCollection<DrinkDetailDto> drinks,
        RecommendationSemanticSearchResult semanticSearchResult)
    {
        if (intent.IsDrinkDetailsRequest && intent.HasRequestedDrink)
        {
            var requestedDrinkName = intent.RequestedDrinkName!;
            var matches = RecommendationCatalogMatcher.SearchDrinksByName(drinks, requestedDrinkName, 8);
            if (matches.Count > 0)
            {
                return matches;
            }

            var semanticMatches = semanticSearchResult.ByDrinkId.Keys.ToHashSet();
            if (semanticMatches.Count > 0)
            {
                return drinks.Where(drink => semanticMatches.Contains(drink.Id)).Take(8).ToList();
            }
        }

        if (intent.HasRequestedIngredients)
        {
            var matches = RecommendationCatalogMatcher.FindDrinksByIngredients(drinks, intent.RequestedIngredientNames, 8);
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
        HashSet<Guid> owned,
        RecommendationSemanticSearchResult semanticSearchResult)
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
        var semanticSignal = semanticSearchResult.Find(drink.Id);
        var matchedLexicalSignals = FindMatchedLexicalSignals(drink, intent);
        var matchedSignals = BuildMatchedSignals(matchedLexicalSignals, semanticSignal);

        var score = missingIngredientNames.Count == 0 ? 100 : 70 - missingIngredientNames.Count * 5;
        score += Math.Min(favoriteCount, 2) * 6;
        score -= dislikedCount * 8;
        score += matchedLexicalSignals.Count * 8;
        score += ownedIngredientCount * 3;

        if (totalRecipeIngredientCount > 0)
        {
            var coverageRatio = (double)ownedIngredientCount / totalRecipeIngredientCount;
            score += (int)Math.Round(coverageRatio * 12, MidpointRounding.AwayFromZero);
        }

        if (intent.HasRequestedIngredients)
        {
            var matchedRequestedIngredientCount = intent.RequestedIngredientNames.Count(requestedIngredientName =>
                RecommendationCatalogMatcher.DrinkContainsIngredient(drink, requestedIngredientName));
            score += matchedRequestedIngredientCount * 24;
        }

        if (intent.IsDrinkDetailsRequest && intent.HasRequestedDrink && drink.Name.Contains(intent.RequestedDrinkName!, StringComparison.OrdinalIgnoreCase))
        {
            score += 40;
        }

        if (semanticSignal is not null)
        {
            score += (int)Math.Round(semanticSignal.WeightedScore * 10d, MidpointRounding.AwayFromZero);
        }

        return new CandidateScore(
            drink,
            missingIngredientNames,
            matchedSignals,
            score,
            semanticSignal?.SummaryHints ?? []);
    }

    private static List<string> FindMatchedLexicalSignals(
        DrinkDetailDto drink,
        RecommendationRequestIntent intent)
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

        var matchedSignals = intent.RequestDescriptors
            .Where(signal => searchCorpus.Contains(signal, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(signal => signal, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (intent.HasRequestedIngredients)
        {
            matchedSignals.AddRange(RecommendationCatalogMatcher.GetMatchedIngredientNames(drink, intent.RequestedIngredientNames));
        }

        if (intent.IsDrinkDetailsRequest && intent.HasRequestedDrink)
        {
            matchedSignals.Add(intent.RequestedDrinkName!);
        }

        return matchedSignals;
    }

    private static List<string> BuildMatchedSignals(
        IReadOnlyCollection<string> matchedLexicalSignals,
        RecommendationSemanticDrinkSignal? semanticSignal)
    {
        return matchedLexicalSignals
            .Concat(semanticSignal?.SummaryHints ?? [])
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
        int Score,
        IReadOnlyCollection<string> SemanticHints);
}
