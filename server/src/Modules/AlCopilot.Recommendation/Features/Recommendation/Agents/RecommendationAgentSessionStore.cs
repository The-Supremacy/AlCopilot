using System.Text.Json;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationAgentSessionStore : IRecommendationAgentSessionStore
{
    public async Task<AgentSession> RestoreAsync(
        string? serializedSessionState,
        AIAgent agent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agent);

        var session = string.IsNullOrWhiteSpace(serializedSessionState)
            ? await agent.CreateSessionAsync(cancellationToken)
            : await DeserializeAsync(serializedSessionState, agent, cancellationToken);
        return session;
    }

    public async Task<string> SerializeAsync(
        AgentSession agentSession,
        AIAgent agent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agentSession);
        ArgumentNullException.ThrowIfNull(agent);

        var serializedSession = await agent.SerializeSessionAsync(
            agentSession,
            cancellationToken: cancellationToken);

        return serializedSession.GetRawText();
    }

    private static async Task<AgentSession> DeserializeAsync(
        string serializedSessionState,
        AIAgent agent,
        CancellationToken cancellationToken)
    {
        using var document = JsonDocument.Parse(serializedSessionState);
        return await agent.DeserializeSessionAsync(
            document.RootElement.Clone(),
            cancellationToken: cancellationToken);
    }
}
