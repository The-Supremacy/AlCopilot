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

public sealed record ImportReviewRowDto(
    string TargetType,
    string TargetKey,
    string Action,
    string ChangeSummary,
    bool RequiresReview,
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

public sealed record ImportBatchDto(
    Guid Id,
    string StrategyKey,
    string Status,
    bool RequiresReview,
    string ApplyReadiness,
    ImportSourceInput Source,
    List<ImportDiagnosticDto> Diagnostics,
    List<ImportReviewRowDto> ReviewRows,
    ImportReviewSummaryDto? ReviewSummary,
    ImportApplySummaryDto? ApplySummary,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ValidatedAtUtc,
    DateTimeOffset? ReviewedAtUtc,
    DateTimeOffset? AppliedAtUtc,
    DateTimeOffset LastUpdatedAtUtc);

public sealed record ImportBatchApplyResultDto(
    ImportBatchDto Batch,
    string ApplyReadiness,
    bool WasApplied);
