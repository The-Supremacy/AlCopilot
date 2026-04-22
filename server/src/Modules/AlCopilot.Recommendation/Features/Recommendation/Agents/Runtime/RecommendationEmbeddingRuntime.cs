using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationEmbeddingRuntime : IRecommendationEmbeddingRuntime
{
    public Task<ReadOnlyMemory<float>> CreateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            "Recommendation embeddings are not implemented yet. This runtime is a seam for future embedding support.");
    }
}
