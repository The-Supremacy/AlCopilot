using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Models;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.ImportSync;

public sealed class StartImportHandler(
    IImportSourceStrategyResolver strategyResolver,
    IImportBatchRepository importBatchRepository,
    ImportBatchWorkflowService workflowService,
    AuditLogWriter auditLogWriter,
    ICurrentActorAccessor currentActorAccessor,
    IUnitOfWork unitOfWork) : IRequestHandler<StartImportCommand, ImportBatchDto>
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
            strategyResult.Import,
            strategyResult.SourceFingerprint);

        var diagnostics = await workflowService.ValidateAsync(batch.ImportContent, cancellationToken);
        var review = await workflowService.ReviewAsync(batch.ImportContent, diagnostics, cancellationToken);
        batch.RecordValidationAndReview(diagnostics, review.Summary, review.Conflicts, review.Rows);

        importBatchRepository.Add(batch);
        auditLogWriter.Write(
            "import-batch.create",
            "import-batch",
            batch.Id.ToString(),
            $"Created import batch '{batch.Id}' from '{batch.Provenance.DisplayName ?? batch.StrategyKey}'.");
        auditLogWriter.Write(
            "import-batch.validate",
            "import-batch",
            batch.Id.ToString(),
            $"Validated import batch '{batch.Id}' during import start with {diagnostics.Count} diagnostics.");
        auditLogWriter.Write(
            "import-batch.review",
            "import-batch",
            batch.Id.ToString(),
            $"Generated review snapshot for import batch '{batch.Id}' with {review.Conflicts.Count} conflicts and {review.Rows.Count} review rows.");
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return batch.ToDto();
    }
}
