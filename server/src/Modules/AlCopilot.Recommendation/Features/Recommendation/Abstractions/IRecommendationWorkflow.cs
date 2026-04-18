using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

public interface IRecommendationWorkflow
{
    Task<RecommendationSessionDto> ExecuteAsync(
        string customerId,
        Guid? sessionId,
        string message,
        CancellationToken cancellationToken = default);
}
