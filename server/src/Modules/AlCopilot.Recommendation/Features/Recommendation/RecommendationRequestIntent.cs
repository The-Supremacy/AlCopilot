namespace AlCopilot.Recommendation.Features.Recommendation;

public enum RecommendationRequestIntentKind
{
    Recommendation = 0,
    IngredientDiscovery = 1,
    RecipeLookup = 2,
    Hybrid = 3,
}

public sealed record RecommendationRequestIntent(
    RecommendationRequestIntentKind Kind,
    string? RequestedDrinkName,
    string? RequestedIngredientName,
    IReadOnlyCollection<string> PreferenceSignals,
    bool LooksLikeRecipeLookup = false)
{
    public bool IsRecipeLookupRequest =>
        LooksLikeRecipeLookup || Kind is RecommendationRequestIntentKind.RecipeLookup or RecommendationRequestIntentKind.Hybrid;

    public bool IsIngredientLed =>
        Kind is RecommendationRequestIntentKind.IngredientDiscovery or RecommendationRequestIntentKind.Hybrid
        && !string.IsNullOrWhiteSpace(RequestedIngredientName);

    public bool IsRecipeLookup =>
        Kind is RecommendationRequestIntentKind.RecipeLookup or RecommendationRequestIntentKind.Hybrid
        && !string.IsNullOrWhiteSpace(RequestedDrinkName);
}
