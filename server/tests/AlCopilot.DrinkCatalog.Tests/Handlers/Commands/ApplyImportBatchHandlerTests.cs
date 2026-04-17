using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.ImportSync;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class ApplyImportBatchHandlerTests
{
    private readonly IImportBatchRepository _importBatchRepository = Substitute.For<IImportBatchRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly IDrinkQueryService _drinkQueryService = Substitute.For<IDrinkQueryService>();
    private readonly IImportBatchProcessingService _processingService = Substitute.For<IImportBatchProcessingService>();
    private readonly IImportBatchApplyService _applyService = Substitute.For<IImportBatchApplyService>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IDrinkCatalogUnitOfWork _unitOfWork = Substitute.For<IDrinkCatalogUnitOfWork>();
    private readonly ApplyImportBatchHandler _handler;

    public ApplyImportBatchHandlerTests()
    {
        _handler = new ApplyImportBatchHandler(
            _importBatchRepository,
            _processingService,
            _applyService,
            new AuditLogWriter(_auditRepository),
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenBatchHasValidationErrors_ReturnsBlockedResult()
    {
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []));
        batch.RecordPreparedSnapshot(new ImportBatchProcessingResult(
            [new ImportDiagnostic(null, "recipe-ingredient-missing", "Ingredient is missing.", "error")],
            new ImportReviewSummary(0, 0, 0),
            [new ImportReviewRow("drink", "Daiquiri", "skip", "Drink 'Daiquiri' is unchanged.", false, true)]));

        _importBatchRepository.GetByIdAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);
        _processingService.GetBatchApplyReadiness(batch).Returns(ImportBatchApplyReadiness.BlockedByValidationErrors);

        var result = await _handler.Handle(
            new ApplyImportBatchCommand(batch.Id),
            CancellationToken.None);

        result.WasApplied.ShouldBeFalse();
        result.ApplyReadiness.ShouldBe(nameof(ImportBatchApplyReadiness.BlockedByValidationErrors));
        result.Batch.ApplySummary.ShouldBeNull();
        await _processingService.DidNotReceive().ProcessAsync(Arg.Any<NormalizedCatalogImport>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBatchRequiresReviewAndHasNotBeenReviewed_ReturnsRequiresReviewResult()
    {
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []));
        batch.RecordPreparedSnapshot(new ImportBatchProcessingResult(
            [],
            new ImportReviewSummary(0, 1, 0),
            [new ImportReviewRow("drink", "Negroni", "update", "Update drink 'Negroni'.", true, false)]));

        _importBatchRepository.GetByIdAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);
        _processingService.GetBatchApplyReadiness(batch).Returns(ImportBatchApplyReadiness.RequiresReview);

        var result = await _handler.Handle(
            new ApplyImportBatchCommand(batch.Id),
            CancellationToken.None);

        result.WasApplied.ShouldBeFalse();
        result.ApplyReadiness.ShouldBe(nameof(ImportBatchApplyReadiness.RequiresReview));
        result.Batch.Status.ShouldBe(nameof(ImportBatchStatus.InProgress));
        await _processingService.DidNotReceive().ProcessAsync(Arg.Any<NormalizedCatalogImport>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSnapshotMissing_RebuildsPreparedSnapshotWithOneProcessingCall()
    {
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []));

        _importBatchRepository.GetByIdAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);
        _processingService.ProcessAsync(batch.ImportContent, Arg.Any<CancellationToken>())
            .Returns(new ImportBatchProcessingResult(
                [],
                new ImportReviewSummary(0, 1, 0),
                [new ImportReviewRow("drink", "Negroni", "update", "Update drink 'Negroni'.", true, false)]));
        _processingService.GetBatchApplyReadiness(batch).Returns(ImportBatchApplyReadiness.RequiresReview);

        var result = await _handler.Handle(
            new ApplyImportBatchCommand(batch.Id),
            CancellationToken.None);

        result.WasApplied.ShouldBeFalse();
        result.ApplyReadiness.ShouldBe(nameof(ImportBatchApplyReadiness.RequiresReview));
        result.Batch.ReviewSummary.ShouldNotBeNull();
        await _processingService.Received(1).ProcessAsync(batch.ImportContent, Arg.Any<CancellationToken>());
    }
}
