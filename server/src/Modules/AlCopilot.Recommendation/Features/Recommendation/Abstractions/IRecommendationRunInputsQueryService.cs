namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

internal interface IRecommendationRunInputsQueryService
{
    Task<RecommendationRunInputs> GetRunInputsAsync(CancellationToken cancellationToken = default);
}
