namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed record ImportDecisionAuditEntry(
    string TargetType,
    string TargetKey,
    string Decision,
    string? Reason,
    DateTimeOffset RecordedAtUtc);
