using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationExecutionTraceRecorder : IRecommendationExecutionTraceRecorder
{
    private readonly List<RecommendationExecutionTraceStep> steps = [];
    private readonly object syncRoot = new();

    public void Record(RecommendationExecutionTraceStep step)
    {
        lock (syncRoot)
        {
            steps.Add(step);
        }
    }

    public IReadOnlyCollection<RecommendationExecutionTraceStep> Drain()
    {
        lock (syncRoot)
        {
            var drained = steps.ToList();
            steps.Clear();
            return drained;
        }
    }
}
