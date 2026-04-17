using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed class ReviewImportBatchHandler(
    IImportBatchRepository importBatchRepository,
    IImportBatchProcessingService processingService,
    IAuditLogWriter auditLogWriter,
    IDrinkCatalogUnitOfWork unitOfWork) : IRequestHandler<ReviewImportBatchCommand, ImportBatchDto>
{
    public async ValueTask<ImportBatchDto> Handle(ReviewImportBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await importBatchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException($"Import batch '{request.BatchId}' not found.");

        if (batch.Status is ImportBatchStatus.Completed)
            throw new InvalidStateException("Completed batches cannot be reviewed.");

        if (batch.Status is ImportBatchStatus.Cancelled)
            throw new InvalidStateException("Cancelled batches cannot be reviewed.");

        var processingResult = await processingService.ProcessAsync(batch.ImportContent, cancellationToken);
        batch.RecordReviewedSnapshot(processingResult);
        importBatchRepository.Update(batch);

        auditLogWriter.Write(
            "import-batch.review",
            "import-batch",
            batch.Id.ToString(),
            $"Reviewed import batch '{batch.Id}' with {processingResult.ReviewSummary.UpdateCount} updates and {processingResult.ReviewRows.Count} review rows.");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return batch.ToDto();
    }
}
