using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

internal interface IRecommendationAgentFactory
{
    RecommendationAgentRuntime Create(
        RecommendationAgentDefinition definition,
        string contextInstructions,
        ChatSession session);
}

internal sealed record RecommendationAgentDefinition(
    string Name,
    string Description,
    string Instructions);

internal sealed record RecommendationAgentRuntime(
    ChatClientAgent Agent,
    ChatOptions ChatOptions,
    int MaxHistoryTurns);
