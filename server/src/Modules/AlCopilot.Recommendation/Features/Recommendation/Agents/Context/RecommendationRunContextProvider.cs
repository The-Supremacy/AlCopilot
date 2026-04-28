using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationRunContextProvider(
    IRecommendationRunInputsQueryService runInputsQueryService,
    IRecommendationSemanticSearchService semanticSearchService,
    IRecommendationRequestIntentResolver requestIntentResolver,
    IRecommendationCandidateBuilder candidateBuilder,
    IRecommendationRunContextBuilder runContextBuilder,
    IRecommendationExecutionTraceRecorder executionTraceRecorder,
    Action<RecommendationRunContext> captureRunContext) : AIContextProvider
{
    protected override async ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var customerMessage = GetCustomerMessage(context);
        if (string.IsNullOrWhiteSpace(customerMessage))
        {
            return new AIContext();
        }

        var inputs = await runInputsQueryService.GetRunInputsAsync(cancellationToken);
        var requestAnalysis = RecommendationRequestQueryAnalyzer.Analyze(customerMessage);
        var semanticSearchResult = RecommendationSemanticSearchPolicy.ShouldUseSemanticSearch(
                requestAnalysis,
                inputs)
            ? await semanticSearchService.SearchAsync(customerMessage, cancellationToken)
            : RecommendationSemanticSearchResult.Empty;
        var intent = await requestIntentResolver.ResolveAsync(
            customerMessage,
            inputs,
            semanticSearchResult,
            cancellationToken);
        var groups = candidateBuilder.Build(
            customerMessage,
            intent,
            inputs.Profile,
            inputs.Drinks,
            semanticSearchResult);
        var runContext = runContextBuilder.Build(
            intent,
            inputs.Profile,
            inputs.Drinks,
            groups,
            semanticSearchResult);

        captureRunContext(runContext);
        RecordTrace(intent, semanticSearchResult, runContext);

        return new AIContext
        {
            Messages =
            [
                new ChatMessage(
                    ChatRole.System,
                    RecommendationRunContextMessageBuilder.Build(runContext)),
            ],
        };
    }

    private static string? GetCustomerMessage(InvokingContext context) =>
        context.AIContext.Messages?
            .Where(message => message.Role == ChatRole.User)
            .Select(message => message.Text)
            .LastOrDefault(text => !string.IsNullOrWhiteSpace(text));

    private void RecordTrace(
        RecommendationRequestIntent intent,
        RecommendationSemanticSearchResult semanticSearchResult,
        RecommendationRunContext runContext)
    {
        executionTraceRecorder.Record(
            new RecommendationExecutionTraceStep(
                "run_context.build",
                "ok",
                $"Built {runContext.Groups.Count} recommendation group(s) for {intent.Kind}.",
                DateTimeOffset.UtcNow,
                new Dictionary<string, string?>
                {
                    ["intentKind"] = intent.Kind.ToString(),
                    ["requestedDrinkName"] = intent.RequestedDrinkName,
                    ["requestedIngredientNames"] = string.Join(", ", intent.RequestedIngredientNames),
                    ["matchedRequestDescriptors"] = string.Join(", ", intent.RequestDescriptors),
                    ["semanticDrinkSignals"] = semanticSearchResult.ByDrinkId.Count.ToString(),
                    ["groupCount"] = runContext.Groups.Count.ToString(),
                    ["itemCount"] = runContext.Groups.Sum(group => group.Items.Count).ToString(),
                },
                []));
    }
}
