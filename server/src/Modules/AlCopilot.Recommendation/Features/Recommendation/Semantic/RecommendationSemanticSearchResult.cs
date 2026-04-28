namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed record RecommendationSemanticSearchResult(
    IReadOnlyDictionary<Guid, RecommendationSemanticSearchResult.DrinkMatch> ByDrinkId)
{
    internal static RecommendationSemanticSearchResult Empty { get; } =
        new RecommendationSemanticSearchResult(new Dictionary<Guid, DrinkMatch>());

    internal DrinkMatch? Find(Guid drinkId)
    {
        return ByDrinkId.TryGetValue(drinkId, out var match) ? match : null;
    }

    internal sealed record DrinkMatch(
        Guid DrinkId,
        string DrinkName,
        double WeightedScore,
        IReadOnlyCollection<DescriptionMatch> DescriptionMatches)
    {
        internal DrinkMatch(
            Guid drinkId,
            string drinkName,
            double weightedScore,
            IReadOnlyCollection<string> descriptionMatches)
            : this(
                drinkId,
                drinkName,
                weightedScore,
                descriptionMatches.Select(text => new DescriptionMatch(text, 0d)).ToList())
        {
        }

        internal IReadOnlyCollection<string> SummaryHints =>
            DescriptionMatches
                .Select(match => match.Text)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
    }

    internal sealed record DescriptionMatch(
        string Text,
        double Score);
}
