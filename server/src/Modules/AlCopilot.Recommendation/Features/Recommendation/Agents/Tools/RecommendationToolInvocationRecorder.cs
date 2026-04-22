using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationToolInvocationRecorder : IRecommendationToolInvocationRecorder
{
    private readonly List<RecommendationToolInvocationDto> invocations = [];

    public void Record(string toolName, string purpose)
    {
        var invocation = new RecommendationToolInvocationDto(
            toolName.Trim(),
            string.IsNullOrWhiteSpace(purpose) ? "Read-only recommendation helper used." : purpose.Trim());

        if (!invocations.Contains(invocation))
        {
            invocations.Add(invocation);
        }
    }

    public IReadOnlyCollection<RecommendationToolInvocationDto> Drain()
    {
        var drained = invocations.ToList();
        invocations.Clear();
        return drained;
    }
}
