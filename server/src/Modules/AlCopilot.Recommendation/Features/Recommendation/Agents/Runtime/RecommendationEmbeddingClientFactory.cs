using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using AlCopilot.Shared.Text;
using Microsoft.Extensions.Options;
using OllamaSharp;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationEmbeddingClientFactory(
    IOptions<RecommendationLlmOptions> llmOptions,
    IOptions<RecommendationOllamaOptions> ollamaOptions,
    IOptions<RecommendationSemanticOptions> semanticOptions) : IRecommendationEmbeddingClientFactory
{
    public IRecommendationEmbeddingClient Create()
    {
        var provider = llmOptions.Value.Provider;

        return provider.ToLowerInvariant() switch
        {
            RecommendationLlmOptions.OllamaProvider => CreateOllamaClient(),
            _ => throw new InvalidOperationException(
                $"Recommendation embedding provider '{provider}' is not supported. Supported providers: {RecommendationLlmOptions.OllamaProvider}."),
        };
    }

    private IRecommendationEmbeddingClient CreateOllamaClient()
    {
        var ollama = ollamaOptions.Value;
        if (!Uri.TryCreate(ollama.Endpoint, UriKind.Absolute, out var endpoint))
        {
            throw new InvalidOperationException($"Recommendation Ollama endpoint '{ollama.Endpoint}' is invalid.");
        }

        var modelId = string.IsNullOrWhiteSpace(semanticOptions.Value.EmbeddingModelId)
            ? ollama.ModelId
            : semanticOptions.Value.EmbeddingModelId;
        if (string.IsNullOrWhiteSpace(modelId))
        {
            throw new InvalidOperationException("Recommendation embedding model id is required.");
        }

        return new RecommendationOllamaEmbeddingClient(new OllamaApiClient(endpoint, modelId));
    }
}

internal sealed class RecommendationOllamaEmbeddingClient(OllamaApiClient client) : IRecommendationEmbeddingClient
{
    public async Task<ReadOnlyMemory<float>> CreateEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        var response = await client.EmbedAsync(input.TrimOrEmpty(), cancellationToken);
        var vector = response.Embeddings.FirstOrDefault()
            ?? throw new InvalidOperationException("Recommendation embedding request returned no vectors.");

        return new ReadOnlyMemory<float>(vector.ToArray());
    }
}
