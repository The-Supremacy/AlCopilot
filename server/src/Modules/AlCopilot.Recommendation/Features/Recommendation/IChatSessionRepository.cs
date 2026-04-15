using AlCopilot.Shared.Data;

namespace AlCopilot.Recommendation.Features.Recommendation;

public interface IChatSessionRepository : IRepository<ChatSession, Guid>
{
    Task<ChatSession?> GetByCustomerSessionIdAsync(
        string customerId,
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
