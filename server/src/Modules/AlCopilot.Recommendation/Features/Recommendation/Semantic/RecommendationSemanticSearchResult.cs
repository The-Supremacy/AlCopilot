namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed record RecommendationSemanticSearchResult(
    IReadOnlyDictionary<Guid, RecommendationSemanticDrinkSignal> ByDrinkId)
{
    internal static RecommendationSemanticSearchResult Empty { get; } =
        new RecommendationSemanticSearchResult(new Dictionary<Guid, RecommendationSemanticDrinkSignal>());

    internal RecommendationSemanticDrinkSignal? TopNameMatch =>
        FindBestFacetMatch(RecommendationSemanticFacetKind.Name);

    internal RecommendationSemanticDrinkSignal? TopIngredientMatch =>
        FindBestFacetMatch(RecommendationSemanticFacetKind.Ingredient);

    internal RecommendationSemanticDrinkSignal? Find(Guid drinkId)
    {
        return ByDrinkId.TryGetValue(drinkId, out var signal) ? signal : null;
    }

    internal RecommendationSemanticDrinkSignal? FindTopFacetMatch(
        RecommendationSemanticFacetKind facetKind,
        double minScore,
        double minScoreGap)
    {
        var ordered = ByDrinkId.Values
            .Where(signal => signal.GetFacetScore(facetKind) > 0d)
            .OrderByDescending(signal => signal.GetFacetScore(facetKind))
            .ThenByDescending(signal => signal.WeightedScore)
            .ToList();

        if (ordered.Count == 0)
        {
            return null;
        }

        var best = ordered[0];
        if (best.GetFacetScore(facetKind) < minScore)
        {
            return null;
        }

        var runnerUpScore = ordered.Count > 1 ? ordered[1].GetFacetScore(facetKind) : 0d;
        return best.GetFacetScore(facetKind) - runnerUpScore >= minScoreGap
            ? best
            : null;
    }

    private RecommendationSemanticDrinkSignal? FindBestFacetMatch(RecommendationSemanticFacetKind facetKind)
    {
        return ByDrinkId.Values
            .Where(signal => signal.GetFacetScore(facetKind) > 0d)
            .OrderByDescending(signal => signal.GetFacetScore(facetKind))
            .ThenByDescending(signal => signal.WeightedScore)
            .FirstOrDefault();
    }
}
