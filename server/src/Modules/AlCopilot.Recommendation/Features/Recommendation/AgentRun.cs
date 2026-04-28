using Microsoft.Extensions.AI;

namespace AlCopilot.Recommendation.Features.Recommendation;

public sealed class AgentRun
{
    public Guid Id { get; private set; }
    public Guid ChatSessionId { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string? Provider { get; private set; }
    public string? Model { get; private set; }
    public string? FinishReason { get; private set; }
    public long? InputTokenCount { get; private set; }
    public long? OutputTokenCount { get; private set; }
    public long? ReasoningTokenCount { get; private set; }
    public string? ErrorSummary { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    private AgentRun()
    {
    }

    public static AgentRun Start(Guid chatSessionId)
    {
        return new AgentRun
        {
            Id = Guid.NewGuid(),
            ChatSessionId = chatSessionId,
            Status = "running",
            StartedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public void Complete(string? provider, string? model, string? finishReason, UsageDetails? usage)
    {
        Status = "completed";
        Provider = provider;
        Model = model;
        FinishReason = finishReason;
        InputTokenCount = usage?.InputTokenCount;
        OutputTokenCount = usage?.OutputTokenCount;
        ReasoningTokenCount = usage?.ReasoningTokenCount;
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Fail(Exception exception)
    {
        Status = "failed";
        ErrorSummary = exception.Message;
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }
}
