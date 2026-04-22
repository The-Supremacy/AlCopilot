using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public sealed class CancelImportBatchHandler(
    IImportBatchRepository importBatchRepository,
    IAuditLogWriter auditLogWriter,
    IDrinkCatalogUnitOfWork unitOfWork) : IRequestHandler<CancelImportBatchCommand, ImportBatchDto>
{
    public async ValueTask<ImportBatchDto> Handle(CancelImportBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await importBatchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException($"Import batch '{request.BatchId}' not found.");

        if (batch.Status is ImportBatchStatus.Completed)
            throw new InvalidStateException("Completed batches cannot be cancelled.");

        if (batch.Status is ImportBatchStatus.Cancelled)
            return batch.ToDto();

        batch.MarkCancelled();
        importBatchRepository.Update(batch);
        auditLogWriter.Write(
            "import-batch.cancel",
            "import-batch",
            batch.Id.ToString(),
            $"Cancelled import batch '{batch.Id}'.");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return batch.ToDto();
    }
}
