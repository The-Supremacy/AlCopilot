using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed record RecommendationNarrationSnapshot(
    CustomerProfileDto Profile,
    IReadOnlyCollection<RecommendationGroupDto> RecommendationGroups,
    IReadOnlyCollection<DrinkDetailDto> CatalogSnapshot);
