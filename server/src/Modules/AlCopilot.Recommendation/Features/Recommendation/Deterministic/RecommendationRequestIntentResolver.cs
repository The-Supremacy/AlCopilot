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

    public RecommendationRequestIntent Resolve(string customerMessage, RecommendationRunInputs inputs)
    {
        var normalizedMessage = customerMessage.TrimOrEmpty();
        var requestedDrinkName = RecommendationCatalogMatcher.FindMentionedDrinkName(inputs.Drinks, normalizedMessage);
        var requestedIngredientName = RecommendationCatalogMatcher.FindMentionedIngredientName(inputs.Drinks, normalizedMessage);
        var matchedPreferenceSignals = PreferenceSignals
            .Where(signal => normalizedMessage.Contains(signal, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(signal => signal, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var looksLikeRecipeLookup = RecipeLookupSignals.Any(signal =>
            normalizedMessage.Contains(signal, StringComparison.OrdinalIgnoreCase));

        var kind = ResolveKind(requestedDrinkName, requestedIngredientName, looksLikeRecipeLookup);
        return new RecommendationRequestIntent(
            kind,
            requestedDrinkName,
            requestedIngredientName,
            matchedPreferenceSignals);
    }

    public async Task<RecommendationRequestIntent> ResolveAsync(
        string customerMessage,
        RecommendationRunInputs inputs,
        RecommendationSemanticSearchResult semanticSearchResult,
        CancellationToken cancellationToken = default)
    {
        var normalizedMessage = customerMessage.TrimOrEmpty();
        var looksLikeRecipeLookup = RecipeLookupSignals.Any(signal =>
            normalizedMessage.Contains(signal, StringComparison.OrdinalIgnoreCase));

        var requestedDrinkName = RecommendationCatalogMatcher.FindMentionedDrinkName(inputs.Drinks, normalizedMessage);
        var requestedIngredientName = RecommendationCatalogMatcher.FindMentionedIngredientName(inputs.Drinks, normalizedMessage);
        var fuzzyDrinkQuery = string.IsNullOrWhiteSpace(requestedDrinkName)
            ? RecommendationCatalogMatcher.ExtractDrinkSearchText(normalizedMessage, looksLikeRecipeLookup)
            : null;
        var fuzzyIngredientQuery = string.IsNullOrWhiteSpace(requestedIngredientName)
            ? RecommendationCatalogMatcher.ExtractIngredientSearchText(normalizedMessage)
            : null;

        if (string.IsNullOrWhiteSpace(requestedDrinkName))
        {
            if (!string.IsNullOrWhiteSpace(fuzzyDrinkQuery))
            {
                var fuzzyDrinkMatches = await fuzzyLookupService.FindDrinkMatchesAsync(fuzzyDrinkQuery, cancellationToken);
                requestedDrinkName = ResolveClearWinner(fuzzyDrinkMatches);
            }
        }

        if (string.IsNullOrWhiteSpace(requestedIngredientName))
        {
            if (!string.IsNullOrWhiteSpace(fuzzyIngredientQuery))
            {
                var fuzzyIngredientMatches = await fuzzyLookupService.FindIngredientMatchesAsync(fuzzyIngredientQuery, cancellationToken);
                requestedIngredientName = ResolveClearWinner(fuzzyIngredientMatches);
            }
        }

        if (string.IsNullOrWhiteSpace(requestedDrinkName) && looksLikeRecipeLookup)
        {
            requestedDrinkName = ResolveSemanticDrinkName(semanticSearchResult, options);
        }

        if (string.IsNullOrWhiteSpace(requestedIngredientName) && !string.IsNullOrWhiteSpace(fuzzyIngredientQuery))
        {
            requestedIngredientName = ResolveSemanticIngredientName(semanticSearchResult, options);
        }
        var matchedPreferenceSignals = PreferenceSignals
            .Where(signal => normalizedMessage.Contains(signal, StringComparison.OrdinalIgnoreCase))
            .Concat(ExtractSemanticPreferenceSignals(semanticSearchResult))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(signal => signal, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var kind = ResolveKind(requestedDrinkName, requestedIngredientName, looksLikeRecipeLookup);
        return new RecommendationRequestIntent(kind, requestedDrinkName, requestedIngredientName, matchedPreferenceSignals);
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

    private static IEnumerable<string> ExtractSemanticPreferenceSignals(
        RecommendationSemanticSearchResult semanticSearchResult)
    {
        return semanticSearchResult.ByDrinkId.Values
            .SelectMany(signal => signal.MatchedDescriptors)
            .SelectMany(descriptor => PreferenceSignals.Where(signal =>
                descriptor.Contains(signal, StringComparison.OrdinalIgnoreCase)));
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
}
