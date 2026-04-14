using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class IngredientRepositoryIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;

    public async Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Ingredients\";");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task CreateIngredient_WithBrands_Persists()
    {
        var repo = new IngredientRepository(_db);
        repo.Add(Ingredient.Create(IngredientName.Create("Rum"), ["Bacardi", "Havana Club"]));
        await _db.SaveChangesAsync();

        var all = await new IngredientQueryService(_db).GetAllAsync();
        var rum = all.ShouldHaveSingleItem();
        rum.NotableBrands.ShouldBe(["Bacardi", "Havana Club"]);
    }

}
