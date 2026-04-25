using System.Text.Json;
using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal static class RecommendationMappings
{
    public static RecommendationSessionDto ToDto(this ChatSession session)
    {
        return new RecommendationSessionDto(
            session.Id,
            session.Title,
            session.CreatedAtUtc,
            session.UpdatedAtUtc,
            session.Turns
                .OrderBy(turn => turn.Sequence)
                .Select(ToDto)
                .ToList());
    }

    public static RecommendationTurnDto ToDto(this ChatTurn turn)
    {
        return new RecommendationTurnDto(
            turn.Id,
            turn.Sequence,
            turn.Role,
            turn.Content,
            turn.GetRecommendationGroups(),
            turn.GetToolInvocations(),
            turn.GetFeedback(),
            turn.CreatedAtUtc);
    }

    public static RecommendationSessionSummaryDto ToSummaryDto(this RecommendationSessionDto session)
    {
        var lastAssistantMessage = session.Turns
            .Where(turn => string.Equals(turn.Role, "assistant", StringComparison.Ordinal))
            .OrderByDescending(turn => turn.Sequence)
            .Select(turn => turn.Content)
            .FirstOrDefault() ?? string.Empty;

        return new RecommendationSessionSummaryDto(
            session.SessionId,
            session.Title,
            session.CreatedAtUtc,
            session.UpdatedAtUtc,
            lastAssistantMessage);
    }
}
