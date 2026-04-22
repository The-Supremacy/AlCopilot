namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

internal interface IRecommendationCurrentRunContextAccessor
{
    RecommendationRunContext? Current { get; set; }
}
