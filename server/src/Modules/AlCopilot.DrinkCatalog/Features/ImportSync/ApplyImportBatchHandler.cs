using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed class ApplyImportBatchHandler(
    IImportBatchRepository importBatchRepository,
    ImportBatchWorkflowService workflowService,
    AuditLogWriter auditLogWriter,
    ICurrentActorAccessor currentActorAccessor,
    IUnitOfWork unitOfWork) : IRequestHandler<ApplyImportBatchCommand, ImportBatchDto>
{
    public async ValueTask<ImportBatchDto> Handle(ApplyImportBatchCommand request, CancellationToken cancellationToken)
    {
        var currentActor = currentActorAccessor.GetCurrent();
        var batch = await importBatchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException($"Import batch '{request.BatchId}' not found.");

        if (batch.Status is ImportBatchStatus.Completed)
            throw new InvalidStateException("Batch has already been completed.");

        if (batch.Status is ImportBatchStatus.Cancelled)
            throw new InvalidStateException("Cancelled batches cannot be applied.");

        if (batch.Diagnostics.Any(d => string.Equals(d.Severity, "error", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidStateException("Batch cannot be applied while validation errors are present.");

        if (!request.OverrideDuplicateFingerprint && !string.IsNullOrWhiteSpace(batch.SourceFingerprint))
        {
            var existing = await importBatchRepository.GetAppliedByStrategyAndFingerprintAsync(
                batch.Strategy,
                batch.SourceFingerprint!,
                cancellationToken);

            if (existing is not null && existing.Id != batch.Id)
                throw new ConflictException("A completed batch already exists for this source fingerprint. Use override to re-run.");
        }

        if (batch.ReviewSummary is null || batch.ReviewedAtUtc is null)
        {
            var diagnostics = await workflowService.ValidateAsync(batch.ImportContent, cancellationToken);
            batch.RecordValidation(diagnostics);

            var review = await workflowService.ReviewAsync(batch.ImportContent, batch.Diagnostics, cancellationToken);
            batch.RecordReview(review.Summary, review.Conflicts, review.Rows, batch.Diagnostics);
        }

        var decisions = (request.Decisions ?? [])
            .ToDictionary(
                d => ImportBatchWorkflowService.BuildDecisionKey(d.TargetType, d.TargetKey),
                StringComparer.OrdinalIgnoreCase);

        var summary = await workflowService.ApplyAsync(batch, decisions, currentActor, cancellationToken);
        auditLogWriter.Write(
            "import-batch.apply",
            "import-batch",
            batch.Id.ToString(),
            $"Applied import batch '{batch.Id}' with {summary.CreatedCount} created, {summary.UpdatedCount} updated, {summary.RejectedCount} rejected.");
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return batch.ToDto();
    }
}
