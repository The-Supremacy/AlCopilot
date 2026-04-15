using Microsoft.SemanticKernel.ChatCompletion;

namespace AlCopilot.Recommendation.Features.Recommendation;

public interface IRecommendationNarrationComposer
{
    ChatHistory BuildChatHistory(RecommendationNarrationRequest request, int maxHistoryTurns);
}
