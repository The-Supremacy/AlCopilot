namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed record RecommendationSemanticProjectionPoint(
    Guid PointId,
    Guid DrinkId,
    string DrinkName,
    string? Category,
    RecommendationSemanticFacetKind FacetKind,
    string Text,
    string? MatchedIngredientName);
