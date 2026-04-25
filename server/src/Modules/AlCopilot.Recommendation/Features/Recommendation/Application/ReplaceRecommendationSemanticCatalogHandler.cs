using AlCopilot.Recommendation.Contracts.Commands;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Mediator;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class ReplaceRecommendationSemanticCatalogHandler(
    IRecommendationSemanticIndexingService indexingService)
    : IRequestHandler<ReplaceRecommendationSemanticCatalogCommand, RecommendationSemanticCatalogIndexResultDto>
{
    public async ValueTask<RecommendationSemanticCatalogIndexResultDto> Handle(
        ReplaceRecommendationSemanticCatalogCommand request,
        CancellationToken cancellationToken)
    {
        var result = await indexingService.ReplaceCatalogAsync(request.Drinks, cancellationToken);
        return new RecommendationSemanticCatalogIndexResultDto(result.DrinkCount, result.PointCount);
    }
}
