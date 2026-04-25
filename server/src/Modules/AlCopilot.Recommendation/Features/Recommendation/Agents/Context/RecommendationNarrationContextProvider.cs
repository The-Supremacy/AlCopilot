using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationNarrationContextProvider(
    IRecommendationCandidateBuilder candidateBuilder,
    IRecommendationRunContextBuilder runContextBuilder,
    IRecommendationExecutionTraceRecorder executionTraceRecorder,
    RecommendationAgentTurnState turnState) : AIContextProvider
{
    protected override ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (turnState.Inputs is null
            || turnState.Intent is null
            || string.IsNullOrWhiteSpace(turnState.CustomerMessage))
        {
            return ValueTask.FromResult(new AIContext());
        }

        var groups = candidateBuilder.Build(
            turnState.CustomerMessage,
            turnState.Intent,
            turnState.Inputs.Profile,
            turnState.Inputs.Drinks,
            turnState.SemanticSearchResult);
        turnState.RunContext = runContextBuilder.Build(
            turnState.Intent,
            turnState.Inputs.Profile,
            turnState.Inputs.Drinks,
            groups,
            turnState.SemanticSearchResult);

        var runContext = turnState.RunContext;
        executionTraceRecorder.Record(
            new RecommendationExecutionTraceStep(
                "run_context.build",
                "ok",
                $"Built {runContext.Groups.Count} recommendation group(s) for {turnState.Intent.Kind}.",
                DateTimeOffset.UtcNow,
                new Dictionary<string, string?>
                {
                    ["intentKind"] = turnState.Intent.Kind.ToString(),
                    ["requestedDrinkName"] = turnState.Intent.RequestedDrinkName,
                    ["requestedIngredientNames"] = string.Join(", ", turnState.Intent.RequestedIngredientNames),
                    ["matchedRequestDescriptors"] = string.Join(", ", turnState.Intent.RequestDescriptors),
                    ["semanticDrinkSignals"] = turnState.SemanticSearchResult.ByDrinkId.Count.ToString(),
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
