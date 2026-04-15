using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.DrinkCatalog.Features.ImportSync;

internal static class ImportBatchMappings
{
    public static ImportBatchDto ToDto(this ImportBatch batch)
    {
        return new ImportBatchDto(
            batch.Id,
            batch.StrategyKey,
            batch.Status.ToString(),
            batch.SourceFingerprint,
            new ImportSourceInput(
                batch.Provenance.SourceReference,
                batch.Provenance.DisplayName,
                batch.Provenance.ContentType,
                new Dictionary<string, string?>(batch.Provenance.Metadata)),
            batch.Diagnostics
                .Select(d => new ImportDiagnosticDto(d.RowNumber, d.Code, d.Message, d.Severity))
                .ToList(),
            batch.ReviewConflicts
                .Select(c => new ImportReviewConflictDto(c.TargetType, c.TargetKey, c.Action, c.Summary))
                .ToList(),
            batch.ReviewRows
                .Select(r => new ImportReviewRowDto(
                    r.TargetType,
                    r.TargetKey,
                    r.Action,
                    r.ChangeSummary,
                    r.HasConflict,
                    r.HasError))
                .ToList(),
            batch.ReviewSummary is null
                ? null
                : new ImportReviewSummaryDto(
                    batch.ReviewSummary.CreateCount,
                    batch.ReviewSummary.UpdateCount,
                    batch.ReviewSummary.SkipCount),
            batch.ApplySummary is null
                ? null
                : new ImportApplySummaryDto(
                    batch.ApplySummary.CreatedCount,
                    batch.ApplySummary.UpdatedCount,
                    batch.ApplySummary.SkippedCount,
            batch.ApplySummary.RejectedCount),
            batch.CreatedAtUtc,
            batch.ValidatedAtUtc,
            batch.ReviewedAtUtc,
            batch.AppliedAtUtc,
            batch.LastUpdatedAtUtc);
    }
}
