using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.ImportSync;
using AlCopilot.DrinkCatalog.Features.ImportSync.Strategies;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class DomainEventRecordIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;

    public async Task InitializeAsync() => _db = fixture.CreateDbContext();

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"ImportBatches\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Tags\"; DELETE FROM drink_catalog.\"Ingredients\"; DELETE FROM drink_catalog.domain_events;");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task CreateDrink_PersistsDrinkCreatedEvent()
    {
        var drink = Drink.Create(DrinkName.Create("EventTest"), DrinkCategory.Create(null), null, null, null, ImageUrl.Create(null));
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        var events = await _db.DomainEventRecords.ToListAsync();
        events.ShouldContain(e => e.AggregateId == drink.Id && e.EventType == "drink-catalog.drink-created.v1");
    }

    [Fact]
    public async Task SoftDeleteDrink_PersistsDrinkDeletedEvent()
    {
        var drink = Drink.Create(DrinkName.Create("DeleteEventTest"), DrinkCategory.Create(null), null, null, null, ImageUrl.Create(null));
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        drink.SoftDelete();
        await _db.SaveChangesAsync();

        var events = await _db.DomainEventRecords
            .Where(e => e.AggregateId == drink.Id)
            .ToListAsync();
        events.ShouldContain(e => e.EventType == "drink-catalog.drink-deleted.v1");
    }

    [Fact]
    public async Task CreateTag_PersistsTagCreatedEvent()
    {
        var tag = Tag.Create(TagName.Create("Classic"));
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();

        var events = await _db.DomainEventRecords.ToListAsync();
        events.ShouldContain(e => e.AggregateId == tag.Id && e.EventType == "drink-catalog.tag-created.v1");
    }

    [Fact]
    public async Task UpdateIngredient_PersistsIngredientUpdatedEvent()
    {
        var ingredient = Ingredient.Create(IngredientName.Create("Vodka"), ["Absolut"]);
        _db.Ingredients.Add(ingredient);
        await _db.SaveChangesAsync();

        ingredient.ClearDomainEvents();
        ingredient.Update(IngredientName.Create("Premium Vodka"), ["Belvedere"]);
        await _db.SaveChangesAsync();

        var events = await _db.DomainEventRecords.Where(e => e.AggregateId == ingredient.Id).ToListAsync();
        events.ShouldContain(e => e.EventType == "drink-catalog.ingredient-updated.v1");
    }

    [Fact]
    public async Task CancelImportBatch_PersistsImportBatchCancelledEvent()
    {
        var batch = ImportBatch.Create(
            ImportStrategyKey.IbaCocktailsSnapshot,
            ImportProvenance.Empty,
            new NormalizedCatalogImport([], [], []));
        _db.ImportBatches.Add(batch);
        await _db.SaveChangesAsync();

        batch.ClearDomainEvents();
        batch.MarkCancelled();
        _db.ImportBatches.Update(batch);
        await _db.SaveChangesAsync();

        var events = await _db.DomainEventRecords.Where(e => e.AggregateId == batch.Id).ToListAsync();
        events.ShouldContain(e => e.EventType == "drink-catalog.import-batch-cancelled.v1");
    }
}
