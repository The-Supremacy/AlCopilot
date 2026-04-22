using AlCopilot.Recommendation.Contracts.Events;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Recommendation.Features.Recommendation.Agents;
using AlCopilot.Shared.Domain;
using System.Text.Json;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class ChatSession : AggregateRoot<Guid>
{
    public string CustomerId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? AgentSessionStateJson { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public List<ChatTurn> Turns { get; private set; } = [];

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

    public void AppendUserTurn(string content)
    {
        var turn = ChatTurn.CreateUserTurn(Id, Turns.Count + 1, content);
        Turns.Add(turn);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        Raise(new RecommendationCustomerMessageRecordedEvent(Id, turn.Id));
    }

    public void AppendAssistantTurn(
        string content,
        IReadOnlyCollection<RecommendationGroupDto> recommendationGroups,
        IReadOnlyCollection<RecommendationToolInvocationDto> toolInvocations,
        IReadOnlyCollection<RecommendationExecutionTraceStep>? executionTraceSteps = null)
    {
        var turn = ChatTurn.CreateAssistantTurn(
            Id,
            Turns.Count + 1,
            content,
            recommendationGroups,
            toolInvocations,
            executionTraceSteps);
        Turns.Add(turn);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        Raise(new RecommendationAssistantMessageRecordedEvent(Id, turn.Id));
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

public sealed class ChatTurn
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Guid Id { get; private set; }
    public Guid ChatSessionId { get; private set; }
    public int Sequence { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string RecommendationGroupsJson { get; private set; } = "[]";
    public string ToolInvocationsJson { get; private set; } = "[]";
    public string? ExecutionTraceJson { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private ChatTurn()
    {
    }

    public static ChatTurn CreateUserTurn(Guid chatSessionId, int sequence, string content)
    {
        return new ChatTurn
        {
            Id = Guid.NewGuid(),
            ChatSessionId = chatSessionId,
            Sequence = sequence,
            Role = "user",
            Content = content,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public static ChatTurn CreateAssistantTurn(
        Guid chatSessionId,
        int sequence,
        string content,
        IReadOnlyCollection<RecommendationGroupDto> recommendationGroups,
        IReadOnlyCollection<RecommendationToolInvocationDto> toolInvocations,
        IReadOnlyCollection<RecommendationExecutionTraceStep>? executionTraceSteps = null)
    {
        return new ChatTurn
        {
            Id = Guid.NewGuid(),
            ChatSessionId = chatSessionId,
            Sequence = sequence,
            Role = "assistant",
            Content = content,
            RecommendationGroupsJson = JsonSerializer.Serialize(recommendationGroups),
            ToolInvocationsJson = JsonSerializer.Serialize(toolInvocations),
            ExecutionTraceJson = executionTraceSteps is null ? null : JsonSerializer.Serialize(executionTraceSteps),
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public List<RecommendationGroupDto> GetRecommendationGroups() =>
        JsonSerializer.Deserialize<List<RecommendationGroupDto>>(RecommendationGroupsJson, SerializerOptions) ?? [];

    public List<RecommendationToolInvocationDto> GetToolInvocations() =>
        JsonSerializer.Deserialize<List<RecommendationToolInvocationDto>>(ToolInvocationsJson, SerializerOptions) ?? [];

    internal List<RecommendationExecutionTraceStep> GetExecutionTraceSteps() =>
        string.IsNullOrWhiteSpace(ExecutionTraceJson)
            ? []
            : JsonSerializer.Deserialize<List<RecommendationExecutionTraceStep>>(ExecutionTraceJson, SerializerOptions) ?? [];
}
