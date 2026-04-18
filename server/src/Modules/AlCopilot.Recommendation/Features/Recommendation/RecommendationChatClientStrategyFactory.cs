using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationChatClientStrategyFactory(
    IOptions<RecommendationLlmOptions> llmOptions,
    IOptions<RecommendationOllamaOptions> ollamaOptions) : IRecommendationChatClientStrategyFactory
{
    public RecommendationChatClientStrategy Create()
    {
        var provider = llmOptions.Value.Provider;

        return provider.ToLowerInvariant() switch
        {
            RecommendationLlmOptions.OllamaProvider => CreateOllamaStrategy(),
            _ => throw new InvalidOperationException(
                $"Recommendation LLM provider '{provider}' is not supported. Supported providers: {RecommendationLlmOptions.OllamaProvider}."),
        };
    }

    private RecommendationChatClientStrategy CreateOllamaStrategy()
    {
        var options = ollamaOptions.Value;
        if (string.IsNullOrWhiteSpace(options.ModelId))
        {
            throw new InvalidOperationException("Recommendation Ollama model id is required.");
        }

        if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out var endpoint))
        {
            throw new InvalidOperationException(
                $"Recommendation Ollama endpoint '{options.Endpoint}' is invalid.");
        }

        return new RecommendationChatClientStrategy(
            new OllamaApiClient(endpoint, options.ModelId),
            new ChatOptions
            {
                ModelId = options.ModelId,
                Temperature = 0.2f,
                TopP = 0.9f,
            },
            options.MaxHistoryTurns);
    }
}
