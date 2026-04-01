using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Data.Repositories;
using AlCopilot.DrinkCatalog.Domain.Aggregates;
using AlCopilot.DrinkCatalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class TagIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;

    public async Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Tags\";");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task CreateTag_Persists()
    {
        var repo = new TagRepository(_db);
        var tag = Tag.Create(TagName.Create("Refreshing"));
        repo.Add(tag);
        await _db.SaveChangesAsync();

        var all = await repo.GetAllAsync();
        all.ShouldContain(t => t.Name == "Refreshing");
    }

    [Fact]
    public async Task DuplicateTagName_ThrowsOnSave()
    {
        var repo = new TagRepository(_db);
        repo.Add(Tag.Create(TagName.Create("Unique")));
        await _db.SaveChangesAsync();

        repo.Add(Tag.Create(TagName.Create("Unique")));
        await Should.ThrowAsync<DbUpdateException>(() => _db.SaveChangesAsync());
    }

    [Fact]
    public async Task IsReferencedByDrinks_WhenReferenced_ReturnsTrue()
    {
        var repo = new TagRepository(_db);
        var tag = Tag.Create(TagName.Create("Referenced"));
        repo.Add(tag);

        var drink = Drink.Create(DrinkName.Create("RefDrink"), null, ImageUrl.Create(null));
        drink.SetTags([tag]);
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        (await repo.IsReferencedByDrinksAsync(tag.Id)).ShouldBeTrue();
    }
}

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class IngredientCategoryIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;

    public async Task InitializeAsync() => _db = fixture.CreateDbContext();

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"Ingredients\"; DELETE FROM drink_catalog.\"IngredientCategories\";");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task CreateCategory_Persists()
    {
        var repo = new IngredientCategoryRepository(_db);
        repo.Add(IngredientCategory.Create(CategoryName.Create("Mixers")));
        await _db.SaveChangesAsync();

        var all = await repo.GetAllAsync();
        all.ShouldContain(c => c.Name == "Mixers");
    }

    [Fact]
    public async Task GetAll_OrderedByName()
    {
        var repo = new IngredientCategoryRepository(_db);
        repo.Add(IngredientCategory.Create(CategoryName.Create("Zzz")));
        repo.Add(IngredientCategory.Create(CategoryName.Create("Aaa")));
        await _db.SaveChangesAsync();

        var all = await repo.GetAllAsync();
        all[0].Name.ShouldBe("Aaa");
    }
}

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class IngredientIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;
    private Guid _categoryId;

    public async Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        var category = IngredientCategory.Create(CategoryName.Create("TestCat"));
        _db.IngredientCategories.Add(category);
        _categoryId = category.Id;
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Ingredients\"; DELETE FROM drink_catalog.\"IngredientCategories\";");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task CreateIngredient_WithBrands_Persists()
    {
        var repo = new IngredientRepository(_db);
        repo.Add(Ingredient.Create(IngredientName.Create("Rum"), _categoryId, ["Bacardi", "Havana Club"]));
        await _db.SaveChangesAsync();

        var all = await repo.GetAllAsync();
        var rum = all.ShouldHaveSingleItem();
        rum.NotableBrands.ShouldBe(["Bacardi", "Havana Club"]);
    }

    [Fact]
    public async Task GetAll_FilterByCategory()
    {
        var repo = new IngredientRepository(_db);
        repo.Add(Ingredient.Create(IngredientName.Create("Filtered"), _categoryId));
        await _db.SaveChangesAsync();

        var filtered = await repo.GetAllAsync(_categoryId);
        filtered.ShouldNotBeEmpty();

        var other = await repo.GetAllAsync(Guid.NewGuid());
        other.ShouldBeEmpty();
    }
}

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class DomainEventIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;

    public async Task InitializeAsync() => _db = fixture.CreateDbContext();

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.domain_events;");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task CreateDrink_PersistsDrinkCreatedEvent()
    {
        var drink = Drink.Create(DrinkName.Create("EventTest"), null, ImageUrl.Create(null));
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        var events = await _db.DomainEventRecords.ToListAsync();
        events.ShouldContain(e => e.AggregateId == drink.Id && e.EventType.Contains("DrinkCreated"));
    }

    [Fact]
    public async Task SoftDeleteDrink_PersistsDrinkDeletedEvent()
    {
        var drink = Drink.Create(DrinkName.Create("DeleteEventTest"), null, ImageUrl.Create(null));
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        drink.SoftDelete();
        await _db.SaveChangesAsync();

        var events = await _db.DomainEventRecords
            .Where(e => e.AggregateId == drink.Id)
            .ToListAsync();
        events.ShouldContain(e => e.EventType.Contains("DrinkDeleted"));
    }
}
