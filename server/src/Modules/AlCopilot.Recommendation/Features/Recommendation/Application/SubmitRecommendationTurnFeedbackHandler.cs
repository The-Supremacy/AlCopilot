using AlCopilot.Recommendation.Contracts.Commands;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class SubmitRecommendationTurnFeedbackHandler(
    ICurrentActorAccessor currentActorAccessor,
    IChatSessionRepository chatSessionRepository,
    IRecommendationUnitOfWork unitOfWork) : IRequestHandler<SubmitRecommendationTurnFeedbackCommand, RecommendationSessionDto>
{
    public async ValueTask<RecommendationSessionDto> Handle(
        SubmitRecommendationTurnFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        var customerId = RecommendationActorResolver.GetCustomerId(currentActorAccessor);
        var session = await chatSessionRepository.GetByCustomerSessionIdAsync(
            customerId,
            request.SessionId,
            cancellationToken);

        if (session is null)
        {
            throw new NotFoundException($"Recommendation session '{request.SessionId}' not found.");
        }

        session.RecordTurnFeedback(request.TurnId, request.Rating, request.Comment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return session.ToDto();
    }
}
