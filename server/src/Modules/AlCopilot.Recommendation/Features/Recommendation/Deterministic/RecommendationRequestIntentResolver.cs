using AlCopilot.Recommendation.Features.Recommendation.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationRequestIntentResolver : IRecommendationRequestIntentResolver
{
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
        var normalizedMessage = customerMessage.Trim();
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
