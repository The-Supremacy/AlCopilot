using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationCatalogInputsProvider(
    IRecommendationRunInputsQueryService runInputsQueryService,
    RecommendationAgentTurnState turnState) : AIContextProvider
{
    protected override async ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        turnState.Inputs = await runInputsQueryService.GetRunInputsAsync(cancellationToken);
        return new AIContext();
    }
}
