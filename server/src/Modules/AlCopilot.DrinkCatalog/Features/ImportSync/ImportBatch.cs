using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.Shared.Domain;

namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed class ImportBatch : AggregateRoot<Guid>
{
    public string StrategyKey { get; private set; } = null!;
    public ImportStrategyKey Strategy => ImportStrategyKeyExtensions.Parse(StrategyKey);
    public ImportBatchStatus Status { get; private set; }
    public string? SourceFingerprint { get; private set; }
    public ImportProvenance Provenance { get; private set; } = ImportProvenance.Empty;
    public NormalizedCatalogImport ImportContent { get; private set; } = new([], [], []);
    public List<ImportDiagnostic> Diagnostics { get; private set; } = [];
    public List<ImportReviewConflict> ReviewConflicts { get; private set; } = [];
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

    private ImportBatch()
    {
    }

    public static ImportBatch Create(
        ImportStrategyKey strategyKey,
        ImportProvenance provenance,
        NormalizedCatalogImport importContent,
        string? sourceFingerprint)
    {
        var now = DateTimeOffset.UtcNow;

        return new ImportBatch
        {
            Id = Guid.NewGuid(),
            StrategyKey = strategyKey.ToWireValue(),
            Status = ImportBatchStatus.InProgress,
            SourceFingerprint = NormalizeOptional(sourceFingerprint),
            Provenance = provenance ?? ImportProvenance.Empty,
            ImportContent = importContent ?? new([], [], []),
            CreatedAtUtc = now,
            LastUpdatedAtUtc = now
        };
    }

    public void UpdateSourceFingerprint(string? sourceFingerprint)
    {
        SourceFingerprint = NormalizeOptional(sourceFingerprint);
        Touch();
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
        ReviewConflicts = [];
        ReviewRows = [];
        ApplySummary = null;
        DecisionAuditTrail = [];
        Touch();
    }

    public void RecordReview(
        ImportReviewSummary reviewSummary,
        IEnumerable<ImportReviewConflict> reviewConflicts,
        IEnumerable<ImportReviewRow> reviewRows,
        IEnumerable<ImportDiagnostic>? diagnostics = null)
    {
        ArgumentNullException.ThrowIfNull(reviewSummary);

        ReviewSummary = reviewSummary;
        ReviewConflicts = NormalizeReviewConflicts(reviewConflicts);
        ReviewRows = NormalizeReviewRows(reviewRows);
        if (diagnostics is not null)
            Diagnostics = NormalizeDiagnostics(diagnostics);
        ReviewedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    public void RecordValidationAndReview(
        IEnumerable<ImportDiagnostic>? diagnostics,
        ImportReviewSummary reviewSummary,
        IEnumerable<ImportReviewConflict> reviewConflicts,
        IEnumerable<ImportReviewRow> reviewRows)
    {
        Diagnostics = NormalizeDiagnostics(diagnostics);
        ValidatedAtUtc = DateTimeOffset.UtcNow;
        ReviewSummary = reviewSummary ?? throw new ArgumentNullException(nameof(reviewSummary));
        ReviewConflicts = NormalizeReviewConflicts(reviewConflicts);
        ReviewRows = NormalizeReviewRows(reviewRows);
        ReviewedAtUtc = DateTimeOffset.UtcNow;
        ApplySummary = null;
        DecisionAuditTrail = [];
        Touch();
    }

    public void MarkCompleted(ImportApplySummary applySummary, IEnumerable<ImportDecisionAuditEntry>? decisionAuditTrail = null)
    {
        ArgumentNullException.ThrowIfNull(applySummary);

        Status = ImportBatchStatus.Completed;
        ApplySummary = applySummary;
        DecisionAuditTrail = NormalizeDecisionAuditTrail(decisionAuditTrail);
        AppliedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    public void MarkCancelled()
    {
        if (Status == ImportBatchStatus.Completed)
            throw new InvalidOperationException("Completed batches cannot be cancelled.");

        Status = ImportBatchStatus.Cancelled;
        CancelledAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    private void Touch()
    {
        LastUpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Value cannot be empty.", paramName);

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
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

    private static List<ImportReviewConflict> NormalizeReviewConflicts(
        IEnumerable<ImportReviewConflict>? reviewConflicts)
    {
        return reviewConflicts?.ToList() ?? [];
    }

    private static List<ImportReviewRow> NormalizeReviewRows(
        IEnumerable<ImportReviewRow>? reviewRows)
    {
        return reviewRows?.ToList() ?? [];
    }
}
