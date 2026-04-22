using System.Diagnostics;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal static class RecommendationTelemetry
{
    internal const string SourceName = "AlCopilot.Recommendation";

    internal static readonly ActivitySource ActivitySource = new(SourceName);
}
