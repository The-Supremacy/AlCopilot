using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class RecommendationSessionQueryService(RecommendationDbContext dbContext) : IRecommendationSessionQueryService
{
    public async Task<RecommendationSessionDto?> GetSessionAsync(
        string customerId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await dbContext.ChatSessions
            .AsNoTracking()
            .Include(item => item.Turns)
            .FirstOrDefaultAsync(
                item => item.Id == sessionId && item.CustomerId == customerId,
                cancellationToken);

        return session?.ToDto();
    }

    public async Task<List<RecommendationSessionSummaryDto>> GetSessionSummariesAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await dbContext.ChatSessions
            .AsNoTracking()
            .Include(session => session.Turns)
            .Where(session => session.CustomerId == customerId)
            .OrderByDescending(session => session.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return sessions
            .Select(session => session.ToDto().ToSummaryDto())
            .ToList();
    }
}
