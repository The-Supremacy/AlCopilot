using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

internal interface IRecommendationToolInvocationRecorder
{
    void Record(string toolName, string purpose);

    IReadOnlyCollection<RecommendationToolInvocationDto> Drain();
}
