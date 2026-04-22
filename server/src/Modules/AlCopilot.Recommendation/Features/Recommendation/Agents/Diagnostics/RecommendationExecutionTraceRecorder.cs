using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationExecutionTraceRecorder : IRecommendationExecutionTraceRecorder
{
    private readonly List<RecommendationExecutionTraceStep> steps = [];

    public void Record(RecommendationExecutionTraceStep step)
    {
        steps.Add(step);
    }

    public IReadOnlyCollection<RecommendationExecutionTraceStep> Drain()
    {
        var drained = steps.ToList();
        steps.Clear();
        return drained;
    }
}
