using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

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
        var ollama = ollamaOptions.Value;
        var llm = llmOptions.Value;
        if (string.IsNullOrWhiteSpace(ollama.ModelId))
        {
            throw new InvalidOperationException("Recommendation Ollama model id is required.");
        }

        if (!Uri.TryCreate(ollama.Endpoint, UriKind.Absolute, out var endpoint))
        {
            throw new InvalidOperationException(
                $"Recommendation Ollama endpoint '{ollama.Endpoint}' is invalid.");
        }

        var chatOptions = new ChatOptions
        {
            ModelId = ollama.ModelId,
            Temperature = llm.Sampling.Temperature,
            TopP = llm.Sampling.TopP,
            TopK = llm.Sampling.TopK,
        };

        if (llm.Reasoning.Enabled)
        {
            chatOptions.Reasoning = new ReasoningOptions
            {
                Effort = llm.Reasoning.Effort,
                Output = llm.Reasoning.Output,
            };
        }

        return new RecommendationChatClientStrategy(
            new OllamaApiClient(endpoint, ollama.ModelId),
            chatOptions,
            ollama.GetEffectiveMaxHistoryMessages());
    }
}
