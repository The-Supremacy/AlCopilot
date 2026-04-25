using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationNarrationContextProvider(
    IRecommendationCandidateBuilder candidateBuilder,
    IRecommendationRunContextBuilder runContextBuilder,
    IRecommendationExecutionTraceRecorder executionTraceRecorder) : AIContextProvider
{
    internal static readonly ProviderSessionState<RecommendationNarrationContextProviderState> SessionState = new(
        _ => new RecommendationNarrationContextProviderState(),
        "recommendation.narration-context");

    public override IReadOnlyList<string> StateKeys =>
    [
        SessionState.StateKey,
    ];

    protected override ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var invocationState = RecommendationInvocationContextProvider.SessionState.GetOrInitializeState(context.Session);
        var inputsState = RecommendationCatalogInputsProvider.SessionState.GetOrInitializeState(context.Session);
        var semanticState = RecommendationSemanticSearchProvider.SessionState.GetOrInitializeState(context.Session);
        var intentState = RecommendationIntentResolutionProvider.SessionState.GetOrInitializeState(context.Session);
        var narrationContextState = SessionState.GetOrInitializeState(context.Session);

        if (inputsState.Inputs is null
            || intentState.Intent is null
            || string.IsNullOrWhiteSpace(invocationState.CustomerMessage))
        {
            return ValueTask.FromResult(new AIContext());
        }

        var groups = candidateBuilder.Build(
            invocationState.CustomerMessage,
            intentState.Intent,
            inputsState.Inputs.Profile,
            inputsState.Inputs.Drinks,
            semanticState.SemanticSearchResult);
        narrationContextState.RunContext = runContextBuilder.Build(
            intentState.Intent,
            inputsState.Inputs.Profile,
            inputsState.Inputs.Drinks,
            groups,
            semanticState.SemanticSearchResult);
        SessionState.SaveState(context.Session, narrationContextState);

        var runContext = narrationContextState.RunContext;
        executionTraceRecorder.Record(
            new RecommendationExecutionTraceStep(
                "run_context.build",
                "ok",
                $"Built {runContext.Groups.Count} recommendation group(s) for {intentState.Intent.Kind}.",
                DateTimeOffset.UtcNow,
                new Dictionary<string, string?>
                {
                    ["intentKind"] = intentState.Intent.Kind.ToString(),
                    ["requestedDrinkName"] = intentState.Intent.RequestedDrinkName,
                    ["requestedIngredientNames"] = string.Join(", ", intentState.Intent.RequestedIngredientNames),
                    ["matchedRequestDescriptors"] = string.Join(", ", intentState.Intent.RequestDescriptors),
                    ["semanticDrinkSignals"] = semanticState.SemanticSearchResult.ByDrinkId.Count.ToString(),
                    ["groupCount"] = runContext.Groups.Count.ToString(),
                    ["itemCount"] = runContext.Groups.Sum(group => group.Items.Count).ToString(),
                },
                []));

        return ValueTask.FromResult(new AIContext
        {
            Messages =
            [
                new ChatMessage(
                    ChatRole.System,
                    RecommendationRunContextMessageBuilder.Build(runContext)),
            ],
        });
    }
}

internal sealed class RecommendationNarrationContextProviderState
{
    public RecommendationRunContext? RunContext { get; set; }
}
