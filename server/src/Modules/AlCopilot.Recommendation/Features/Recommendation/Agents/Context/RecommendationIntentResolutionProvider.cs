using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationIntentResolutionProvider(
    IRecommendationRequestIntentResolver requestIntentResolver,
    RecommendationAgentTurnState turnState) : AIContextProvider
{
    protected override async ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (turnState.Inputs is null || string.IsNullOrWhiteSpace(turnState.CustomerMessage))
        {
            turnState.Intent = null;
            return new AIContext();
        }

        turnState.Intent = await requestIntentResolver.ResolveAsync(
            turnState.CustomerMessage,
            turnState.Inputs,
            turnState.SemanticSearchResult,
            cancellationToken);

        return new AIContext();
    }
}
