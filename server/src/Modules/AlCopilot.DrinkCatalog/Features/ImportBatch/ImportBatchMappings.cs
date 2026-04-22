using AlCopilot.DrinkCatalog.Contracts.DTOs;

namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

internal static class ImportBatchMappings
{
    public static ImportBatchDto ToDto(this ImportBatch batch)
    {
        return new ImportBatchDto(
            batch.Id,
            batch.StrategyKey,
            batch.Status.ToString(),
            batch.RequiresReview,
            batch.GetApplyReadiness().ToString(),
            new ImportSourceInput(
                batch.Provenance.SourceReference,
                batch.Provenance.DisplayName,
                batch.Provenance.ContentType,
                new Dictionary<string, string?>(batch.Provenance.Metadata)),
            batch.Diagnostics
                .Select(d => new ImportDiagnosticDto(d.RowNumber, d.Code, d.Message, d.Severity))
                .ToList(),
            batch.ReviewRows
                .Select(r => new ImportReviewRowDto(
                    r.TargetType,
                    r.TargetKey,
                    r.Action,
                    r.ChangeSummary,
                    r.RequiresReview,
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

    public static ImportBatchApplyResultDto ToApplyResultDto(
        this ImportBatch batch,
        ImportBatchApplyReadiness applyReadiness,
        bool wasApplied)
    {
        return new ImportBatchApplyResultDto(
            batch.ToDto(),
            applyReadiness.ToString(),
            wasApplied);
    }
}
