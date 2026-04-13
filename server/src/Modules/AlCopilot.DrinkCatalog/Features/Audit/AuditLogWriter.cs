using AlCopilot.Shared.Models;

namespace AlCopilot.DrinkCatalog.Features.Audit;

public sealed class AuditLogWriter(IAuditLogEntryRepository repository, ICurrentActorAccessor? currentActorAccessor = null)
{
    public void Write(
        string action,
        string subjectType,
        string? subjectKey,
        string summary,
        string? actor = null,
        string? actorUserId = null)
    {
        var currentActor = currentActorAccessor?.GetCurrent() ?? CurrentActor.Anonymous;

        repository.Add(new AuditLogEntry
        {
            Action = Normalize(action, nameof(action)),
            SubjectType = Normalize(subjectType, nameof(subjectType)),
            SubjectKey = NormalizeOptional(subjectKey),
            ActorUserId = NormalizeOptional(actorUserId ?? currentActor.UserId),
            Actor = Normalize(actor ?? currentActor.DisplayName, nameof(actor)),
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
