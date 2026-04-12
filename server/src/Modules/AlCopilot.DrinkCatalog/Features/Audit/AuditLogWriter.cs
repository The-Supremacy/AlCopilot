namespace AlCopilot.DrinkCatalog.Features.Audit;

public sealed class AuditLogWriter(IAuditLogEntryRepository repository)
{
    public void Write(
        string action,
        string subjectType,
        string? subjectKey,
        string summary,
        string actor = "anonymous")
    {
        repository.Add(new AuditLogEntry
        {
            Action = Normalize(action, nameof(action)),
            SubjectType = Normalize(subjectType, nameof(subjectType)),
            SubjectKey = NormalizeOptional(subjectKey),
            Actor = Normalize(actor, nameof(actor)),
            Summary = Normalize(summary, nameof(summary)),
            OccurredAtUtc = DateTimeOffset.UtcNow,
        });
    }

    private static string Normalize(string value, string paramName)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Value cannot be empty.", paramName);

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
