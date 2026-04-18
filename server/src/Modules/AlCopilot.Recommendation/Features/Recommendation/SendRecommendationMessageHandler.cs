using AlCopilot.Recommendation.Contracts.Commands;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class SubmitRecommendationRequestHandler(
    ICurrentActorAccessor currentActorAccessor,
    IRecommendationWorkflow workflow) : IRequestHandler<SubmitRecommendationRequestCommand, RecommendationSessionDto>
{
    public async ValueTask<RecommendationSessionDto> Handle(
        SubmitRecommendationRequestCommand request,
        CancellationToken cancellationToken)
    {
        var customerId = RecommendationActorResolver.GetCustomerId(currentActorAccessor);
        return await workflow.ExecuteAsync(
            customerId,
            request.SessionId,
            request.Message,
            cancellationToken);
    }
}
