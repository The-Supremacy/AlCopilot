using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Shared.Text;
using Microsoft.Extensions.Options;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationRequestIntentResolver(
    IRecommendationCatalogFuzzyLookupService fuzzyLookupService,
    IOptions<RecommendationSemanticOptions> semanticOptions) : IRecommendationRequestIntentResolver
{
    private readonly RecommendationSemanticOptions options = semanticOptions.Value;

    private static readonly string[] RecipeLookupSignals =
    [
        "recipe",
        "how do i make",
        "how to make",
        "what's in",
        "what is in",
        "ingredients for",
        "method for",
        "instructions for",
    ];

    private static readonly string[] PreferenceSignals =
    [
        "sweet",
        "sparkling",
        "bubbly",
        "refreshing",
        "citrusy",
        "citrus",
        "fruity",
        "bitter",
        "dry",
        "smoky",
        "spicy",
        "herbal",
        "creamy",
        "tropical",
        "light",
        "boozy",
        "strong",
    ];

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
            entities.RequestedIngredientName,
            analysis.LooksLikeRecipeLookup);

        return new RecommendationRequestIntent(
            kind,
            entities.RequestedDrinkName,
            entities.RequestedIngredientName,
            analysis.PreferenceSignals,
            analysis.LooksLikeRecipeLookup);
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
        var requestedIngredientName = RecommendationCatalogMatcher.FindMentionedIngredientName(
            inputs.Drinks,
            analysis.NormalizedMessage);

        if (string.IsNullOrWhiteSpace(requestedDrinkName) && !string.IsNullOrWhiteSpace(analysis.DrinkSearchText))
        {
            var fuzzyDrinkMatches = await fuzzyLookupService.FindDrinkMatchesAsync(
                analysis.DrinkSearchText,
                cancellationToken);
            requestedDrinkName = ResolveClearWinner(fuzzyDrinkMatches);
        }

        if (string.IsNullOrWhiteSpace(requestedIngredientName) && !string.IsNullOrWhiteSpace(analysis.IngredientSearchText))
        {
            var fuzzyIngredientMatches = await fuzzyLookupService.FindIngredientMatchesAsync(
                analysis.IngredientSearchText,
                cancellationToken);
            requestedIngredientName = ResolveClearWinner(fuzzyIngredientMatches);
        }

        if (string.IsNullOrWhiteSpace(requestedDrinkName) && analysis.LooksLikeRecipeLookup)
        {
            requestedDrinkName = ResolveSemanticDrinkName(semanticSearchResult, options);
        }

        if (string.IsNullOrWhiteSpace(requestedIngredientName) && !string.IsNullOrWhiteSpace(analysis.IngredientSearchText))
        {
            requestedIngredientName = ResolveSemanticIngredientName(semanticSearchResult, options);
        }

        return new RecommendationResolvedRequestEntities(requestedDrinkName, requestedIngredientName);
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

    private static string? ResolveSemanticIngredientName(
        RecommendationSemanticSearchResult semanticSearchResult,
        RecommendationSemanticOptions options)
    {
        var topIngredientMatch = semanticSearchResult.FindTopFacetMatch(
            RecommendationSemanticFacetKind.Ingredient,
            options.IngredientMatchMinScore,
            options.FacetMatchMinScoreGap);
        return topIngredientMatch?.MatchedIngredients.FirstOrDefault();
    }

    private static RecommendationRequestIntentKind ResolveKind(
        string? requestedDrinkName,
        string? requestedIngredientName,
        bool looksLikeRecipeLookup)
    {
        if (looksLikeRecipeLookup && !string.IsNullOrWhiteSpace(requestedIngredientName))
        {
            return RecommendationRequestIntentKind.Hybrid;
        }

        if (looksLikeRecipeLookup || !string.IsNullOrWhiteSpace(requestedDrinkName))
        {
            return RecommendationRequestIntentKind.RecipeLookup;
        }

        if (!string.IsNullOrWhiteSpace(requestedIngredientName))
        {
            return RecommendationRequestIntentKind.IngredientDiscovery;
        }

        return RecommendationRequestIntentKind.Recommendation;
    }

    private static class RecommendationRequestQueryAnalyzer
    {
        internal static RecommendationRequestQueryAnalysis Analyze(string customerMessage)
        {
            var normalizedMessage = customerMessage.TrimOrEmpty();
            var looksLikeRecipeLookup = RecipeLookupSignals.Any(signal =>
                normalizedMessage.Contains(signal, StringComparison.OrdinalIgnoreCase));
            var preferenceSignals = PreferenceSignals
                .Where(signal => normalizedMessage.Contains(signal, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(signal => signal, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var drinkSearchText = RecommendationCatalogMatcher.ExtractDrinkSearchText(
                normalizedMessage,
                looksLikeRecipeLookup);
            var ingredientSearchText = RecommendationCatalogMatcher.ExtractIngredientSearchText(normalizedMessage);

            return new RecommendationRequestQueryAnalysis(
                normalizedMessage,
                looksLikeRecipeLookup,
                preferenceSignals,
                drinkSearchText,
                ingredientSearchText);
        }
    }

    private sealed record RecommendationRequestQueryAnalysis(
        string NormalizedMessage,
        bool LooksLikeRecipeLookup,
        IReadOnlyCollection<string> PreferenceSignals,
        string? DrinkSearchText,
        string? IngredientSearchText);

    private sealed record RecommendationResolvedRequestEntities(
        string? RequestedDrinkName,
        string? RequestedIngredientName);
}
