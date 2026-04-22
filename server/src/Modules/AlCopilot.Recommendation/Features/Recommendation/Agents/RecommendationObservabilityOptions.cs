namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

public sealed class RecommendationObservabilityOptions
{
    public const string SectionName = "Recommendation:Observability";

    public bool EnableSensitiveData { get; init; }
}
