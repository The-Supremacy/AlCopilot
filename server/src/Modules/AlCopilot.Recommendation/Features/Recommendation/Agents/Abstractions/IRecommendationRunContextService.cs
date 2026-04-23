namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

internal interface IRecommendationRunContextService
{
    Task<RecommendationRunContext> CreateAsync(
        string customerMessage,
        CancellationToken cancellationToken = default);
}
