using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

internal interface IRecommendationNarratorAgentFactory
{
    AIAgent Create(ChatSession session);
}
