using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.ImportBatch;
using AlCopilot.DrinkCatalog.Features.ImportBatch.Strategies;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using NSubstitute;
using Shouldly;

namespace AlCopilot.DrinkCatalog.UnitTests.Services;

public sealed class ImportBatchApplyServiceTests
{
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();
    private readonly IDrinkRepository _drinkRepository = Substitute.For<IDrinkRepository>();

    [Fact]
    public async Task ApplyAsync_WhenBatchContainsCreateOnlyChanges_CreatesEntitiesAndMarksBatchCompleted()
    {
        _tagRepository.GetByNameAsync("Classic", Arg.Any<CancellationToken>())
            .Returns((Tag?)null);
        _ingredientRepository.GetByNameAsync("Gin", Arg.Any<CancellationToken>())
            .Returns((Ingredient?)null);

        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport(
                [new NormalizedTagImport("Classic")],
                [new NormalizedIngredientImport("Gin", [])],
                []));

        var service = new ImportBatchApplyService(
            _tagRepository,
            _ingredientRepository,
            _drinkRepository);

        var summary = await service.ApplyAsync(batch, CancellationToken.None);

        summary.CreatedCount.ShouldBe(2);
        summary.UpdatedCount.ShouldBe(0);
        summary.SkippedCount.ShouldBe(0);
        batch.Status.ShouldBe(ImportBatchStatus.Completed);
        _tagRepository.Received(1).Add(Arg.Any<Tag>());
        _ingredientRepository.Received(1).Add(Arg.Any<Ingredient>());
    }

    [Fact]
    public async Task ApplyAsync_WhenBatchContainsReviewedUpdates_UpdatesExistingEntities()
    {
        var existingIngredient = Ingredient.Create(IngredientName.Create("Gin"), ["Old Brand"]);
        var existingTag = Tag.Create(TagName.Create("Classic"));
        var existingDrink = Drink.Create(
            DrinkName.Create("Negroni"),
            DrinkCategory.Create("Contemporary Classics"),
            "Original description",
            "Stirred",
            "Orange peel",
            ImageUrl.Create(null));
        existingDrink.SetTags([existingTag]);
        existingDrink.SetRecipeEntries([RecipeEntry.Create(existingDrink.Id, existingIngredient.Id, Quantity.Create("1 oz"), null)]);

        _ingredientRepository.GetByNameAsync("Gin", Arg.Any<CancellationToken>())
            .Returns(existingIngredient);
        _drinkRepository.GetByNameAsync("Negroni", Arg.Any<CancellationToken>())
            .Returns(existingDrink);
        _tagRepository.GetByNameAsync("Classic", Arg.Any<CancellationToken>())
            .Returns(existingTag);

        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport(
                [],
                [new NormalizedIngredientImport("Gin", [])],
                [
                    new NormalizedDrinkImport(
                        "Negroni",
                        "The Unforgettables",
                        "Updated description",
                        "Build",
                        "Orange twist",
                        null,
                        ["Classic"],
                        [new NormalizedDrinkRecipeEntryImport("Gin", "1.5 oz", null)])
                ]));
        batch.RecordReviewedSnapshot(new ImportBatchProcessingResult(
            [],
            new ImportReviewSummary(0, 2, 0),
            [
                new ImportReviewRow("ingredient", "Gin", "update", "Ingredient 'Gin' would update notable brands.", true, false),
                new ImportReviewRow("drink", "Negroni", "update", "Drink 'Negroni' would update metadata, tags, or recipe entries.", true, false)
            ]));

        var service = new ImportBatchApplyService(
            _tagRepository,
            _ingredientRepository,
            _drinkRepository);

        var summary = await service.ApplyAsync(batch, CancellationToken.None);

        summary.CreatedCount.ShouldBe(0);
        summary.UpdatedCount.ShouldBe(2);
        summary.SkippedCount.ShouldBe(0);
        batch.Status.ShouldBe(ImportBatchStatus.Completed);
        existingIngredient.NotableBrands.ShouldBeEmpty();
        existingDrink.Category.Value.ShouldBe("The Unforgettables");
        existingDrink.Description.ShouldBe("Updated description");
        existingDrink.Method.ShouldBe("Build");
        existingDrink.Garnish.ShouldBe("Orange twist");
        existingDrink.RecipeEntries.ShouldHaveSingleItem().Quantity.Value.ShouldBe("1.5 oz");
    }
}
