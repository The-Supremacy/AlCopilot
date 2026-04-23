using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;

internal interface IRecommendationRunContextBuilder
{
    RecommendationRunContext Build(
        RecommendationRequestIntent intent,
        CustomerProfileDto profile,
        IReadOnlyCollection<DrinkDetailDto> drinks,
        IReadOnlyCollection<RecommendationGroupDto> groups,
        RecommendationSemanticSearchResult semanticSearchResult);
}
