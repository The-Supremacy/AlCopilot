namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

internal interface IRecommendationSemanticSearchService
{
    Task<RecommendationSemanticSearchResult> SearchAsync(
        string customerMessage,
        CancellationToken cancellationToken = default);
}
