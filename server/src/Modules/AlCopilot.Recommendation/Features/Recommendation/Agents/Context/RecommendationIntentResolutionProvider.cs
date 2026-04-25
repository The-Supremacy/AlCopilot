using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationIntentResolutionProvider(
    IRecommendationRequestIntentResolver requestIntentResolver) : AIContextProvider
{
    internal static readonly ProviderSessionState<RecommendationIntentResolutionProviderState> SessionState = new(
        _ => new RecommendationIntentResolutionProviderState(),
        "recommendation.intent-resolution");

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
        var semanticState = RecommendationSemanticSearchProvider.SessionState.GetOrInitializeState(context.Session);
        var intentState = SessionState.GetOrInitializeState(context.Session);

        if (inputsState.Inputs is null || string.IsNullOrWhiteSpace(invocationState.CustomerMessage))
        {
            intentState.Intent = null;
            SessionState.SaveState(context.Session, intentState);
            return new AIContext();
        }

        intentState.Intent = await requestIntentResolver.ResolveAsync(
            invocationState.CustomerMessage,
            inputsState.Inputs,
            semanticState.SemanticSearchResult,
            cancellationToken);
        SessionState.SaveState(context.Session, intentState);

        return new AIContext();
    }
}

internal sealed class RecommendationIntentResolutionProviderState
{
    public RecommendationRequestIntent? Intent { get; set; }
}
