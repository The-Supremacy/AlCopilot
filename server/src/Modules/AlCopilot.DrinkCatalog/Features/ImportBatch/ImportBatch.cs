using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;
using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public sealed class ImportBatch : AggregateRoot<Guid>
{
    public string StrategyKey { get; private set; } = null!;
    public ImportStrategyKey Strategy => ImportStrategyKeyExtensions.Parse(StrategyKey);
    public ImportBatchStatus Status { get; private set; }
    public ImportProvenance Provenance { get; private set; } = ImportProvenance.Empty;
    public NormalizedCatalogImport ImportContent { get; private set; } = new([], [], []);
    public List<ImportDiagnostic> Diagnostics { get; private set; } = [];
    public List<ImportReviewRow> ReviewRows { get; private set; } = [];
    public List<ImportDecisionAuditEntry> DecisionAuditTrail { get; private set; } = [];
    public ImportReviewSummary? ReviewSummary { get; private set; }
    public ImportApplySummary? ApplySummary { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ValidatedAtUtc { get; private set; }
    public DateTimeOffset? ReviewedAtUtc { get; private set; }
    public DateTimeOffset? AppliedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public DateTimeOffset LastUpdatedAtUtc { get; private set; }
    public bool RequiresReview => ReviewRows.Any(row => row.RequiresReview);

    private ImportBatch()
    {
    }

    public static ImportBatch Create(
        ImportStrategyKey strategyKey,
        ImportProvenance provenance,
        NormalizedCatalogImport importContent)
    {
        var now = DateTimeOffset.UtcNow;

        var batch = new ImportBatch
        {
            Id = Guid.NewGuid(),
            StrategyKey = strategyKey.ToWireValue(),
            Status = ImportBatchStatus.InProgress,
            Provenance = provenance ?? ImportProvenance.Empty,
            ImportContent = importContent ?? new([], [], []),
            CreatedAtUtc = now,
            LastUpdatedAtUtc = now
        };

        batch.Raise(new ImportBatchInitializedEvent(batch.Id));
        return batch;
    }

    public void RecordValidation(IEnumerable<ImportDiagnostic>? diagnostics = null)
    {
        Diagnostics = NormalizeDiagnostics(diagnostics);
        ValidatedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    public void RecordValidationFailure(IEnumerable<ImportDiagnostic> diagnostics)
    {
        var normalizedDiagnostics = NormalizeDiagnostics(diagnostics);
        if (normalizedDiagnostics.Count == 0)
            throw new InvalidOperationException("Validation failure requires at least one diagnostic.");

        Diagnostics = normalizedDiagnostics;
        ValidatedAtUtc = DateTimeOffset.UtcNow;
        ReviewSummary = null;
        ReviewRows = [];
        ApplySummary = null;
        DecisionAuditTrail = [];
        ReviewedAtUtc = null;
        Touch();
    }

    public void RecordPreparedSnapshot(ImportBatchProcessingResult processingResult)
    {
        ArgumentNullException.ThrowIfNull(processingResult);

        Diagnostics = NormalizeDiagnostics(processingResult.Diagnostics);
        ValidatedAtUtc = DateTimeOffset.UtcNow;
        ReviewSummary = processingResult.ReviewSummary;
        ReviewRows = NormalizeReviewRows(processingResult.ReviewRows);
        ReviewedAtUtc = null;
        ApplySummary = null;
        DecisionAuditTrail = [];
        Touch();
        Raise(new ImportBatchPreparedEvent(Id));
    }

    public void RecordReviewedSnapshot(ImportBatchProcessingResult processingResult)
    {
        ArgumentNullException.ThrowIfNull(processingResult);

        Diagnostics = NormalizeDiagnostics(processingResult.Diagnostics);
        ValidatedAtUtc = DateTimeOffset.UtcNow;
        ReviewSummary = processingResult.ReviewSummary;
        ReviewRows = NormalizeReviewRows(processingResult.ReviewRows);
        ReviewedAtUtc = DateTimeOffset.UtcNow;
        ApplySummary = null;
        DecisionAuditTrail = [];
        Touch();
        Raise(new ImportBatchReviewedEvent(Id));
    }

    public void MarkCompleted(ImportApplySummary applySummary, IEnumerable<ImportDecisionAuditEntry>? decisionAuditTrail = null)
    {
        ArgumentNullException.ThrowIfNull(applySummary);

        Status = ImportBatchStatus.Completed;
        ApplySummary = applySummary;
        DecisionAuditTrail = NormalizeDecisionAuditTrail(decisionAuditTrail);
        AppliedAtUtc = DateTimeOffset.UtcNow;
        Touch();
        Raise(new ImportBatchCompletedEvent(Id));
    }

    public void MarkCancelled()
    {
        if (Status == ImportBatchStatus.Completed)
            throw new InvalidOperationException("Completed batches cannot be cancelled.");

        Status = ImportBatchStatus.Cancelled;
        CancelledAtUtc = DateTimeOffset.UtcNow;
        Touch();
        Raise(new ImportBatchCancelledEvent(Id));
    }

    private void Touch()
    {
        LastUpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public ImportBatchApplyReadiness GetApplyReadiness()
    {
        if (Status is ImportBatchStatus.Completed)
            return ImportBatchApplyReadiness.Completed;

        if (Status is ImportBatchStatus.Cancelled)
            return ImportBatchApplyReadiness.Cancelled;

        if (Diagnostics.Any(d => string.Equals(d.Severity, "error", StringComparison.OrdinalIgnoreCase)))
            return ImportBatchApplyReadiness.BlockedByValidationErrors;

        if (RequiresReview && ReviewedAtUtc is null)
            return ImportBatchApplyReadiness.RequiresReview;

        return ImportBatchApplyReadiness.Ready;
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Value cannot be empty.", paramName);

        return normalized;
    }

    private static List<ImportDiagnostic> NormalizeDiagnostics(IEnumerable<ImportDiagnostic>? diagnostics)
    {
        return diagnostics?.ToList() ?? [];
    }

    private static List<ImportDecisionAuditEntry> NormalizeDecisionAuditTrail(
        IEnumerable<ImportDecisionAuditEntry>? decisionAuditTrail)
    {
        return decisionAuditTrail?.ToList() ?? [];
    }

    private static List<ImportReviewRow> NormalizeReviewRows(
        IEnumerable<ImportReviewRow>? reviewRows)
    {
        return reviewRows?.ToList() ?? [];
    }
}
