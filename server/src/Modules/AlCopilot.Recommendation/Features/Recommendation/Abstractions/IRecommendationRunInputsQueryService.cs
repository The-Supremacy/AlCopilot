using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

public interface IRecommendationRunInputsQueryService
{
    Task<RecommendationRunInputs> GetRunInputsAsync(CancellationToken cancellationToken = default);
}

public sealed record RecommendationRunInputs(
    CustomerProfileDto Profile,
    IReadOnlyCollection<DrinkDetailDto> Drinks);
