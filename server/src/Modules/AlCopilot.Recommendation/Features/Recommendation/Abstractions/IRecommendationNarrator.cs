using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

public interface IRecommendationNarrator
{
    Task<RecommendationNarrationResult> NarrateAsync(
        RecommendationNarrationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record RecommendationNarrationRequest(
    ChatSession Session,
    string CustomerMessage,
    string ContextInstructions);

public sealed record RecommendationNarrationResult(
    string Content,
    List<RecommendationToolInvocationDto> ToolInvocations);
