namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class AgentMessageDiagnostic
{
    public Guid Id { get; private set; }
    public Guid ChatSessionId { get; private set; }
    public Guid AgentRunId { get; private set; }
    public Guid? AgentMessageId { get; private set; }
    public string Kind { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Text { get; private set; }
    public string? RawPayloadJson { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private AgentMessageDiagnostic()
    {
    }

    public static AgentMessageDiagnostic Create(
        Guid chatSessionId,
        Guid agentRunId,
        Guid? agentMessageId,
        string kind,
        string name,
        string? text,
        string? rawPayloadJson)
    {
        return new AgentMessageDiagnostic
        {
            Id = Guid.NewGuid(),
            ChatSessionId = chatSessionId,
            AgentRunId = agentRunId,
            AgentMessageId = agentMessageId,
            Kind = kind,
            Name = name,
            Text = string.IsNullOrWhiteSpace(text) ? null : text,
            RawPayloadJson = string.IsNullOrWhiteSpace(rawPayloadJson) ? null : rawPayloadJson,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }
}
