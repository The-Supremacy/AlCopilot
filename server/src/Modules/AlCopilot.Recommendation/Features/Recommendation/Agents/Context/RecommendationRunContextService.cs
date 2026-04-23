using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using System.Diagnostics;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationRunContextService(
    IRecommendationRunInputsQueryService runInputsQueryService,
    IRecommendationSemanticSearchService semanticSearchService,
    IRecommendationRequestIntentResolver requestIntentResolver,
    IRecommendationCandidateBuilder candidateBuilder,
    IRecommendationRunContextBuilder runContextBuilder,
    IRecommendationExecutionTraceRecorder executionTraceRecorder) : IRecommendationRunContextService
{
    public async Task<RecommendationRunContext> CreateAsync(
        string customerMessage,
        CancellationToken cancellationToken = default)
    {
        using var activity = RecommendationTelemetry.ActivitySource.StartActivity(
            "recommendation.run_context.build",
            ActivityKind.Internal);
        activity?.SetTag("recommendation.customer_message.length", customerMessage.Length);

        var inputs = await runInputsQueryService.GetRunInputsAsync(cancellationToken);
        var semanticSearchResult = await semanticSearchService.SearchAsync(customerMessage, cancellationToken);
        var intent = await requestIntentResolver.ResolveAsync(customerMessage, inputs, semanticSearchResult, cancellationToken);
        var groups = candidateBuilder.Build(customerMessage, intent, inputs.Profile, inputs.Drinks, semanticSearchResult);
        var runContext = runContextBuilder.Build(intent, inputs.Profile, inputs.Drinks, groups, semanticSearchResult);

        activity?.SetTag("recommendation.intent.kind", intent.Kind.ToString());
        activity?.SetTag("recommendation.groups.count", runContext.Groups.Count);
        activity?.SetTag("recommendation.items.count", runContext.Groups.Sum(group => group.Items.Count));
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
                    ["requestedIngredientName"] = intent.RequestedIngredientName,
                    ["matchedPreferenceSignals"] = string.Join(", ", intent.PreferenceSignals),
                    ["semanticDrinkSignals"] = semanticSearchResult.ByDrinkId.Count.ToString(),
                    ["groupCount"] = runContext.Groups.Count.ToString(),
                    ["itemCount"] = runContext.Groups.Sum(group => group.Items.Count).ToString(),
                },
                []));

        return runContext;
    }
}
