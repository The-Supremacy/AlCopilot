using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationSemanticSearchProvider(
    IRecommendationSemanticSearchService semanticSearchService,
    RecommendationAgentTurnState turnState) : AIContextProvider
{
    protected override async ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (turnState.RequestAnalysis is null
            || turnState.Inputs is null
            || string.IsNullOrWhiteSpace(turnState.CustomerMessage))
        {
            turnState.SemanticSearchResult = RecommendationSemanticSearchResult.Empty;
            return new AIContext();
        }

        turnState.SemanticSearchResult = RecommendationSemanticSearchPolicy.ShouldUseSemanticSearch(
                turnState.RequestAnalysis,
                turnState.Inputs)
            ? await semanticSearchService.SearchAsync(turnState.CustomerMessage, cancellationToken)
            : RecommendationSemanticSearchResult.Empty;

        return new AIContext();
    }
}
