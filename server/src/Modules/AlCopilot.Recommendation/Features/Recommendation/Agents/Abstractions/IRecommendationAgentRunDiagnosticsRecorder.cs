using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

internal interface IRecommendationAgentRunDiagnosticsRecorder
{
    void Record(ChatSession session, AgentRun agentRun, AgentResponse response);
}
