namespace AlCopilot.Recommendation.Features.Recommendation;

public enum RecommendationRequestIntentKind
{
    Recommendation = 0,
    DrinkDetails = 1,
}

public sealed record RecommendationRequestIntent(
    RecommendationRequestIntentKind Kind,
    string? RequestedDrinkName,
    IReadOnlyCollection<string> RequestedIngredientNames,
    IReadOnlyCollection<string> RequestDescriptors,
    bool LooksLikeDrinkDetails = false)
{
    public bool IsDrinkDetailsRequest =>
        LooksLikeDrinkDetails || Kind is RecommendationRequestIntentKind.DrinkDetails;

    public bool HasRequestedIngredients =>
        RequestedIngredientNames.Count > 0;

    public bool HasRequestedDrink =>
        !string.IsNullOrWhiteSpace(RequestedDrinkName);

    public string? RequestedIngredientName =>
        RequestedIngredientNames.FirstOrDefault();
}
