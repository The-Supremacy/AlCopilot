using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

public interface IRecommendationConversationService
{
    Task<RecommendationSessionDto> SendMessageAsync(
        string customerId,
        Guid? sessionId,
        string message,
        CancellationToken cancellationToken = default);
}
