using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation;

public interface IRecommendationSessionQueryService
{
    Task<RecommendationSessionDto?> GetSessionAsync(
        string customerId,
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<List<RecommendationSessionSummaryDto>> GetSessionSummariesAsync(
        string customerId,
        CancellationToken cancellationToken = default);
}
