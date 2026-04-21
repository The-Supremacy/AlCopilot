using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

internal interface IRecommendationAgentSessionStore
{
    Task<AgentSession> RestoreAsync(
        string? serializedSessionState,
        AIAgent agent,
        CancellationToken cancellationToken = default);

    Task<string> SerializeAsync(
        AgentSession agentSession,
        AIAgent agent,
        CancellationToken cancellationToken = default);
}
