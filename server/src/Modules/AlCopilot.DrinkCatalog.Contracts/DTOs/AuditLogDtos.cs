namespace AlCopilot.DrinkCatalog.Contracts.DTOs;

public sealed record AuditLogEntryDto(
    long Id,
    string Action,
    string SubjectType,
    string? SubjectKey,
    string? ActorUserId,
    string Actor,
    string Summary,
    DateTimeOffset OccurredAtUtc);
