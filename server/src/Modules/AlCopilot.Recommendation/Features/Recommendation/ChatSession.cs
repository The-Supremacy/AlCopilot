using AlCopilot.Recommendation.Contracts.Events;
using AlCopilot.Shared.Domain;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class ChatSession : AggregateRoot<Guid>
{
    public string CustomerId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? AgentSessionStateJson { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private ChatSession()
    {
    }

    public static ChatSession Create(string customerId, string firstMessage)
    {
        var now = DateTimeOffset.UtcNow;
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Title = BuildTitle(firstMessage),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        session.Raise(new RecommendationSessionStartedEvent(session.Id, customerId));
        return session;
    }

    public void UpdateAgentSessionState(string serializedState)
    {
        if (string.IsNullOrWhiteSpace(serializedState))
        {
            throw new ArgumentException("Serialized agent session state is required.", nameof(serializedState));
        }

        AgentSessionStateJson = serializedState;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string BuildTitle(string message)
    {
        var normalized = message.Trim();
        if (normalized.Length <= 60)
        {
            return normalized;
        }

        return $"{normalized[..57]}...";
    }
}
