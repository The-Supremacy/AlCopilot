using AlCopilot.Recommendation.Contracts.Commands;
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
    IAgentMessageRepository agentMessageRepository,
    IRecommendationUnitOfWork unitOfWork) : IRequestHandler<SubmitRecommendationTurnFeedbackCommand>
{
    public async ValueTask<Unit> Handle(
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

        var message = await agentMessageRepository.GetBySessionMessageIdAsync(
                request.SessionId,
                request.TurnId,
                cancellationToken)
            ?? throw new NotFoundException($"Recommendation turn '{request.TurnId}' not found.");

        message.RecordFeedback(request.Rating, request.Comment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
