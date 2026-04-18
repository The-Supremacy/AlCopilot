using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

public interface IRecommendationCandidateBuilder
{
    List<RecommendationGroupDto> Build(
        string customerRequest,
        CustomerProfileDto profile,
        IReadOnlyCollection<DrinkDetailDto> drinks);
}
