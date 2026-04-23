using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Microsoft.Extensions.Logging;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationSemanticIndexingService(
    IRecommendationEmbeddingClientFactory embeddingClientFactory,
    IRecommendationVectorStore vectorStore,
    ILogger<RecommendationSemanticIndexingService> logger) : IRecommendationSemanticIndexingService
{
    public async Task<RecommendationSemanticCatalogIndexResult> ReplaceCatalogAsync(
        IReadOnlyCollection<DrinkDetailDto> drinks,
        CancellationToken cancellationToken = default)
    {
        var projectionPoints = RecommendationSemanticProjectionBuilder.Build(drinks);
        if (projectionPoints.Count == 0)
        {
            await vectorStore.ReplaceCatalogAsync([], 0, cancellationToken);
            return new RecommendationSemanticCatalogIndexResult(drinks.Count, 0);
        }

        var embeddingClient = embeddingClientFactory.Create();
        var vectorPoints = new List<RecommendationVectorPoint>(projectionPoints.Count);

        foreach (var point in projectionPoints)
        {
            var vector = await embeddingClient.CreateEmbeddingAsync(point.Text, cancellationToken);
            vectorPoints.Add(new RecommendationVectorPoint(
                point.PointId,
                point.DrinkId,
                point.DrinkName,
                point.FacetKind,
                point.Text,
                point.MatchedIngredientName,
                vector));
        }

        await vectorStore.ReplaceCatalogAsync(vectorPoints, vectorPoints[0].Vector.Length, cancellationToken);
        logger.LogInformation(
            "Replaced recommendation semantic catalog in vector store with {DrinkCount} drinks and {PointCount} points.",
            drinks.Count,
            vectorPoints.Count);

        return new RecommendationSemanticCatalogIndexResult(drinks.Count, vectorPoints.Count);
    }
}
