using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationCurrentRunContextAccessor : IRecommendationCurrentRunContextAccessor
{
    public RecommendationRunContext? Current { get; set; }
}
