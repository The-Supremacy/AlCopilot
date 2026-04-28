using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class AgentMessageDiagnosticRepository(RecommendationDbContext dbContext)
    : IAgentMessageDiagnosticRepository
{
    public void Add(AgentMessageDiagnostic diagnostic) => dbContext.AgentMessageDiagnostics.Add(diagnostic);
}
