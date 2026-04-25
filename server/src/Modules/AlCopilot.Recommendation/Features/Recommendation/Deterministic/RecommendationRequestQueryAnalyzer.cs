using AlCopilot.Shared.Text;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal static class RecommendationRequestQueryAnalyzer
{
    private static readonly string[] DrinkDetailsSignals =
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

    private static readonly string[] RequestDescriptors =
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

    internal static RecommendationRequestQueryAnalysis Analyze(string customerMessage)
    {
        var normalizedMessage = customerMessage.TrimOrEmpty();
        var looksLikeDrinkDetails = DrinkDetailsSignals.Any(signal =>
            normalizedMessage.Contains(signal, StringComparison.OrdinalIgnoreCase));
        var requestDescriptors = RequestDescriptors
            .Where(signal => normalizedMessage.Contains(signal, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(signal => signal, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var drinkSearchText = RecommendationCatalogMatcher.ExtractDrinkSearchText(
            normalizedMessage,
            looksLikeDrinkDetails);
        var ingredientSearchTexts = RecommendationCatalogMatcher.ExtractIngredientSearchTexts(normalizedMessage);

        return new RecommendationRequestQueryAnalysis(
            normalizedMessage,
            looksLikeDrinkDetails,
            requestDescriptors,
            drinkSearchText,
            ingredientSearchTexts);
    }
}

internal sealed record RecommendationRequestQueryAnalysis(
    string NormalizedMessage,
    bool LooksLikeDrinkDetails,
    IReadOnlyCollection<string> RequestDescriptors,
    string? DrinkSearchText,
    IReadOnlyCollection<string> IngredientSearchTexts);
