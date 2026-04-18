using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Contracts.Queries;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class GetRecommendationSessionHandler(
    IRecommendationSessionQueryService queryService,
    ICurrentActorAccessor currentActorAccessor) : IRequestHandler<GetRecommendationSessionQuery, RecommendationSessionDto?>
{
    public async ValueTask<RecommendationSessionDto?> Handle(
        GetRecommendationSessionQuery request,
        CancellationToken cancellationToken)
    {
        var customerId = RecommendationActorResolver.GetCustomerId(currentActorAccessor);
        return await queryService.GetSessionAsync(customerId, request.SessionId, cancellationToken);
    }
}
