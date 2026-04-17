namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public sealed record ImportDecisionAuditEntry(
    string TargetType,
    string TargetKey,
    string Decision,
    string? Reason,
    string? ActorUserId,
    string ActorDisplayName,
    DateTimeOffset RecordedAtUtc);
