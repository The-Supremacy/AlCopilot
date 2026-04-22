namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

internal interface IRecommendationExecutionTraceRecorder
{
    void Record(RecommendationExecutionTraceStep step);

    IReadOnlyCollection<RecommendationExecutionTraceStep> Drain();
}
