using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using AlCopilot.Shared.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationSemanticSearchService(
    IRecommendationEmbeddingClientFactory embeddingClientFactory,
    IRecommendationVectorStore vectorStore,
    IOptions<RecommendationSemanticOptions> semanticOptions,
    ILogger<RecommendationSemanticSearchService> logger) : IRecommendationSemanticSearchService
{
    private readonly RecommendationSemanticOptions options = semanticOptions.Value;

    public async Task<RecommendationSemanticSearchResult> SearchAsync(
        string customerMessage,
        CancellationToken cancellationToken = default)
    {
        if (!options.Enabled || string.IsNullOrWhiteSpace(customerMessage))
        {
            return RecommendationSemanticSearchResult.Empty;
        }

        try
        {
            var embeddingClient = embeddingClientFactory.Create();
            var queryVector = await embeddingClient.CreateEmbeddingAsync(customerMessage.TrimOrEmpty(), cancellationToken);
            var hits = await vectorStore.SearchAsync(
                queryVector,
                Math.Clamp(options.SearchLimit, 1, 50),
                cancellationToken);
            return RecommendationSemanticHitAggregator.Aggregate(hits, options);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Recommendation semantic search failed and will fall back to deterministic-only scoring.");
            return RecommendationSemanticSearchResult.Empty;
        }
    }
}
