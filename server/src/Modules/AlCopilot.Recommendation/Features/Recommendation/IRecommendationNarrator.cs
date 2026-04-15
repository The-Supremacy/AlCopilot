using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation;

public interface IRecommendationNarrator
{
    Task<RecommendationNarrationResult> GenerateAsync(
        RecommendationNarrationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record RecommendationNarrationRequest(
    ChatSession Session,
    string CustomerMessage,
    CustomerProfileDto Profile,
    IReadOnlyCollection<RecommendationGroupDto> RecommendationGroups,
    IReadOnlyCollection<DrinkDetailDto> CatalogSnapshot);

public sealed record RecommendationNarrationResult(
    string Content,
    List<RecommendationToolInvocationDto> ToolInvocations);
