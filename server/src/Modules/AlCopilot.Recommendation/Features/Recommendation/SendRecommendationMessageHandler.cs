using AlCopilot.CustomerProfile.Contracts.DTOs;
using AlCopilot.CustomerProfile.Contracts.Queries;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.Recommendation.Contracts.Commands;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class SubmitRecommendationRequestHandler(
    IChatSessionRepository chatSessionRepository,
    IUnitOfWork unitOfWork,
    ICurrentActorAccessor currentActorAccessor,
    IMediator mediator,
    IRecommendationCandidateBuilder candidateBuilder,
    IRecommendationNarrator narrator) : IRequestHandler<SubmitRecommendationRequestCommand, RecommendationSessionDto>
{
    public async ValueTask<RecommendationSessionDto> Handle(
        SubmitRecommendationRequestCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedMessage = NormalizeMessage(request.Message);
        var customerId = RecommendationActorResolver.GetCustomerId(currentActorAccessor);
        var session = await LoadOrCreateSessionAsync(customerId, request.SessionId, normalizedMessage, cancellationToken);
        session.AppendUserTurn(normalizedMessage);

        var profile = await mediator.Send(new GetCustomerProfileQuery(), cancellationToken);
        var drinks = await mediator.Send(new GetRecommendationCatalogQuery(), cancellationToken);
        var groups = candidateBuilder.Build(normalizedMessage, profile, drinks);
        var narration = await narrator.GenerateAsync(
            new RecommendationNarrationRequest(session, normalizedMessage, profile, groups, drinks),
            cancellationToken);

        session.AppendAssistantTurn(narration.Content, groups, narration.ToolInvocations);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return session.ToDto();
    }

    private async Task<ChatSession> LoadOrCreateSessionAsync(
        string customerId,
        Guid? sessionId,
        string normalizedMessage,
        CancellationToken cancellationToken)
    {
        if (sessionId.HasValue)
        {
            var existing = await chatSessionRepository.GetByCustomerSessionIdAsync(
                customerId,
                sessionId.Value,
                cancellationToken);

            if (existing is not null)
            {
                return existing;
            }
        }

        var session = ChatSession.Create(customerId, normalizedMessage);
        chatSessionRepository.Add(session);
        return session;
    }

    private static string NormalizeMessage(string message)
    {
        var normalized = message.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ValidationException("Recommendation message is required.");
        }

        return normalized;
    }
}
