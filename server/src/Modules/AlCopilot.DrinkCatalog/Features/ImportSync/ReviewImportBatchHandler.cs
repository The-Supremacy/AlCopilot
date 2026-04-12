using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed class ReviewImportBatchHandler(
    IImportBatchRepository importBatchRepository,
    ImportBatchWorkflowService workflowService,
    AuditLogWriter auditLogWriter,
    IUnitOfWork unitOfWork) : IRequestHandler<ReviewImportBatchCommand, ImportBatchDto>
{
    public async ValueTask<ImportBatchDto> Handle(ReviewImportBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await importBatchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException($"Import batch '{request.BatchId}' not found.");

        if (batch.Status is ImportBatchStatus.Completed)
            throw new InvalidStateException("Completed batches cannot be reviewed.");

        if (batch.Status is ImportBatchStatus.Cancelled)
            throw new InvalidStateException("Cancelled batches cannot be reviewed.");

        var diagnostics = await workflowService.ValidateAsync(batch.ImportContent, cancellationToken);
        var review = await workflowService.ReviewAsync(batch.ImportContent, diagnostics, cancellationToken);
        batch.RecordValidationAndReview(diagnostics, review.Summary, review.Conflicts, review.Rows);

        auditLogWriter.Write(
            "import-batch.review",
            "import-batch",
            batch.Id.ToString(),
            $"Reviewed import batch '{batch.Id}' with {review.Conflicts.Count} conflicts and {review.Rows.Count} review rows.");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return batch.ToDto();
    }
}
