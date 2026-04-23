using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Google.Protobuf.Collections;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationQdrantVectorStore(
    IOptions<RecommendationSemanticOptions> semanticOptions,
    ILogger<RecommendationQdrantVectorStore> logger) : IRecommendationVectorStore
{
    private readonly RecommendationSemanticOptions options = semanticOptions.Value;

    public async Task ReplaceCatalogAsync(
        IReadOnlyCollection<RecommendationVectorPoint> points,
        int vectorSize,
        CancellationToken cancellationToken = default)
    {
        if (!options.Enabled)
        {
            return;
        }

        try
        {
            var client = CreateClient();
            await RebuildCollectionAsync(client, vectorSize, points, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Recommendation semantic catalog replacement failed because Qdrant was unavailable.");
            throw;
        }
    }

    public async Task<IReadOnlyCollection<RecommendationSemanticHit>> SearchAsync(
        ReadOnlyMemory<float> queryVector,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (!options.Enabled || queryVector.Length == 0)
        {
            return [];
        }

        try
        {
            var client = CreateClient();

            var matches = await client.SearchAsync(
                options.CollectionName,
                queryVector.ToArray(),
                null,
                null,
                (ulong)Math.Clamp(limit, 1, 50),
                0,
                true,
                false,
                null,
                null,
                null,
                null,
                null,
                null,
                cancellationToken);

            return matches
                .Select(match =>
                {
                    if (!TryReadPayload(match.Payload, out var hit))
                    {
                        return null;
                    }

                    return hit with { Score = match.Score };
                })
                .Where(hit => hit is not null)
                .Cast<RecommendationSemanticHit>()
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Recommendation semantic retrieval fell back to deterministic-only mode because Qdrant was unavailable.");
            return [];
        }
    }

    private QdrantClient CreateClient()
    {
        var endpoint = new Uri(options.QdrantEndpoint, UriKind.Absolute);
        return new QdrantClient(
            host: endpoint.Host,
            port: endpoint.Port,
            https: string.Equals(endpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase),
            apiKey: string.IsNullOrWhiteSpace(endpoint.UserInfo) ? null : endpoint.UserInfo);
    }

    private async Task RebuildCollectionAsync(
        QdrantClient client,
        int vectorSize,
        IReadOnlyCollection<RecommendationVectorPoint> points,
        CancellationToken cancellationToken)
    {
        try
        {
            await client.DeleteCollectionAsync(options.CollectionName, cancellationToken: cancellationToken);
        }
        catch
        {
            // Missing collection is fine for rebuild semantics.
        }

        await client.CreateCollectionAsync(
            options.CollectionName,
            new VectorParams
            {
                Size = (ulong)Math.Max(vectorSize, 1),
                Distance = Distance.Cosine,
            },
            cancellationToken: cancellationToken);

        if (points.Count == 0)
        {
            return;
        }

        var pointStructs = points
            .Select(point => new PointStruct
            {
                Id = point.PointId,
                Vectors = point.Vector.ToArray(),
                Payload =
                {
                    ["drinkId"] = point.DrinkId.ToString(),
                    ["drinkName"] = point.DrinkName,
                    ["facetKind"] = point.FacetKind.ToString(),
                    ["text"] = point.Text,
                    ["matchedIngredientName"] = point.MatchedIngredientName ?? string.Empty,
                    ["pointId"] = point.PointId.ToString(),
                },
            })
            .ToList();

        await client.UpsertAsync(options.CollectionName, pointStructs, cancellationToken: cancellationToken);
    }

    private static bool TryReadPayload(
        MapField<string, Value> payload,
        out RecommendationSemanticHit hit)
    {
        hit = null!;

        if (!payload.TryGetValue("drinkId", out var drinkIdValue)
            || !Guid.TryParse(drinkIdValue.StringValue, out var drinkId)
            || !payload.TryGetValue("drinkName", out var drinkNameValue)
            || !payload.TryGetValue("facetKind", out var facetKindValue)
            || !Enum.TryParse<RecommendationSemanticFacetKind>(facetKindValue.StringValue, out var facetKind)
            || !payload.TryGetValue("text", out var textValue))
        {
            return false;
        }

        Guid pointId = Guid.Empty;
        if (payload.TryGetValue("pointId", out var pointIdValue))
        {
            Guid.TryParse(pointIdValue.StringValue, out pointId);
        }

        var matchedIngredientName = payload.TryGetValue("matchedIngredientName", out var ingredientValue)
            ? ingredientValue.StringValue
            : null;
        if (string.IsNullOrWhiteSpace(matchedIngredientName))
        {
            matchedIngredientName = null;
        }

        hit = new RecommendationSemanticHit(
            pointId,
            drinkId,
            drinkNameValue.StringValue,
            facetKind,
            textValue.StringValue,
            matchedIngredientName,
            0d);
        return true;
    }
}
