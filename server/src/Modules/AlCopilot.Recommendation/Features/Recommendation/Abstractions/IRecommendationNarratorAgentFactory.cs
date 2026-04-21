using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

internal interface IRecommendationNarratorAgentFactory
{
    AIAgent Create();
}
