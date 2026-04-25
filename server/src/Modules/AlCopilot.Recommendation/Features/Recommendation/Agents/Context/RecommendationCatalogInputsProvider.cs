using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.Agents.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationCatalogInputsProvider(
    IRecommendationRunInputsQueryService runInputsQueryService) : AIContextProvider
{
    internal static readonly ProviderSessionState<RecommendationCatalogInputsProviderState> SessionState = new(
        _ => new RecommendationCatalogInputsProviderState(),
        "recommendation.catalog-inputs");

    public override IReadOnlyList<string> StateKeys =>
    [
        SessionState.StateKey,
    ];

    protected override async ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var state = SessionState.GetOrInitializeState(context.Session);
        state.Inputs = await runInputsQueryService.GetRunInputsAsync(cancellationToken);
        SessionState.SaveState(context.Session, state);
        return new AIContext();
    }
}

internal sealed class RecommendationCatalogInputsProviderState
{
    public RecommendationRunInputs? Inputs { get; set; }
}
