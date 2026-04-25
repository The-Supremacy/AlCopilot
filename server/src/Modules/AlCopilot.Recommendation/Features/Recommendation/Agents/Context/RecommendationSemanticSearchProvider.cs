using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationSemanticSearchProvider(
    IRecommendationSemanticSearchService semanticSearchService) : AIContextProvider
{
    internal static readonly ProviderSessionState<RecommendationSemanticSearchProviderState> SessionState = new(
        _ => new RecommendationSemanticSearchProviderState(),
        "recommendation.semantic-search");

    public override IReadOnlyList<string> StateKeys =>
    [
        SessionState.StateKey,
    ];

    protected override async ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var invocationState = RecommendationInvocationContextProvider.SessionState.GetOrInitializeState(context.Session);
        var inputsState = RecommendationCatalogInputsProvider.SessionState.GetOrInitializeState(context.Session);
        var semanticState = SessionState.GetOrInitializeState(context.Session);
        if (invocationState.RequestAnalysis is null
            || inputsState.Inputs is null
            || string.IsNullOrWhiteSpace(invocationState.CustomerMessage))
        {
            semanticState.SemanticSearchResult = RecommendationSemanticSearchResult.Empty;
            SessionState.SaveState(context.Session, semanticState);
            return new AIContext();
        }

        semanticState.SemanticSearchResult = RecommendationSemanticSearchPolicy.ShouldUseSemanticSearch(
                invocationState.RequestAnalysis,
                inputsState.Inputs)
            ? await semanticSearchService.SearchAsync(invocationState.CustomerMessage, cancellationToken)
            : RecommendationSemanticSearchResult.Empty;
        SessionState.SaveState(context.Session, semanticState);

        return new AIContext();
    }
}

internal sealed class RecommendationSemanticSearchProviderState
{
    public RecommendationSemanticSearchResult SemanticSearchResult { get; set; } =
        RecommendationSemanticSearchResult.Empty;
}
