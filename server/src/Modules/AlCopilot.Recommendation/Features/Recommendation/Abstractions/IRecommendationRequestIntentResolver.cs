namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

internal interface IRecommendationRequestIntentResolver
{
    Task<RecommendationRequestIntent> ResolveAsync(
        string customerMessage,
        RecommendationRunInputs inputs,
        RecommendationSemanticSearchResult semanticSearchResult,
        CancellationToken cancellationToken = default);
}
