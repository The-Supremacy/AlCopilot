namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

public interface IRecommendationNarrationContextQueryService
{
    Task<RecommendationNarrationSnapshot> GetSnapshotAsync(
        string customerMessage,
        CancellationToken cancellationToken = default);
}
