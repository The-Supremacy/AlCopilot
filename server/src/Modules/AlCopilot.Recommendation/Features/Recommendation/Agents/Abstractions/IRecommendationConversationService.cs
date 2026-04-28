using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

public interface IRecommendationConversationService
{
    Task<SubmitRecommendationMessageResultDto> SendMessageAsync(
        string customerId,
        Guid? sessionId,
        string message,
        CancellationToken cancellationToken = default);
}
