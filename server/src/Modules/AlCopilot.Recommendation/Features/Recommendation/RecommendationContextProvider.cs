using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationContextProvider(string instructions) : AIContextProvider
{
    protected override ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        return new ValueTask<AIContext>(new AIContext
        {
            Instructions = instructions,
        });
    }
}
