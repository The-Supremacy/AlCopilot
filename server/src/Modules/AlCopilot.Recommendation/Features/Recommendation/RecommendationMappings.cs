using AlCopilot.Recommendation.Contracts.DTOs;

namespace AlCopilot.Recommendation.Features.Recommendation;

internal static class RecommendationMappings
{
    public static RecommendationSessionDto ToDto(
        this ChatSession session,
        IReadOnlyCollection<AgentMessage> agentMessages,
        IReadOnlyDictionary<Guid, List<RecommendationGroupDto>> recommendationGroupsByRunId)
    {
        var visibleMessages = agentMessages
            .Where(IsVisibleMessage)
            .OrderBy(message => message.Sequence)
            .ToList();
        var sequence = 1;

        return new RecommendationSessionDto(
            session.Id,
            session.Title,
            session.CreatedAtUtc,
            session.UpdatedAtUtc,
            visibleMessages
                .Select(message => ToDto(message, sequence++, recommendationGroupsByRunId))
                .ToList());
    }

    private static RecommendationTurnDto ToDto(
        AgentMessage message,
        int sequence,
        IReadOnlyDictionary<Guid, List<RecommendationGroupDto>> recommendationGroupsByRunId)
    {
        var recommendationGroups = message.AgentRunId.HasValue
            && string.Equals(message.Role, "assistant", StringComparison.Ordinal)
            && recommendationGroupsByRunId.TryGetValue(message.AgentRunId.Value, out var groups)
                ? groups
                : [];

        return new RecommendationTurnDto(
            message.Id,
            sequence,
            message.Role,
            message.TextContent ?? string.Empty,
            recommendationGroups,
            GetFeedback(message),
            message.CreatedAtUtc);
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

    internal static bool IsVisibleMessage(AgentMessage message) =>
        message.Role is "user" or "assistant"
        && message.Kind is "text"
        && !string.IsNullOrWhiteSpace(message.TextContent);

    internal static RecommendationTurnFeedbackDto? GetFeedback(AgentMessage message) =>
        string.IsNullOrWhiteSpace(message.FeedbackRating) || message.FeedbackCreatedAtUtc is null
            ? null
            : new RecommendationTurnFeedbackDto(
                message.FeedbackRating,
                message.FeedbackComment,
                message.FeedbackCreatedAtUtc.Value);
}
