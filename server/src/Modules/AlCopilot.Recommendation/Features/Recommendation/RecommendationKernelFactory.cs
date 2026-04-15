using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationKernelFactory(
    RecommendationReadOnlyTools readOnlyTools,
    IOptions<RecommendationLlmOptions> llmOptions,
    IOptions<RecommendationOllamaOptions> ollamaOptions)
{
    public RecommendationKernelContext Create()
    {
        var provider = llmOptions.Value.Provider;

        return provider.ToLowerInvariant() switch
        {
            RecommendationLlmOptions.OllamaProvider => CreateOllamaContext(),
            _ => throw new InvalidOperationException(
                $"Recommendation LLM provider '{provider}' is not supported. Supported providers: {RecommendationLlmOptions.OllamaProvider}."),
        };
    }

    private RecommendationKernelContext CreateOllamaContext()
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

        var kernelBuilder = Kernel.CreateBuilder()
            .AddOllamaChatCompletion(options.ModelId, endpoint, serviceId: "recommendation-ollama");

        if (options.EnableReadOnlyTools)
        {
            kernelBuilder.Plugins.AddFromObject(readOnlyTools, "RecommendationTools");
        }

        var kernel = kernelBuilder.Build();
        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        return new RecommendationKernelContext(kernel, chatCompletion, options.MaxHistoryTurns);
    }
}

internal sealed record RecommendationKernelContext(
    Kernel Kernel,
    IChatCompletionService ChatCompletion,
    int MaxHistoryTurns);
