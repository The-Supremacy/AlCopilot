using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

internal interface IRecommendationNarratorAgentFactory
{
    RecommendationNarratorAgentRuntime Create(ChatSession session, AgentRun agentRun);
}

internal sealed class RecommendationNarratorAgentRuntime(
    AIAgent agent,
    Func<RecommendationRunContext?> getRunContext)
{
    public AIAgent Agent { get; } = agent;

    public RecommendationRunContext? RunContext => getRunContext();
}
