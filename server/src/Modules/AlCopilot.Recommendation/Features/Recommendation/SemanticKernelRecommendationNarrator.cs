using AlCopilot.Recommendation.Contracts.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class SemanticKernelRecommendationNarrator(
    RecommendationKernelFactory kernelFactory,
    IRecommendationNarrationComposer narrationComposer,
    ILogger<SemanticKernelRecommendationNarrator> logger) : IRecommendationNarrator
{
    public async Task<RecommendationNarrationResult> GenerateAsync(
        RecommendationNarrationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var context = kernelFactory.Create();
            var history = narrationComposer.BuildChatHistory(request, context.MaxHistoryTurns);
            var completion = await context.ChatCompletion.GetChatMessageContentAsync(
                history,
                new OllamaPromptExecutionSettings
                {
                    Temperature = 0.2f,
                    TopP = 0.9f,
                },
                context.Kernel,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(completion.Content))
            {
                throw new InvalidOperationException("Recommendation LLM returned an empty assistant message.");
            }

            return new RecommendationNarrationResult(completion.Content.Trim(), []);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Recommendation narration failed. The customer portal requires a working configured LLM provider.");
            throw;
        }
    }
}
