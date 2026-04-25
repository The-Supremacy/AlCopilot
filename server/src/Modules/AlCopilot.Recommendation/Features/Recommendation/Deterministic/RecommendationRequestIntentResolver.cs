using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Shared.Text;
using Microsoft.Extensions.Options;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationRequestIntentResolver(
    IRecommendationCatalogFuzzyLookupService fuzzyLookupService,
    IOptions<RecommendationSemanticOptions> semanticOptions) : IRecommendationRequestIntentResolver
{
    private readonly RecommendationSemanticOptions options = semanticOptions.Value;

    public async Task<RecommendationRequestIntent> ResolveAsync(
        string customerMessage,
        RecommendationRunInputs inputs,
        RecommendationSemanticSearchResult semanticSearchResult,
        CancellationToken cancellationToken = default)
    {
        var analysis = RecommendationRequestQueryAnalyzer.Analyze(customerMessage);
        var entities = await ResolveEntitiesAsync(analysis, inputs, semanticSearchResult, cancellationToken);
        var kind = ResolveKind(
            entities.RequestedDrinkName,
            analysis.LooksLikeDrinkDetails);

        return new RecommendationRequestIntent(
            kind,
            entities.RequestedDrinkName,
            entities.RequestedIngredientNames,
            analysis.RequestDescriptors,
            analysis.LooksLikeDrinkDetails);
    }

    private async Task<RecommendationResolvedRequestEntities> ResolveEntitiesAsync(
        RecommendationRequestQueryAnalysis analysis,
        RecommendationRunInputs inputs,
        RecommendationSemanticSearchResult semanticSearchResult,
        CancellationToken cancellationToken)
    {
        var requestedDrinkName = RecommendationCatalogMatcher.FindMentionedDrinkName(
            inputs.Drinks,
            analysis.NormalizedMessage);
        var requestedIngredientNames = RecommendationCatalogMatcher.FindMentionedIngredientNames(
            inputs.Drinks,
            analysis.NormalizedMessage)
            .ToList();

        if (string.IsNullOrWhiteSpace(requestedDrinkName) && !string.IsNullOrWhiteSpace(analysis.DrinkSearchText))
        {
            var fuzzyDrinkMatches = await fuzzyLookupService.FindDrinkMatchesAsync(
                analysis.DrinkSearchText,
                cancellationToken);
            requestedDrinkName = ResolveClearWinner(fuzzyDrinkMatches);
        }

        // IngredientSearchTexts are raw ingredient-like phrases extracted from the prompt.
        // requestedIngredientNames contains only exact catalog ingredient matches.
        // If we extracted phrases but still have no exact ingredient matches, try fuzzy lookup next.
        if (requestedIngredientNames.Count == 0 && analysis.IngredientSearchTexts.Count > 0)
        {
            foreach (var ingredientSearchText in analysis.IngredientSearchTexts)
            {
                var fuzzyIngredientMatches = await fuzzyLookupService.FindIngredientMatchesAsync(
                    ingredientSearchText,
                    cancellationToken);
                var requestedIngredientName = ResolveClearWinner(fuzzyIngredientMatches);
                if (!string.IsNullOrWhiteSpace(requestedIngredientName))
                {
                    requestedIngredientNames.Add(requestedIngredientName);
                }
            }
        }

        if (string.IsNullOrWhiteSpace(requestedDrinkName) && analysis.LooksLikeDrinkDetails)
        {
            requestedDrinkName = ResolveSemanticDrinkName(semanticSearchResult, options);
        }

        if (requestedIngredientNames.Count == 0 && analysis.IngredientSearchTexts.Count > 0)
        {
            requestedIngredientNames.AddRange(ResolveSemanticIngredientNames(semanticSearchResult, options));
        }

        return new RecommendationResolvedRequestEntities(
            requestedDrinkName,
            requestedIngredientNames
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList());
    }

    private static string? ResolveClearWinner(IReadOnlyCollection<RecommendationFuzzyMatch> matches)
    {
        var ordered = matches
            .OrderByDescending(match => match.Similarity)
            .ToList();
        if (ordered.Count == 0)
        {
            return null;
        }

        var best = ordered[0];
        var runnerUpSimilarity = ordered.Count > 1 ? ordered[1].Similarity : 0d;

        return best.Similarity >= 0.55d && best.Similarity - runnerUpSimilarity >= 0.08d
            ? best.Name
            : null;
    }

    private static string? ResolveSemanticDrinkName(
        RecommendationSemanticSearchResult semanticSearchResult,
        RecommendationSemanticOptions options)
    {
        var topNameMatch = semanticSearchResult.FindTopFacetMatch(
            RecommendationSemanticFacetKind.Name,
            options.NameMatchMinScore,
            options.FacetMatchMinScoreGap);
        if (topNameMatch is null)
        {
            return null;
        }

        return topNameMatch.DrinkName;
    }

    private static IReadOnlyCollection<string> ResolveSemanticIngredientNames(
        RecommendationSemanticSearchResult semanticSearchResult,
        RecommendationSemanticOptions options)
    {
        var topIngredientMatch = semanticSearchResult.FindTopFacetMatch(
            RecommendationSemanticFacetKind.Ingredient,
            options.IngredientMatchMinScore,
            options.FacetMatchMinScoreGap);
        return topIngredientMatch?.MatchedIngredients
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];
    }

    private static RecommendationRequestIntentKind ResolveKind(
        string? requestedDrinkName,
        bool looksLikeDrinkDetails)
    {
        if (looksLikeDrinkDetails || !string.IsNullOrWhiteSpace(requestedDrinkName))
        {
            return RecommendationRequestIntentKind.DrinkDetails;
        }

        return RecommendationRequestIntentKind.Recommendation;
    }
    private sealed record RecommendationResolvedRequestEntities(
        string? RequestedDrinkName,
        IReadOnlyCollection<string> RequestedIngredientNames);
}
