using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationConversationService(
    IChatSessionRepository chatSessionRepository,
    IRecommendationNarrationContextQueryService contextQueryService,
    IRecommendationNarratorAgentFactory agentFactory,
    IRecommendationAgentSessionStore sessionStore,
    IRecommendationUnitOfWork unitOfWork,
    ILogger<RecommendationConversationService> logger) : IRecommendationConversationService
{
    public async Task<RecommendationSessionDto> SendMessageAsync(
        string customerId,
        Guid? sessionId,
        string message,
        CancellationToken cancellationToken = default)
    {
        var request = RecommendationConversationRequest.Create(customerId, sessionId, message);

        try
        {
            return await RunAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Recommendation conversation failed. The customer portal requires a working configured LLM provider.");
            throw;
        }
    }

    private async Task<RecommendationSessionDto> RunAsync(
        RecommendationConversationRequest request,
        CancellationToken cancellationToken)
    {
        var session = await LoadOrCreateSessionAsync(request, cancellationToken);
        var snapshot = await contextQueryService.GetSnapshotAsync(request.Message, cancellationToken);

        var agent = agentFactory.Create();
        var agentSession = await sessionStore.RestoreAsync(
            session.AgentSessionStateJson,
            agent,
            cancellationToken);
        var response = await agent.RunAsync(
            BuildMessages(session, request.Message),
            agentSession,
            options: null,
            cancellationToken);

        var content = string.IsNullOrWhiteSpace(response.Text)
            ? response.Messages.LastOrDefault(message => message.Role == ChatRole.Assistant)?.Text
            : response.Text;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Recommendation LLM returned an empty assistant message.");
        }

        var serializedSession = await sessionStore.SerializeAsync(
            agentSession,
            agent,
            cancellationToken);

        session.UpdateAgentSessionState(serializedSession);
        session.AppendUserTurn(request.Message);
        session.AppendAssistantTurn(content.Trim(), snapshot.RecommendationGroups, []);
        await SaveSessionChangesAsync(session, cancellationToken);

        return session.ToDto();
    }

    private async Task<ChatSession> LoadOrCreateSessionAsync(
        RecommendationConversationRequest request,
        CancellationToken cancellationToken)
    {
        if (request.SessionId.HasValue)
        {
            var existing = await chatSessionRepository.GetByCustomerSessionIdAsync(
                request.CustomerId,
                request.SessionId.Value,
                cancellationToken);

            if (existing is not null)
            {
                return existing;
            }
        }

        var session = ChatSession.Create(request.CustomerId, request.Message);
        chatSessionRepository.Add(session);
        return session;
    }

    private async Task SaveSessionChangesAsync(ChatSession session, CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(
                ex,
                "Recommendation session {SessionId} for customer {CustomerId} could not be saved because the row no longer matched the pending update.",
                session.Id,
                session.CustomerId);
            throw new ConflictException(
                "This recommendation session was changed while your message was being processed. Please retry.");
        }
    }

    private static List<ChatMessage> BuildMessages(ChatSession session, string customerMessage)
    {
        var history = session.Turns
            .OrderBy(turn => turn.Sequence)
            .Select(ToChatMessage)
            .ToList();

        history.Add(new ChatMessage(ChatRole.User, customerMessage));
        return history;
    }

    private static ChatMessage ToChatMessage(ChatTurn turn)
    {
        return new ChatMessage(
            string.Equals(turn.Role, "assistant", StringComparison.Ordinal) ? ChatRole.Assistant : ChatRole.User,
            turn.Content);
    }

    private sealed record RecommendationConversationRequest(
        string CustomerId,
        Guid? SessionId,
        string Message)
    {
        internal static RecommendationConversationRequest Create(string customerId, Guid? sessionId, string message)
        {
            var normalized = message.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new AlCopilot.Shared.Errors.ValidationException("Recommendation message is required.");
            }

            return new RecommendationConversationRequest(customerId, sessionId, normalized);
        }
    }
}
