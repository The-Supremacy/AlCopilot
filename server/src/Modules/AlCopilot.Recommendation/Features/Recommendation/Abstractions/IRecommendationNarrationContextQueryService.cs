using AlCopilot.Recommendation.Features.Recommendation.Agents;

namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

public interface IRecommendationRunContextQueryService
{
    Task<RecommendationRunContext> GetRunContextAsync(
        string customerMessage,
        CancellationToken cancellationToken = default);
}
