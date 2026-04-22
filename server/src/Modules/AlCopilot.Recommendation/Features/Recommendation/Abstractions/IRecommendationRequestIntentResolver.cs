namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

internal interface IRecommendationRequestIntentResolver
{
    RecommendationRequestIntent Resolve(
        string customerMessage,
        RecommendationRunInputs inputs);
}
