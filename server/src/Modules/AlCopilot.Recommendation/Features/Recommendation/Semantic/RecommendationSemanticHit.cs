namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed record RecommendationSemanticHit(
    Guid PointId,
    Guid DrinkId,
    string DrinkName,
    RecommendationSemanticFacetKind FacetKind,
    string Text,
    string? MatchedIngredientName,
    double Score);
