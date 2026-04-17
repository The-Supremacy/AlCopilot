using AlCopilot.DrinkCatalog.Contracts.Events;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.ImportSync;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.DrinkCatalog.Features.Tag;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Domain.Aggregates;

public sealed class TagTests
{
    [Fact]
    public void Create_SetsPropertiesAndRaisesEvent()
    {
        var tag = Tag.Create(TagName.Create("Classic"));

        tag.Id.ShouldNotBe(Guid.Empty);
        tag.Name.Value.ShouldBe("Classic");
        tag.CreatedAtUtc.ShouldNotBe(default);
        tag.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<TagCreatedEvent>();
    }

    [Fact]
    public void Rename_UpdatesNameAndRaisesEvent()
    {
        var tag = Tag.Create(TagName.Create("Classic"));
        tag.ClearDomainEvents();

        tag.Rename(TagName.Create("Modern"));

        tag.Name.Value.ShouldBe("Modern");
        tag.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<TagRenamedEvent>();
    }
}

public sealed class IngredientTests
{
    [Fact]
    public void Create_SetsPropertiesAndRaisesEvent()
    {
        var ingredient = Ingredient.Create(
            IngredientName.Create("Tequila"), ["Patron", "Don Julio"]);

        ingredient.Id.ShouldNotBe(Guid.Empty);
        ingredient.Name.Value.ShouldBe("Tequila");
        ingredient.NotableBrands.ShouldBe(["Patron", "Don Julio"]);
        ingredient.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<IngredientCreatedEvent>();
    }

    [Fact]
    public void Update_ReplacesNameAndBrandsAndRaisesEvent()
    {
        var ingredient = Ingredient.Create(
            IngredientName.Create("Vodka"), ["Absolut"]);
        ingredient.ClearDomainEvents();

        ingredient.Update(IngredientName.Create("Premium Vodka"), ["Grey Goose", "Belvedere"]);

        ingredient.Name.Value.ShouldBe("Premium Vodka");
        ingredient.NotableBrands.ShouldBe(["Grey Goose", "Belvedere"]);
        ingredient.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<IngredientUpdatedEvent>();
    }
}

public sealed class ImportBatchTests
{
    [Fact]
    public void Create_RaisesInitializedEvent()
    {
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []));

        batch.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ImportBatchInitializedEvent>();
    }

    [Fact]
    public void RecordPreparedSnapshot_RaisesPreparedEvent()
    {
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []));
        batch.ClearDomainEvents();

        batch.RecordPreparedSnapshot(new ImportBatchProcessingResult(
            [],
            new ImportReviewSummary(0, 0, 0),
            []));

        batch.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ImportBatchPreparedEvent>();
    }

    [Fact]
    public void RecordReviewedSnapshot_RaisesReviewedEvent()
    {
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []));
        batch.ClearDomainEvents();

        batch.RecordReviewedSnapshot(new ImportBatchProcessingResult(
            [],
            new ImportReviewSummary(0, 1, 0),
            [new ImportReviewRow("drink", "Negroni", "update", "Update drink.", true, false)]));

        batch.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ImportBatchReviewedEvent>();
    }

    [Fact]
    public void MarkCompleted_RaisesCompletedEvent()
    {
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []));
        batch.ClearDomainEvents();

        batch.MarkCompleted(new ImportApplySummary(1, 0, 0, 0));

        batch.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ImportBatchCompletedEvent>();
    }

    [Fact]
    public void MarkCancelled_RaisesCancelledEvent()
    {
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []));
        batch.ClearDomainEvents();

        batch.MarkCancelled();

        batch.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ImportBatchCancelledEvent>();
    }
}
