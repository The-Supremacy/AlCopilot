using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed class InitializeImportBatchHandler(
    IImportSourceStrategyResolver strategyResolver,
    IImportBatchRepository importBatchRepository,
    IImportBatchProcessingService processingService,
    IAuditLogWriter auditLogWriter,
    ICurrentActorAccessor currentActorAccessor,
    IDrinkCatalogUnitOfWork unitOfWork) : IRequestHandler<StartImportCommand, ImportBatchDto>
{
    public async ValueTask<ImportBatchDto> Handle(StartImportCommand request, CancellationToken cancellationToken)
    {
        var currentActor = currentActorAccessor.GetCurrent();
        var strategy = strategyResolver.GetRequired(request.StrategyKey);
        var strategyResult = await strategy.CreateImportAsync(
            new ImportSourceStrategyRequest(
                request.Payload,
                new ImportProvenance(
                    request.Source.SourceReference,
                    request.Source.DisplayName,
                    request.Source.ContentType,
                    request.Source.Metadata ?? [],
                    currentActor.UserId,
                    currentActor.DisplayName)),
            cancellationToken);
        var provenance = strategyResult.Provenance with
        {
            InitiatedByUserId = currentActor.UserId,
            InitiatedByDisplayName = currentActor.DisplayName,
        };

        var batch = ImportBatch.Create(
            strategy.Key,
            provenance,
            strategyResult.Import);

        var processingResult = await processingService.ProcessAsync(batch.ImportContent, cancellationToken);
        batch.RecordPreparedSnapshot(processingResult);

        importBatchRepository.Add(batch);
        auditLogWriter.Write(
            "import-batch.initialize",
            "import-batch",
            batch.Id.ToString(),
            $"Initialized import batch '{batch.Id}' from '{batch.Provenance.DisplayName ?? batch.StrategyKey}'.");
        auditLogWriter.Write(
            "import-batch.validate",
            "import-batch",
            batch.Id.ToString(),
            $"Validated import batch '{batch.Id}' during import initialization with {processingResult.Diagnostics.Count} diagnostics.");
        auditLogWriter.Write(
            "import-batch.process",
            "import-batch",
            batch.Id.ToString(),
            $"Processed import batch '{batch.Id}' into a review snapshot with {processingResult.ReviewSummary.UpdateCount} updates and {processingResult.ReviewRows.Count} review rows.");
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return batch.ToDto();
    }
}
