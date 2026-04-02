using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.IngredientCategory;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class IngredientRepositoryIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
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
