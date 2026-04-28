namespace AlCopilot.Recommendation.Features.Recommendation;

internal static class RecommendationSemanticHitAggregator
{
    internal static RecommendationSemanticSearchResult Aggregate(
        IReadOnlyCollection<RecommendationSemanticHit> hits,
        RecommendationSemanticOptions options)
    {
        var grouped = hits
            .Where(hit => hit.FacetKind == RecommendationSemanticFacetKind.Description)
            .Where(hit => hit.Score >= options.DescriptionMinScore)
            .GroupBy(hit => hit.DrinkId)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var weightedScore = group.Sum(hit => hit.Score * options.DescriptionWeight);
                    var descriptionMatches = group
                        .GroupBy(hit => hit.Text, StringComparer.OrdinalIgnoreCase)
                        .Select(groupedHit => new RecommendationSemanticSearchResult.DescriptionMatch(
                            groupedHit.Key,
                            groupedHit.Max(hit => hit.Score)))
                        .OrderByDescending(match => match.Score)
                        .ThenBy(match => match.Text, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    return new RecommendationSemanticSearchResult.DrinkMatch(
                        group.Key,
                        group.Select(hit => hit.DrinkName).First(),
                        weightedScore,
                        descriptionMatches);
                });

        return new RecommendationSemanticSearchResult(grouped);
    }
}
