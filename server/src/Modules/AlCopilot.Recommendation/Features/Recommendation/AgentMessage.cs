namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class AgentMessage
{
    public Guid Id { get; private set; }
    public Guid ChatSessionId { get; private set; }
    public Guid? AgentRunId { get; private set; }
    public int Sequence { get; private set; }
    public string NativeMessageId { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public string Kind { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public string? TextContent { get; private set; }
    public string RawMessageJson { get; private set; } = string.Empty;
    public string? FeedbackRating { get; private set; }
    public string? FeedbackComment { get; private set; }
    public DateTimeOffset? FeedbackCreatedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private AgentMessage()
    {
    }

    public static AgentMessage Create(
        Guid chatSessionId,
        Guid? agentRunId,
        int sequence,
        string nativeMessageId,
        string role,
        string kind,
        string source,
        string? textContent,
        string rawMessageJson)
    {
        return new AgentMessage
        {
            Id = Guid.TryParse(nativeMessageId, out var messageId) ? messageId : Guid.NewGuid(),
            ChatSessionId = chatSessionId,
            AgentRunId = agentRunId,
            Sequence = sequence,
            NativeMessageId = nativeMessageId,
            Role = role,
            Kind = kind,
            Source = source,
            TextContent = string.IsNullOrWhiteSpace(textContent) ? null : textContent,
            RawMessageJson = rawMessageJson,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public void RecordFeedback(string rating, string? comment)
    {
        if (!string.Equals(Role, "assistant", StringComparison.Ordinal))
        {
            throw new AlCopilot.Shared.Errors.ValidationException("Feedback can only be recorded for assistant turns.");
        }

        var normalizedRating = rating.Trim().ToLowerInvariant();
        if (normalizedRating is not "positive" and not "negative")
        {
            throw new AlCopilot.Shared.Errors.ValidationException(
                "Recommendation feedback rating must be 'positive' or 'negative'.");
        }

        var normalizedComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (normalizedComment?.Length > 1000)
        {
            throw new AlCopilot.Shared.Errors.ValidationException(
                "Recommendation feedback comment must be 1000 characters or fewer.");
        }

        FeedbackRating = normalizedRating;
        FeedbackComment = normalizedComment;
        FeedbackCreatedAtUtc = DateTimeOffset.UtcNow;
    }
}
