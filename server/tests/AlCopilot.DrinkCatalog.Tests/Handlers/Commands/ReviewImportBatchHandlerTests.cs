using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
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

public sealed class ReviewImportBatchHandlerTests
{
    private readonly IImportBatchRepository _importBatchRepository = Substitute.For<IImportBatchRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();
    private readonly IDrinkQueryService _drinkQueryService = Substitute.For<IDrinkQueryService>();
    private readonly IImportBatchProcessingService _processingService = Substitute.For<IImportBatchProcessingService>();
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IDrinkCatalogUnitOfWork _unitOfWork = Substitute.For<IDrinkCatalogUnitOfWork>();
    private readonly ReviewImportBatchHandler _handler;

    public ReviewImportBatchHandlerTests()
    {
        _handler = new ReviewImportBatchHandler(
            _importBatchRepository,
            _processingService,
            new AuditLogWriter(_auditRepository),
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenExistingDrinkDiffers_MarksRowAsRequiringReview()
    {
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport(
                [],
                [],
                [
                    new NormalizedDrinkImport(
                        "Negroni",
                        "The Unforgettables",
                        null,
                        "Shaken",
                        "Orange peel",
                        null,
                        ["Classic"],
                        [new NormalizedDrinkRecipeEntryImport("Gin", "1 oz", null)])
                ]));
        batch.RecordValidation([]);

        _importBatchRepository.GetByIdAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);
        _drinkQueryService.GetAllAsync(Arg.Any<CancellationToken>()).Returns(
        [
            new DrinkDetailDto(
                Guid.NewGuid(),
                "Negroni",
                "Contemporary Classics",
                "Original",
                "Stirred",
                "Orange peel",
                null,
                [new TagDto(Guid.NewGuid(), "Classic", 1)],
                [new RecipeEntryDto(
                    new IngredientDto(Guid.NewGuid(), "Gin", []),
                    "1 oz",
                    null)])
        ]);
        _processingService.ProcessAsync(batch.ImportContent, Arg.Any<CancellationToken>())
            .Returns(new ImportBatchProcessingResult(
                [],
                new ImportReviewSummary(0, 1, 0),
                [new ImportReviewRow("drink", "Negroni", "update", "Drink 'Negroni' would update metadata, tags, or recipe entries.", true, false)]));

        var result = await _handler.Handle(new ReviewImportBatchCommand(batch.Id), CancellationToken.None);

        result.Status.ShouldBe(nameof(ImportBatchStatus.InProgress));
        result.RequiresReview.ShouldBeTrue();
        result.ReviewedAtUtc.ShouldNotBeNull();
        result.ReviewSummary!.UpdateCount.ShouldBe(1);
        result.ReviewRows.ShouldContain(r => r.TargetType == "drink" && r.TargetKey == "Negroni" && r.RequiresReview);
        await _processingService.Received(1).ProcessAsync(batch.ImportContent, Arg.Any<CancellationToken>());
    }
}
