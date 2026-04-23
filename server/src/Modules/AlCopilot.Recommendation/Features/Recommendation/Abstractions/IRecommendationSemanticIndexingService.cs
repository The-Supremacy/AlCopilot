using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

internal interface IRecommendationSemanticIndexingService
{
    Task<RecommendationSemanticCatalogIndexResult> ReplaceCatalogAsync(
        IReadOnlyCollection<DrinkDetailDto> drinks,
        CancellationToken cancellationToken = default);
}
