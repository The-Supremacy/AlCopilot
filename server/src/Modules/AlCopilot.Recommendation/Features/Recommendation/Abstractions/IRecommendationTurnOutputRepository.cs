namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

internal interface IRecommendationTurnOutputRepository
{
    void AddRange(IEnumerable<RecommendationTurnGroup> groups);
}
