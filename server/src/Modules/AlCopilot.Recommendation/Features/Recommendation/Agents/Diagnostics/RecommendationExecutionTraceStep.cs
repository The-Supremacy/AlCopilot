namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

public sealed record RecommendationExecutionTraceStep(
    string StepName,
    string Outcome,
    string? Summary,
    DateTimeOffset RecordedAtUtc,
    IReadOnlyDictionary<string, string?> Attributes,
    IReadOnlyCollection<string> Details);
