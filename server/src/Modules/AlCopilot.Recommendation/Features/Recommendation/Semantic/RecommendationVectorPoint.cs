namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed record RecommendationVectorPoint(
    Guid PointId,
    Guid DrinkId,
    string DrinkName,
    RecommendationSemanticFacetKind FacetKind,
    string Text,
    string? MatchedIngredientName,
    ReadOnlyMemory<float> Vector);
