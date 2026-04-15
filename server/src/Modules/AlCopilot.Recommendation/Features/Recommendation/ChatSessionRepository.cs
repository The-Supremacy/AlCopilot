using AlCopilot.Recommendation.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class ChatSessionRepository(RecommendationDbContext dbContext) : IChatSessionRepository
{
    public async Task<ChatSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await QueryAggregate()
            .FirstOrDefaultAsync(session => session.Id == id, cancellationToken);
    }

    public async Task<ChatSession?> GetByCustomerSessionIdAsync(
        string customerId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await QueryAggregate()
            .FirstOrDefaultAsync(
                session => session.Id == sessionId && session.CustomerId == customerId,
                cancellationToken);
    }

    public void Add(ChatSession aggregate) => dbContext.ChatSessions.Add(aggregate);

    public void Remove(ChatSession aggregate) => dbContext.ChatSessions.Remove(aggregate);

    private IQueryable<ChatSession> QueryAggregate()
    {
        return dbContext.ChatSessions.Include(session => session.Turns);
    }
}
