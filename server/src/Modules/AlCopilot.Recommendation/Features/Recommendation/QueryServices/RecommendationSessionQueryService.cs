using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
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
            .FirstOrDefaultAsync(
                item => item.Id == sessionId && item.CustomerId == customerId,
                cancellationToken);

        if (session is null)
        {
            return null;
        }

        var agentMessages = await LoadAgentMessagesAsync([session.Id], cancellationToken);
        var recommendationGroups = await LoadRecommendationGroupsAsync(agentMessages, cancellationToken);
        return session.ToDto(agentMessages, recommendationGroups);
    }

    public async Task<List<RecommendationSessionSummaryDto>> GetSessionSummariesAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await dbContext.ChatSessions
            .AsNoTracking()
            .Where(session => session.CustomerId == customerId)
            .OrderByDescending(session => session.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        var sessionIds = sessions.Select(session => session.Id).ToList();
        var agentMessages = await LoadAgentMessagesAsync(sessionIds, cancellationToken);
        var recommendationGroups = await LoadRecommendationGroupsAsync(agentMessages, cancellationToken);

        return sessions
            .Select(session => session.ToDto(
                agentMessages.Where(message => message.ChatSessionId == session.Id).ToList(),
                recommendationGroups).ToSummaryDto())
            .ToList();
    }

    private async Task<List<AgentMessage>> LoadAgentMessagesAsync(
        IReadOnlyCollection<Guid> sessionIds,
        CancellationToken cancellationToken)
    {
        return await dbContext.AgentMessages
            .AsNoTracking()
            .Where(message => sessionIds.Contains(message.ChatSessionId))
            .OrderBy(message => message.Sequence)
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<Guid, List<RecommendationGroupDto>>> LoadRecommendationGroupsAsync(
        IReadOnlyCollection<AgentMessage> agentMessages,
        CancellationToken cancellationToken)
    {
        var agentRunIds = agentMessages
            .Where(message => message.AgentRunId.HasValue)
            .Select(message => message.AgentRunId!.Value)
            .Distinct()
            .ToList();

        var groups = await dbContext.RecommendationTurnGroups
            .AsNoTracking()
            .Include(group => group.Items)
                .ThenInclude(item => item.MissingIngredients)
            .Include(group => group.Items)
                .ThenInclude(item => item.MatchedSignals)
            .Include(group => group.Items)
                .ThenInclude(item => item.RecipeEntries)
            .Where(group => agentRunIds.Contains(group.AgentRunId))
            .OrderBy(group => group.Sequence)
            .ToListAsync(cancellationToken);

        return groups
            .GroupBy(group => group.AgentRunId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(item => item.Sequence).Select(item => item.ToDto()).ToList());
    }
}
