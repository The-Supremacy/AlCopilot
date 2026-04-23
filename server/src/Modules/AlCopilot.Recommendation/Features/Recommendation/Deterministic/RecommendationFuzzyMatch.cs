namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed record RecommendationFuzzyMatch(
    Guid Id,
    string Name,
    double Similarity);
