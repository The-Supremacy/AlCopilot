using AlCopilot.Recommendation.Data;
using AlCopilot.Recommendation.Features.Recommendation.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal sealed class AgentMessageRepository(RecommendationDbContext dbContext) : IAgentMessageRepository
{
    public async Task<AgentMessage?> GetBySessionMessageIdAsync(
        Guid chatSessionId,
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.AgentMessages
            .FirstOrDefaultAsync(
                message => message.Id == messageId && message.ChatSessionId == chatSessionId,
                cancellationToken);
    }
}
