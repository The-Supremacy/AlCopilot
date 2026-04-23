using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed record RecommendationRunInputs(
    CustomerProfileDto Profile,
    IReadOnlyCollection<DrinkDetailDto> Drinks);
