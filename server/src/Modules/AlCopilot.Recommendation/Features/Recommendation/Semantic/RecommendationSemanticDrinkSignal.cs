namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed record RecommendationSemanticDrinkSignal(
    Guid DrinkId,
    string DrinkName,
    double WeightedScore,
    double NameScore,
    double IngredientScore,
    double DescriptionScore,
    IReadOnlyCollection<RecommendationSemanticFacetKind> MatchedFacets,
    IReadOnlyCollection<string> MatchedIngredients,
    IReadOnlyCollection<string> MatchedDescriptors,
    IReadOnlyCollection<string> MatchedTexts)
{
    internal IReadOnlyCollection<string> SummaryHints =>
        MatchedTexts
            .Concat(MatchedIngredients)
            .Concat(MatchedDescriptors)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();

    internal double GetFacetScore(RecommendationSemanticFacetKind facetKind)
    {
        return facetKind switch
        {
            RecommendationSemanticFacetKind.Name => NameScore,
            RecommendationSemanticFacetKind.Ingredient => IngredientScore,
            RecommendationSemanticFacetKind.Description => DescriptionScore,
            _ => 0d,
        };
    }
}
