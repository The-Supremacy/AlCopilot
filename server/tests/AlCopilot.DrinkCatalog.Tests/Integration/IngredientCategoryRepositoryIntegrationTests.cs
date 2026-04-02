using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Features.IngredientCategory;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class IngredientCategoryRepositoryIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
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
