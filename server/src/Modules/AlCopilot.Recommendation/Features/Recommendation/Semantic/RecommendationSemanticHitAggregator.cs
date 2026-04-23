namespace AlCopilot.Recommendation.Features.Recommendation;

internal static class RecommendationSemanticHitAggregator
{
    internal static RecommendationSemanticSearchResult Aggregate(
        IReadOnlyCollection<RecommendationSemanticHit> hits,
        RecommendationSemanticOptions options)
    {
        var grouped = hits
            .GroupBy(hit => hit.DrinkId)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var weightedScore = group.Sum(hit => hit.Score * GetFacetWeight(hit.FacetKind, options));
                    var nameScore = group
                        .Where(hit => hit.FacetKind == RecommendationSemanticFacetKind.Name)
                        .Select(hit => hit.Score)
                        .DefaultIfEmpty(0d)
                        .Max();
                    var ingredientScore = group
                        .Where(hit => hit.FacetKind == RecommendationSemanticFacetKind.Ingredient)
                        .Select(hit => hit.Score)
                        .DefaultIfEmpty(0d)
                        .Max();
                    var descriptionScore = group
                        .Where(hit => hit.FacetKind == RecommendationSemanticFacetKind.Description)
                        .Select(hit => hit.Score)
                        .DefaultIfEmpty(0d)
                        .Max();
                    var matchedFacets = group.Select(hit => hit.FacetKind)
                        .Distinct()
                        .OrderBy(kind => kind.ToString(), StringComparer.Ordinal)
                        .ToList();
                    var matchedIngredients = group
                        .Where(hit => hit.FacetKind == RecommendationSemanticFacetKind.Ingredient)
                        .Select(hit => hit.MatchedIngredientName ?? hit.Text)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    var matchedDescriptors = group
                        .Where(hit => hit.FacetKind == RecommendationSemanticFacetKind.Description)
                        .Select(hit => hit.Text)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    var matchedTexts = group.Select(hit => hit.Text)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    return new RecommendationSemanticDrinkSignal(
                        group.Key,
                        group.Select(hit => hit.DrinkName).First(),
                        weightedScore,
                        nameScore,
                        ingredientScore,
                        descriptionScore,
                        matchedFacets,
                        matchedIngredients,
                        matchedDescriptors,
                        matchedTexts);
                });

        return new RecommendationSemanticSearchResult(grouped);
    }

    private static double GetFacetWeight(
        RecommendationSemanticFacetKind facetKind,
        RecommendationSemanticOptions options)
    {
        return facetKind switch
        {
            RecommendationSemanticFacetKind.Name => options.NameWeight,
            RecommendationSemanticFacetKind.Ingredient => options.IngredientWeight,
            RecommendationSemanticFacetKind.Description => options.DescriptionWeight,
            _ => 1d,
        };
    }
}
