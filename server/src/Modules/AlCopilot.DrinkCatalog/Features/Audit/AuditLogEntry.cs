namespace AlCopilot.DrinkCatalog.Features.Audit;

public sealed class AuditLogEntry
{
    public long Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string SubjectType { get; set; } = string.Empty;
    public string? SubjectKey { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
}
