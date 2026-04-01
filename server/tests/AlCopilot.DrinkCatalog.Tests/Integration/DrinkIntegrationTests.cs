using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Data.Repositories;
using AlCopilot.DrinkCatalog.Domain.Aggregates;
using AlCopilot.DrinkCatalog.Domain.ValueObjects;
using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class DrinkBrowseIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;

    public async Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        await SeedDataAsync();
    }

    public async Task DisposeAsync()
    {
        // Clean up seeded data
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Tags\"; DELETE FROM drink_catalog.\"Ingredients\"; DELETE FROM drink_catalog.\"IngredientCategories\";");
        await _db.DisposeAsync();
    }

    private async Task SeedDataAsync()
    {
        var tag1 = Tag.Create(TagName.Create("Classic"));
        var tag2 = Tag.Create(TagName.Create("Strong"));
        _db.Tags.Add(tag1);
        _db.Tags.Add(tag2);

        var drink1 = Drink.Create(DrinkName.Create("Margarita"), "A tequila classic", ImageUrl.Create(null));
        drink1.SetTags([tag1]);
        var drink2 = Drink.Create(DrinkName.Create("Old Fashioned"), "Whiskey cocktail", ImageUrl.Create(null));
        drink2.SetTags([tag1, tag2]);
        var deleted = Drink.Create(DrinkName.Create("Deleted Drink"), null, ImageUrl.Create(null));
        deleted.SoftDelete();

        _db.Drinks.Add(drink1);
        _db.Drinks.Add(drink2);
        _db.Drinks.Add(deleted);
        await _db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetPaged_ReturnsAllActiveDrinks()
    {
        var repo = new DrinkRepository(_db);
        var result = await repo.GetPagedAsync(null, 1, 20);

        result.TotalCount.ShouldBe(2);
        result.Items.ShouldNotContain(d => d.Name == "Deleted Drink");
    }

    [Fact]
    public async Task GetPaged_FilterByTag_ReturnsMatchingDrinks()
    {
        var tag = await _db.Tags.FirstAsync(t => t.Name == TagName.Create("Strong"));
        var repo = new DrinkRepository(_db);
        var result = await repo.GetPagedAsync([tag.Id], 1, 20);

        result.TotalCount.ShouldBe(1);
        result.Items[0].Name.ShouldBe("Old Fashioned");
    }

    [Fact]
    public async Task GetPaged_Pagination_ReturnsCorrectPage()
    {
        var repo = new DrinkRepository(_db);
        var result = await repo.GetPagedAsync(null, 1, 1);

        result.TotalCount.ShouldBe(2);
        result.Items.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetPaged_FilterByMultipleTags_ReturnsUnionOrLogic()
    {
        // Arrange: add a third tag and a drink that has ONLY that tag
        var tag3 = Tag.Create(TagName.Create("Fruity"));
        _db.Tags.Add(tag3);
        var drink3 = Drink.Create(DrinkName.Create("Daiquiri"), "Rum and lime", ImageUrl.Create(null));
        drink3.SetTags([tag3]);
        _db.Drinks.Add(drink3);
        await _db.SaveChangesAsync();

        // tag "Strong" is only on Old Fashioned, tag "Fruity" is only on Daiquiri
        var tagStrong = await _db.Tags.FirstAsync(t => t.Name == TagName.Create("Strong"));
        var repo = new DrinkRepository(_db);

        // Act: filter by both Strong and Fruity — OR should return both, AND would return neither
        var result = await repo.GetPagedAsync([tagStrong.Id, tag3.Id], 1, 20);

        // Assert
        result.TotalCount.ShouldBe(2);
        result.Items.Select(d => d.Name).ShouldBe(
            ["Daiquiri", "Old Fashioned"],
            ignoreOrder: true);
    }
}

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class DrinkSearchIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;

    public async Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        var category = IngredientCategory.Create(CategoryName.Create("Spirits"));
        _db.IngredientCategories.Add(category);
        var ingredient = Ingredient.Create(IngredientName.Create("Tequila"), category.Id, ["Patron"]);
        _db.Ingredients.Add(ingredient);

        var tag = Tag.Create(TagName.Create("Tropical"));
        _db.Tags.Add(tag);

        var drink = Drink.Create(DrinkName.Create("Margarita"), "A tequila classic", ImageUrl.Create(null));
        drink.SetTags([tag]);
        drink.SetRecipeEntries([RecipeEntry.Create(drink.Id, ingredient.Id, Quantity.Create("2 oz"), null)]);
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Tags\"; DELETE FROM drink_catalog.\"Ingredients\"; DELETE FROM drink_catalog.\"IngredientCategories\";");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task Search_ByName_FindsDrink()
    {
        var repo = new DrinkRepository(_db);
        var result = await repo.SearchAsync("marg", 1, 20);
        result.TotalCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Search_NoResults_ReturnsEmpty()
    {
        var repo = new DrinkRepository(_db);
        var result = await repo.SearchAsync("nonexistent", 1, 20);
        result.TotalCount.ShouldBe(0);
    }
}

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class DrinkDetailIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;
    private Guid _drinkId;

    public async Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        var category = IngredientCategory.Create(CategoryName.Create("Citrus"));
        _db.IngredientCategories.Add(category);
        var ingredient = Ingredient.Create(IngredientName.Create("Lime Juice"), category.Id, ["Nellie & Joe's"]);
        _db.Ingredients.Add(ingredient);

        var drink = Drink.Create(DrinkName.Create("Gimlet"), "Gin and lime", ImageUrl.Create(null));
        drink.SetRecipeEntries([RecipeEntry.Create(drink.Id, ingredient.Id, Quantity.Create("1 oz"), "Fresh")]);
        _db.Drinks.Add(drink);
        _drinkId = drink.Id;
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Tags\"; DELETE FROM drink_catalog.\"Ingredients\"; DELETE FROM drink_catalog.\"IngredientCategories\";");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task GetDetailById_ReturnsFullDetail()
    {
        var repo = new DrinkRepository(_db);
        var detail = await repo.GetDetailByIdAsync(_drinkId);

        detail.ShouldNotBeNull();
        detail.Name.ShouldBe("Gimlet");
        detail.RecipeEntries.ShouldNotBeEmpty();
        detail.RecipeEntries[0].Ingredient.Name.ShouldBe("Lime Juice");
        detail.RecipeEntries[0].Ingredient.NotableBrands.ShouldContain("Nellie & Joe's");
    }

    [Fact]
    public async Task GetDetailById_NotFound_ReturnsNull()
    {
        var repo = new DrinkRepository(_db);
        var detail = await repo.GetDetailByIdAsync(Guid.NewGuid());
        detail.ShouldBeNull();
    }
}

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>;
