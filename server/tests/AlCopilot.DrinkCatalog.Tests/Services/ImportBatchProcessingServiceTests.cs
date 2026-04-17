using AlCopilot.DrinkCatalog.Contracts.DTOs;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.ImportBatch;
using AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Services;

public sealed class ImportBatchProcessingServiceTests
{
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();
    private readonly IDrinkQueryService _drinkQueryService = Substitute.For<IDrinkQueryService>();

    [Fact]
    public async Task ProcessAsync_WhenImportContainsCreatesUpdatesAndErrors_ReturnsCoherentSnapshot()
    {
        var existingIngredient = Ingredient.Create(IngredientName.Create("Campari"), ["Old Brand"]);

        _tagRepository.GetByNameAsync("Classic", Arg.Any<CancellationToken>())
            .Returns(Tag.Create(TagName.Create("Classic")));
        _tagRepository.GetByNameAsync("Modern", Arg.Any<CancellationToken>())
            .Returns((Tag?)null);

        _ingredientRepository.GetByNameAsync("Campari", Arg.Any<CancellationToken>())
            .Returns(existingIngredient);
        _ingredientRepository.GetByNameAsync("Missing Ingredient", Arg.Any<CancellationToken>())
            .Returns((Ingredient?)null);

        _drinkQueryService.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(
            [
                new DrinkDetailDto(
                    Guid.NewGuid(),
                    "Negroni",
                    "Contemporary Classics",
                    "Original description",
                    "Stirred",
                    "Orange peel",
                    null,
                    [new TagDto(Guid.NewGuid(), "Classic", 1)],
                    [new RecipeEntryDto(new IngredientDto(Guid.NewGuid(), "Campari", []), "1 oz", null)])
            ]);

        var service = new ImportBatchProcessingService(
            _tagRepository,
            _ingredientRepository,
            _drinkQueryService);

        var result = await service.ProcessAsync(
            new NormalizedCatalogImport(
                [new NormalizedTagImport("Classic"), new NormalizedTagImport("Modern")],
                [new NormalizedIngredientImport("Campari", ["New Brand"])],
                [
                    new NormalizedDrinkImport(
                        "Negroni",
                        "The Unforgettables",
                        "Updated description",
                        "Stirred",
                        "Orange peel",
                        null,
                        ["Classic"],
                        [new NormalizedDrinkRecipeEntryImport("Campari", "1 oz", null)]),
                    new NormalizedDrinkImport(
                        "Mystery",
                        "Contemporary Classics",
                        null,
                        null,
                        null,
                        null,
                        [],
                        [new NormalizedDrinkRecipeEntryImport("Missing Ingredient", "1 oz", null)])
                ]),
            CancellationToken.None);

        result.Diagnostics.ShouldHaveSingleItem().Code.ShouldBe("recipe-ingredient-missing");
        result.ReviewSummary.CreateCount.ShouldBe(2);
        result.ReviewSummary.UpdateCount.ShouldBe(2);
        result.ReviewSummary.SkipCount.ShouldBe(1);
        result.ReviewRows.ShouldContain(r => r.TargetType == "tag" && r.TargetKey == "Modern" && r.Action == "create");
        result.ReviewRows.ShouldContain(r => r.TargetType == "ingredient" && r.TargetKey == "Campari" && r.Action == "update");
        result.ReviewRows.ShouldContain(r => r.TargetType == "drink" && r.TargetKey == "Negroni" && r.Action == "update");
        result.ReviewRows.ShouldContain(r => r.TargetType == "drink" && r.TargetKey == "Mystery" && r.HasError);
    }

    [Fact]
    public void GetBatchApplyReadiness_WhenBatchRequiresReview_ReturnsBatchReadiness()
    {
        var service = new ImportBatchProcessingService(
            _tagRepository,
            _ingredientRepository,
            _drinkQueryService);
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []));
        batch.RecordPreparedSnapshot(new ImportBatchProcessingResult(
            [],
            new ImportReviewSummary(0, 1, 0),
            [new ImportReviewRow("drink", "Negroni", "update", "Update drink 'Negroni'.", true, false)]));

        var readiness = service.GetBatchApplyReadiness(batch);

        readiness.ShouldBe(ImportBatchApplyReadiness.RequiresReview);
    }
}
