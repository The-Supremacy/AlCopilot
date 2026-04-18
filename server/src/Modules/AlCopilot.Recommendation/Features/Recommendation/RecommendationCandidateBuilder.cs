using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class DeterministicRecommendationCandidateBuilder : IRecommendationCandidateBuilder
{
    public List<RecommendationGroupDto> Build(
        string customerRequest,
        CustomerProfileDto profile,
        IReadOnlyCollection<DrinkDetailDto> drinks)
    {
        var favorites = profile.FavoriteIngredientIds.ToHashSet();
        var prohibited = profile.ProhibitedIngredientIds.ToHashSet();
        var disliked = profile.DislikedIngredientIds.ToHashSet();
        var owned = profile.OwnedIngredientIds.ToHashSet();

        var ranked = drinks
            .Where(drink => !drink.RecipeEntries.Any(entry => prohibited.Contains(entry.Ingredient.Id)))
            .Select(drink => Score(drink, favorites, disliked, owned))
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Drink.Name, StringComparer.Ordinal)
            .Take(8)
            .ToList();

        return
        [
            new RecommendationGroupDto(
                "make-now",
                "Make Now",
                ranked.Where(item => item.MissingIngredientNames.Count == 0).Take(4).Select(ToDto).ToList()),
            new RecommendationGroupDto(
                "buy-next",
                "Buy Next",
                ranked.Where(item => item.MissingIngredientNames.Count > 0).Take(4).Select(ToDto).ToList()),
        ];
    }

    private static CandidateScore Score(
        DrinkDetailDto drink,
        HashSet<Guid> favorites,
        HashSet<Guid> disliked,
        HashSet<Guid> owned)
    {
        var missingIngredientNames = drink.RecipeEntries
            .Where(entry => !owned.Contains(entry.Ingredient.Id))
            .Select(entry => entry.Ingredient.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var dislikedCount = drink.RecipeEntries.Count(entry => disliked.Contains(entry.Ingredient.Id));
        var favoriteCount = drink.RecipeEntries.Count(entry => favorites.Contains(entry.Ingredient.Id));

        var score = missingIngredientNames.Count == 0 ? 100 : 70 - missingIngredientNames.Count * 5;
        score += Math.Min(favoriteCount, 2) * 6;
        score -= dislikedCount * 8;

        return new CandidateScore(
            drink,
            missingIngredientNames,
            [],
            score);
    }

    private static RecommendationItemDto ToDto(CandidateScore result)
    {
        return new RecommendationItemDto(
            result.Drink.Id,
            result.Drink.Name,
            result.Drink.Description,
            result.MissingIngredientNames,
            result.MatchedSignals,
            result.Score);
    }

    private sealed record CandidateScore(
        DrinkDetailDto Drink,
        List<string> MissingIngredientNames,
        List<string> MatchedSignals,
        int Score);
}
