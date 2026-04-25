using AlCopilot.Recommendation.Features.Recommendation;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

namespace AlCopilot.Recommendation.UnitTests.Regression;

internal sealed class RecommendationRunContextEvaluationHarness(
    RecommendationRunInputs inputs,
    IRecommendationSemanticSearchService semanticSearchService,
    IRecommendationRequestIntentResolver requestIntentResolver,
    IRecommendationCandidateBuilder candidateBuilder,
    IRecommendationRunContextBuilder runContextBuilder)
{
    public async Task<RecommendationRunContext> CreateAsync(
        string customerMessage,
        CancellationToken cancellationToken = default)
    {
        var analysis = RecommendationRequestQueryAnalyzer.Analyze(customerMessage);
        var semanticSearchResult = RecommendationSemanticSearchPolicy.ShouldUseSemanticSearch(analysis, inputs)
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

        return runContextBuilder.Build(
            intent,
            inputs.Profile,
            inputs.Drinks,
            groups,
            semanticSearchResult);
    }
}
