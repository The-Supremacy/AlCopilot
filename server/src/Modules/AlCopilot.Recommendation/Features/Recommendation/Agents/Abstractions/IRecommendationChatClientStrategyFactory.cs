using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

internal interface IRecommendationChatClientStrategyFactory
{
    RecommendationChatClientStrategy Create();
}

public sealed record RecommendationChatClientStrategy(
    IChatClient ChatClient,
    ChatOptions ChatOptions);
