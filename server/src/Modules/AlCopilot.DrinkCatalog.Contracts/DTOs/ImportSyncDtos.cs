namespace AlCopilot.DrinkCatalog.Contracts.DTOs;

public sealed record ImportSourceInput(
    string? SourceReference,
    string? DisplayName,
    string? ContentType,
    Dictionary<string, string?> Metadata);

public sealed record ImportDiagnosticDto(
    int? RowNumber,
    string Code,
    string Message,
    string Severity);

public sealed record ImportReviewConflictDto(
    string TargetType,
    string TargetKey,
    string Action,
    string Summary);

public sealed record ImportReviewRowDto(
    string TargetType,
    string TargetKey,
    string Action,
    string ChangeSummary,
    bool HasConflict,
    bool HasError);

public sealed record ImportReviewSummaryDto(
    int CreateCount,
    int UpdateCount,
    int SkipCount);

public sealed record ImportApplySummaryDto(
    int CreatedCount,
    int UpdatedCount,
    int SkippedCount,
    int RejectedCount);

public sealed record ImportDecisionInput(
    string TargetType,
    string TargetKey,
    string Decision,
    string? Reason);

public sealed record ImportBatchDto(
    Guid Id,
    string StrategyKey,
    string Status,
    string? SourceFingerprint,
    ImportSourceInput Source,
    List<ImportDiagnosticDto> Diagnostics,
    List<ImportReviewConflictDto> ReviewConflicts,
    List<ImportReviewRowDto> ReviewRows,
    ImportReviewSummaryDto? ReviewSummary,
    ImportApplySummaryDto? ApplySummary,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ValidatedAtUtc,
    DateTimeOffset? ReviewedAtUtc,
    DateTimeOffset? AppliedAtUtc,
    DateTimeOffset LastUpdatedAtUtc);
