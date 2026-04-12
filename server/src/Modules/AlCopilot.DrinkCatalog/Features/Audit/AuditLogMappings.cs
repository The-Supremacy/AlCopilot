using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.DrinkCatalog.Features.Audit;

internal static class AuditLogMappings
{
    public static AuditLogEntryDto ToDto(this AuditLogEntry entry) =>
        new(
            entry.Id,
            entry.Action,
            entry.SubjectType,
            entry.SubjectKey,
            entry.Actor,
            entry.Summary,
            entry.OccurredAtUtc);
}
