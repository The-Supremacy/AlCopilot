using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationTurnOutputRepository(RecommendationDbContext dbContext)
    : IRecommendationTurnOutputRepository
{
    public void AddRange(IEnumerable<RecommendationTurnGroup> groups) =>
        dbContext.RecommendationTurnGroups.AddRange(groups);
}
