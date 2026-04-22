using AlCopilot.Recommendation.Features.Recommendation.Agents;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

internal interface IRecommendationRunContextFactory
{
    Task<RecommendationRunContext> CreateAsync(
        string customerMessage,
        CancellationToken cancellationToken = default);
}
