namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

internal interface IRecommendationVectorStore
{
    Task ReplaceCatalogAsync(
        IReadOnlyCollection<RecommendationVectorPoint> points,
        int vectorSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RecommendationSemanticHit>> SearchAsync(
        ReadOnlyMemory<float> queryVector,
        int limit,
        CancellationToken cancellationToken = default);
}
