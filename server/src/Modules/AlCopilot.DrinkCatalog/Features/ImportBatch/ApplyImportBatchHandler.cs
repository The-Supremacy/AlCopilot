using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.Drink.Abstractions;
using AlCopilot.Recommendation.Contracts.Commands;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.ImportBatch;

public sealed class ApplyImportBatchHandler(
    IImportBatchRepository importBatchRepository,
    IImportBatchProcessingService processingService,
    IImportBatchApplyService applyService,
    IDrinkQueryService drinkQueryService,
    IMediator mediator,
    IAuditLogWriter auditLogWriter,
    IDrinkCatalogUnitOfWork unitOfWork) : IRequestHandler<ApplyImportBatchCommand, ImportBatchApplyResultDto>
{
    public async ValueTask<ImportBatchApplyResultDto> Handle(ApplyImportBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await importBatchRepository.GetByIdAsync(request.BatchId, cancellationToken)
            ?? throw new NotFoundException($"Import batch '{request.BatchId}' not found.");

        if (batch.Status is ImportBatchStatus.Completed)
            throw new InvalidStateException("Batch has already been completed.");

        if (batch.Status is ImportBatchStatus.Cancelled)
            throw new InvalidStateException("Cancelled batches cannot be applied.");

        if (batch.ReviewSummary is null || batch.ReviewRows.Count == 0)
        {
            var processingResult = await processingService.ProcessAsync(batch.ImportContent, cancellationToken);
            batch.RecordPreparedSnapshot(processingResult);
            importBatchRepository.Update(batch);
        }

        var batchApplyReadiness = processingService.GetBatchApplyReadiness(batch);
        if (batchApplyReadiness is not ImportBatchApplyReadiness.Ready)
        {
            return batch.ToApplyResultDto(batchApplyReadiness, false);
        }

        var summary = await applyService.ApplyAsync(batch, cancellationToken);
        var indexedCatalog = await drinkQueryService.GetAllAsync(cancellationToken);
        await mediator.Send(new ReplaceRecommendationSemanticCatalogCommand(indexedCatalog), cancellationToken);
        importBatchRepository.Update(batch);
        auditLogWriter.Write(
            "import-batch.apply",
            "import-batch",
            batch.Id.ToString(),
            $"Applied import batch '{batch.Id}' with {summary.CreatedCount} created, {summary.UpdatedCount} updated, {summary.RejectedCount} rejected.");
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return batch.ToApplyResultDto(batch.GetApplyReadiness(), true);
    }
}
