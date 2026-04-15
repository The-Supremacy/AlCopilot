using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.Queries;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class GetRecommendationSessionsHandler(
    IRecommendationSessionQueryService queryService,
    ICurrentActorAccessor currentActorAccessor) : IRequestHandler<GetRecommendationSessionsQuery, List<RecommendationSessionSummaryDto>>
{
    public async ValueTask<List<RecommendationSessionSummaryDto>> Handle(
        GetRecommendationSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var customerId = RecommendationActorResolver.GetCustomerId(currentActorAccessor);
        return await queryService.GetSessionSummariesAsync(customerId, cancellationToken);
    }
}
