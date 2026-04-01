namespace AlCopilot.Shared.Data;

public sealed class DomainEventRecord
{
    public long Id { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
    public long Sequence { get; set; }
    public bool IsPublished { get; set; }
}
