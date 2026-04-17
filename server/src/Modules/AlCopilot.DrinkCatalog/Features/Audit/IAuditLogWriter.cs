namespace AlCopilot.DrinkCatalog.Features.Audit;

public interface IAuditLogWriter
{
    void Write(
        string action,
        string subjectType,
        string? subjectKey,
        string summary,
        string? actor = null,
        string? actorUserId = null);
}
