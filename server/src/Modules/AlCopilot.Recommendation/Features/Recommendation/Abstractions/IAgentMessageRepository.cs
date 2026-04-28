namespace AlCopilot.Recommendation.Features.Recommendation.Abstractions;

public interface IAgentMessageRepository
{
    Task<AgentMessage?> GetBySessionMessageIdAsync(
        Guid chatSessionId,
        Guid messageId,
        CancellationToken cancellationToken = default);
}
