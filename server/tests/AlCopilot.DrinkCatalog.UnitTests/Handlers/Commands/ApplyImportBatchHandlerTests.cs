using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.DrinkCatalog.Features.ImportBatch;
using AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Recommendation.Contracts.Commands;
using AlCopilot.Recommendation.Contracts.DTOs;
using AlCopilot.Shared.Data;
using Mediator;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.UnitTests.Handlers.Commands;

public sealed class ApplyImportBatchHandlerTests
{
    private readonly IImportBatchRepository _importBatchRepository = Substitute.For<IImportBatchRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly IDrinkQueryService _drinkQueryService = Substitute.For<IDrinkQueryService>();
    private readonly IImportBatchProcessingService _processingService = Substitute.For<IImportBatchProcessingService>();
    private readonly IImportBatchApplyService _applyService = Substitute.For<IImportBatchApplyService>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IDrinkCatalogUnitOfWork _unitOfWork = Substitute.For<IDrinkCatalogUnitOfWork>();
    private readonly ApplyImportBatchHandler _handler;

    public ApplyImportBatchHandlerTests()
    {
        _handler = new ApplyImportBatchHandler(
            _importBatchRepository,
            _processingService,
            _applyService,
            _drinkQueryService,
            _mediator,
            new AuditLogWriter(_auditRepository),
            _unitOfWork);

        _mediator.Send(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new RecommendationSemanticCatalogIndexResultDto(0, 0));
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

    [Fact]
    public async Task Handle_WhenBatchIsApplied_SavesBeforeReadingCatalogForSemanticRebuild()
    {
        var calls = new List<string>();
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []));
        batch.RecordPreparedSnapshot(new ImportBatchProcessingResult(
            [],
            new ImportReviewSummary(0, 0, 0),
            [new ImportReviewRow("drink", "French 75", "create", "Create drink 'French 75'.", false, false)]));
        var indexedCatalog = new List<DrinkDetailDto>
        {
            new(
                Guid.Parse("00000000-0000-0000-0000-000000000701"),
                "French 75",
                null,
                "Sparkling, bright, and lightly sweet.",
                null,
                null,
                null,
                [],
                []),
        };

        _importBatchRepository.GetByIdAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);
        _processingService.GetBatchApplyReadiness(batch).Returns(ImportBatchApplyReadiness.Ready);
        _applyService.ApplyAsync(batch, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                calls.Add("apply");
                var summary = new ImportApplySummary(1, 0, 0, 0);
                batch.MarkCompleted(summary);
                return summary;
            });
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                calls.Add("save");
                return 1;
            });
        _drinkQueryService.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                calls.Add("read-catalog");
                return indexedCatalog;
            });
        _mediator.Send(Arg.Any<ReplaceRecommendationSemanticCatalogCommand>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                calls.Add("replace-semantic");
                return new RecommendationSemanticCatalogIndexResultDto(1, 1);
            });

        var result = await _handler.Handle(
            new ApplyImportBatchCommand(batch.Id),
            CancellationToken.None);

        result.WasApplied.ShouldBeTrue();
        calls.ShouldBe(["apply", "save", "read-catalog", "replace-semantic"]);
        await _mediator.Received(1).Send(
            Arg.Is<ReplaceRecommendationSemanticCatalogCommand>(command => command.Drinks == indexedCatalog),
            Arg.Any<CancellationToken>());
    }
}
