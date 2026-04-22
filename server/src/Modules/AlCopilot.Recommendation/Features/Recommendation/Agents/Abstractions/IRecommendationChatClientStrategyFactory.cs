using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

public interface IRecommendationChatClientStrategyFactory
{
    RecommendationChatClientStrategy Create();
}

public sealed record RecommendationChatClientStrategy(
    IChatClient ChatClient,
    ChatOptions ChatOptions,
    int MaxHistoryMessages);
