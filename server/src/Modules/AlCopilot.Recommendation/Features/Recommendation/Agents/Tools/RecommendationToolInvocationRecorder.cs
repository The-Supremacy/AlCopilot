using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationToolInvocationRecorder : IRecommendationToolInvocationRecorder
{
    private readonly List<RecommendationToolInvocationDto> invocations = [];
    private readonly object syncRoot = new();

    public void Record(string toolName, string purpose)
    {
        var invocation = new RecommendationToolInvocationDto(
            toolName.Trim(),
            string.IsNullOrWhiteSpace(purpose) ? "Read-only recommendation helper used." : purpose.Trim());

        lock (syncRoot)
        {
            if (!invocations.Contains(invocation))
            {
                invocations.Add(invocation);
            }
        }
    }

    public IReadOnlyCollection<RecommendationToolInvocationDto> Drain()
    {
        lock (syncRoot)
        {
            var drained = invocations.ToList();
            invocations.Clear();
            return drained;
        }
    }
}
