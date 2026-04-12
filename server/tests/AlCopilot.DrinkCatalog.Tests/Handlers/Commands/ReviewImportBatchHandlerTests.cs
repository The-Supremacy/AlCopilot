using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Contracts.DTOs;
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
    private readonly IAuditLogEntryRepository _auditRepository = Substitute.For<IAuditLogEntryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ReviewImportBatchHandler _handler;

    public ReviewImportBatchHandlerTests()
    {
        var workflowService = new ImportBatchWorkflowService(
            _tagRepository,
            _ingredientRepository,
            _drinkRepository);
        _handler = new ReviewImportBatchHandler(
            _importBatchRepository,
            workflowService,
            new AuditLogWriter(_auditRepository),
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenExistingDrinkDiffers_AddsReviewConflict()
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
                ]),
            "sha256:test");
        batch.RecordValidation([]);

        _importBatchRepository.GetByIdAsync(batch.Id, Arg.Any<CancellationToken>()).Returns(batch);
        _drinkRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(
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

        var result = await _handler.Handle(new ReviewImportBatchCommand(batch.Id), CancellationToken.None);

        result.Status.ShouldBe(nameof(ImportBatchStatus.InProgress));
        result.ReviewConflicts.ShouldContain(c => c.TargetType == "drink" && c.TargetKey == "Negroni");
        result.ReviewSummary!.UpdateCount.ShouldBe(1);
        result.ReviewRows.ShouldContain(r => r.TargetType == "drink" && r.TargetKey == "Negroni" && r.HasConflict);
    }
}
