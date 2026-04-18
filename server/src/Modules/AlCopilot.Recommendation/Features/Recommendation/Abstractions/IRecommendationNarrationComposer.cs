using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

public interface IRecommendationNarrationComposer
{
    string BuildContextInstructions(
        string customerMessage,
        AlCopilot.CustomerProfile.Contracts.DTOs.CustomerProfileDto profile,
        IReadOnlyCollection<AlCopilot.Recommendation.Contracts.DTOs.RecommendationGroupDto> recommendationGroups,
        IReadOnlyCollection<AlCopilot.DrinkCatalog.Contracts.DTOs.DrinkDetailDto> catalogSnapshot);
}
