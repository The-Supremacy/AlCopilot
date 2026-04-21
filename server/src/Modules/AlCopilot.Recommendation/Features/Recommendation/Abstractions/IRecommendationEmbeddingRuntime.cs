namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

internal interface IRecommendationEmbeddingRuntime
{
    Task<ReadOnlyMemory<float>> CreateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default);
}
