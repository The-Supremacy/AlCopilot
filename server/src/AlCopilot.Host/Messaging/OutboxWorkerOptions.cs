namespace AlCopilot.Host.Messaging;

public sealed class OutboxWorkerOptions
{
    public const string SectionName = "Messaging:Outbox";

    public int BatchSize { get; set; } = 50;

    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(2);
}
