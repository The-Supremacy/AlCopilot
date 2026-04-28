using AlCopilot.Recommendation.Contracts.Commands;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class SubmitRecommendationRequestHandler(
    ICurrentActorAccessor currentActorAccessor,
    IRecommendationConversationService conversationService)
    : IRequestHandler<SubmitRecommendationRequestCommand, SubmitRecommendationMessageResultDto>
{
    public async ValueTask<SubmitRecommendationMessageResultDto> Handle(
        SubmitRecommendationRequestCommand request,
        CancellationToken cancellationToken)
    {
        var customerId = RecommendationActorResolver.GetCustomerId(currentActorAccessor);
        return await conversationService.SendMessageAsync(
            customerId,
            request.SessionId,
            request.Message,
            cancellationToken);
    }
}
