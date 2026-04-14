using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Tag;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class TagRepositoryIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
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

        var all = await new TagQueryService(_db).GetAllAsync();
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

        var drink = Drink.Create(DrinkName.Create("RefDrink"), DrinkCategory.Create(null), null, null, null, ImageUrl.Create(null));
        drink.SetTags([tag]);
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        (await repo.IsReferencedByDrinksAsync(tag.Id)).ShouldBeTrue();
    }
}
