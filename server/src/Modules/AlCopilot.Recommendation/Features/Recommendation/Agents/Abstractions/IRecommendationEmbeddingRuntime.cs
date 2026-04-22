namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

internal interface IRecommendationEmbeddingRuntime
{
    Task<ReadOnlyMemory<float>> CreateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default);
}
