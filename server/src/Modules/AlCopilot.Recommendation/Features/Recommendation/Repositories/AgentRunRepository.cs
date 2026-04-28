using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class AgentRunRepository(RecommendationDbContext dbContext) : IAgentRunRepository
{
    public void Add(AgentRun run) => dbContext.AgentRuns.Add(run);
}
