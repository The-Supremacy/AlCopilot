namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

internal interface IRecommendationEmbeddingClientFactory
{
    IRecommendationEmbeddingClient Create();
}

internal interface IRecommendationEmbeddingClient
{
    Task<ReadOnlyMemory<float>> CreateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default);
}
