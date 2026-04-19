using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
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
    CustomerProfileDto Profile,
    IReadOnlyCollection<RecommendationGroupDto> RecommendationGroups,
    IReadOnlyCollection<DrinkDetailDto> CatalogSnapshot);

internal sealed record RecommendationNarrationContext(
    string ProfileSummary,
    string CandidateSummary);

public sealed record RecommendationNarrationResult(
    string Content,
    List<RecommendationToolInvocationDto> ToolInvocations,
    string SerializedAgentSessionState);
