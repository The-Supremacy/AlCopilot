namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

internal interface IRecommendationCatalogFuzzyLookupService
{
    Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindDrinkMatchesAsync(
        string searchText,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RecommendationFuzzyMatch>> FindIngredientMatchesAsync(
        string searchText,
        CancellationToken cancellationToken = default);
}
