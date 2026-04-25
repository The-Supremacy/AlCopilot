using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using AlCopilot.Recommendation.Features.Recommendation.Agents.Abstractions;
using AlCopilot.Shared.Errors;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlCopilot.Recommendation.Features.Recommendation.Agents;

internal sealed class RecommendationConversationService(
    IChatSessionRepository chatSessionRepository,
    IRecommendationNarratorAgentFactory agentFactory,
    IRecommendationAgentSessionStore sessionStore,
    IRecommendationExecutionTraceRecorder executionTraceRecorder,
    IRecommendationToolInvocationRecorder toolInvocationRecorder,
    IRecommendationUnitOfWork unitOfWork,
    IHostEnvironment hostEnvironment,
    IOptions<RecommendationObservabilityOptions> observabilityOptions,
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
        var turnState = new RecommendationAgentTurnState();
        var agent = agentFactory.Create(session, turnState);
        var agentSession = await sessionStore.RestoreAsync(
            session.AgentSessionStateJson,
            agent,
            cancellationToken);
        var response = await agent.RunAsync(
            [new ChatMessage(ChatRole.User, request.Message)],
            agentSession,
            options: null,
            cancellationToken);
        executionTraceRecorder.Record(BuildAgentRunTraceStep(response));

        var content = string.IsNullOrWhiteSpace(response.Text)
            ? response.Messages.LastOrDefault(message => message.Role == ChatRole.Assistant)?.Text
            : response.Text;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Recommendation LLM returned an empty assistant message.");
        }

        var toolInvocations = toolInvocationRecorder.Drain();
        var executionTrace = ShouldPersistExecutionTrace()
            ? executionTraceRecorder.Drain()
            : null;
        var serializedSession = await sessionStore.SerializeAsync(
            agentSession,
            agent,
            cancellationToken);

        session.UpdateAgentSessionState(serializedSession);
        session.UpdateLatestAssistantTurnArtifacts(
            turnState.RunContext?.RecommendationGroups ?? [],
            toolInvocations,
            executionTrace);

        await SaveSessionChangesAsync(session, cancellationToken);

        return session.ToDto();
    }

    private static RecommendationExecutionTraceStep BuildAgentRunTraceStep(AgentResponse response)
    {
        var reasoning = string.Join(
            "\n\n",
            response.Messages
                .SelectMany(message => message.Contents)
                .OfType<TextReasoningContent>()
                .Select(content => content.Text)
                .Where(text => !string.IsNullOrWhiteSpace(text)));
        var usage = response.Usage;
        var finishReason = response.FinishReason?.ToString();

        return new RecommendationExecutionTraceStep(
            "agent.run",
            string.IsNullOrWhiteSpace(finishReason) ? "completed" : finishReason,
            "Recommendation narrator agent produced an assistant response.",
            DateTimeOffset.UtcNow,
            new Dictionary<string, string?>
            {
                ["finishReason"] = finishReason,
                ["inputTokens"] = usage?.InputTokenCount?.ToString(),
                ["outputTokens"] = usage?.OutputTokenCount?.ToString(),
                ["reasoningTokens"] = usage?.ReasoningTokenCount?.ToString(),
                ["messageCount"] = response.Messages.Count.ToString(),
            },
            [],
            string.IsNullOrWhiteSpace(reasoning) ? null : reasoning);
    }

    private bool ShouldPersistExecutionTrace()
    {
        return hostEnvironment.IsDevelopment()
            && observabilityOptions.Value.PersistExecutionTraceInDevelopment;
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
