using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Contracts.Queries;
using AlCopilot.DrinkCatalog.Features.Drink;
using AlCopilot.DrinkCatalog.Features.Ingredient;
using AlCopilot.DrinkCatalog.Features.Tag;
using AlCopilot.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class DrinkQueryServiceBrowseTests(PostgresFixture fixture) : IAsyncLifetime
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
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Tags\"; DELETE FROM drink_catalog.\"Ingredients\";");
        await _db.DisposeAsync();
    }

    private async Task SeedDataAsync()
    {
        var tag1 = Tag.Create(TagName.Create("Classic"));
        var tag2 = Tag.Create(TagName.Create("Strong"));
        _db.Tags.Add(tag1);
        _db.Tags.Add(tag2);

        var drink1 = Drink.Create(DrinkName.Create("Margarita"), DrinkCategory.Create("The Unforgettables"), "A tequila classic", "Shake", "Salt rim", ImageUrl.Create(null));
        drink1.SetTags([tag1]);
        var drink2 = Drink.Create(DrinkName.Create("Old Fashioned"), DrinkCategory.Create("The Unforgettables"), "Whiskey cocktail", "Stir", "Orange peel", ImageUrl.Create(null));
        drink2.SetTags([tag1, tag2]);
        var deleted = Drink.Create(DrinkName.Create("Deleted Drink"), DrinkCategory.Create(null), null, null, null, ImageUrl.Create(null));
        deleted.SoftDelete();

        _db.Drinks.Add(drink1);
        _db.Drinks.Add(drink2);
        _db.Drinks.Add(deleted);
        await _db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetPaged_ReturnsAllActiveDrinks()
    {
        var queryService = new DrinkQueryService(_db);
        var result = await queryService.GetPagedAsync(new DrinkFilter(null, null, 1, 20));

        result.TotalCount.ShouldBe(2);
        result.Items.ShouldNotContain(d => d.Name == "Deleted Drink");
    }

    [Fact]
    public async Task GetPaged_FilterByTag_ReturnsMatchingDrinks()
    {
        var tag = await _db.Tags.FirstAsync(t => t.Name == TagName.Create("Strong"));
        var queryService = new DrinkQueryService(_db);
        var result = await queryService.GetPagedAsync(new DrinkFilter(null, [tag.Id], 1, 20));

        result.TotalCount.ShouldBe(1);
        result.Items[0].Name.ShouldBe("Old Fashioned");
    }

    [Fact]
    public async Task GetPaged_Pagination_ReturnsCorrectPage()
    {
        var queryService = new DrinkQueryService(_db);
        var result = await queryService.GetPagedAsync(new DrinkFilter(null, null, 1, 1));

        result.TotalCount.ShouldBe(2);
        result.Items.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetPaged_FilterByMultipleTags_ReturnsUnionOrLogic()
    {
        // Arrange: add a third tag and a drink that has ONLY that tag
        var tag3 = Tag.Create(TagName.Create("Fruity"));
        _db.Tags.Add(tag3);
        var drink3 = Drink.Create(DrinkName.Create("Daiquiri"), DrinkCategory.Create("Contemporary Classics"), "Rum and lime", "Shake", "Lime", ImageUrl.Create(null));
        drink3.SetTags([tag3]);
        _db.Drinks.Add(drink3);
        await _db.SaveChangesAsync();

        // tag "Strong" is only on Old Fashioned, tag "Fruity" is only on Daiquiri
        var tagStrong = await _db.Tags.FirstAsync(t => t.Name == TagName.Create("Strong"));
        var queryService = new DrinkQueryService(_db);

        // Act: filter by both Strong and Fruity — OR should return both, AND would return neither
        var result = await queryService.GetPagedAsync(new DrinkFilter(null, [tagStrong.Id, tag3.Id], 1, 20));

        // Assert
        result.TotalCount.ShouldBe(2);
        result.Items.Select(d => d.Name).ShouldBe(
            ["Daiquiri", "Old Fashioned"],
            ignoreOrder: true);
    }
}

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class DrinkQueryServiceFilterTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;

    public async Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        var ingredient = Ingredient.Create(IngredientName.Create("Tequila"), ["Patron"]);
        _db.Ingredients.Add(ingredient);

        var tag = Tag.Create(TagName.Create("Tropical"));
        _db.Tags.Add(tag);

        var drink = Drink.Create(DrinkName.Create("Margarita"), DrinkCategory.Create("The Unforgettables"), "A tequila classic", "Shake", "Salt rim", ImageUrl.Create(null));
        drink.SetTags([tag]);
        drink.SetRecipeEntries([RecipeEntry.Create(drink.Id, ingredient.Id, Quantity.Create("2 oz"), null)]);
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Tags\"; DELETE FROM drink_catalog.\"Ingredients\";");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task GetPaged_WithQuery_FindsDrink()
    {
        var queryService = new DrinkQueryService(_db);
        var result = await queryService.GetPagedAsync(new DrinkFilter("marg", null, 1, 20));
        result.TotalCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetPaged_WithQueryAndTagFilter_ReturnsMatchingDrink()
    {
        var tag = await _db.Tags.SingleAsync();
        var queryService = new DrinkQueryService(_db);
        var result = await queryService.GetPagedAsync(new DrinkFilter("marg", [tag.Id], 1, 20));

        result.TotalCount.ShouldBe(1);
        result.Items[0].Name.ShouldBe("Margarita");
    }

    [Fact]
    public async Task GetPaged_WithQuery_NoResults_ReturnsEmpty()
    {
        var queryService = new DrinkQueryService(_db);
        var result = await queryService.GetPagedAsync(new DrinkFilter("nonexistent", null, 1, 20));
        result.TotalCount.ShouldBe(0);
    }
}

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class DrinkQueryServiceFuzzyMatchTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;

    public async Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();

        _db.Ingredients.Add(Ingredient.Create(IngredientName.Create("Tequila"), ["Patron"]));
        _db.Ingredients.Add(Ingredient.Create(IngredientName.Create("Gin"), ["Tanqueray"]));

        var margarita = Drink.Create(
            DrinkName.Create("Margarita"),
            DrinkCategory.Create("The Unforgettables"),
            "A tequila classic",
            "Shake",
            "Salt rim",
            ImageUrl.Create(null));
        var deletedMargarita = Drink.Create(
            DrinkName.Create("Margarita Deleted"),
            DrinkCategory.Create(null),
            null,
            null,
            null,
            ImageUrl.Create(null));
        deletedMargarita.SoftDelete();

        _db.Drinks.Add(margarita);
        _db.Drinks.Add(deletedMargarita);
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Ingredients\";");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task FindFuzzyDrinkMatches_TranslatesAndReturnsActiveMatches()
    {
        var queryService = new DrinkQueryService(_db);

        var matches = await queryService.FindFuzzyDrinkMatchesAsync("Margarita");

        var match = matches.ShouldHaveSingleItem();
        match.DrinkName.ShouldBe("Margarita");
        match.Similarity.ShouldBeGreaterThanOrEqualTo(0.30d);
    }

    [Fact]
    public async Task FindFuzzyIngredientMatches_TranslatesAndReturnsMatches()
    {
        var queryService = new DrinkQueryService(_db);

        var matches = await queryService.FindFuzzyIngredientMatchesAsync("Tequila");

        var match = matches.ShouldHaveSingleItem();
        match.IngredientName.ShouldBe("Tequila");
        match.Similarity.ShouldBeGreaterThanOrEqualTo(0.30d);
    }
}

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class DrinkQueryServiceDetailTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;
    private Guid _drinkId;

    public async Task InitializeAsync()
    {
        _db = fixture.CreateDbContext();
        var ingredient = Ingredient.Create(IngredientName.Create("Lime Juice"), ["Nellie & Joe's"]);
        _db.Ingredients.Add(ingredient);

        var drink = Drink.Create(DrinkName.Create("Gimlet"), DrinkCategory.Create("Contemporary Classics"), "Gin and lime", "Shake", "Lime", ImageUrl.Create(null));
        drink.SetRecipeEntries([RecipeEntry.Create(drink.Id, ingredient.Id, Quantity.Create("1 oz"), "Fresh")]);
        _db.Drinks.Add(drink);
        _drinkId = drink.Id;
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Tags\"; DELETE FROM drink_catalog.\"Ingredients\";");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task GetDetailById_ReturnsFullDetail()
    {
        var queryService = new DrinkQueryService(_db);
        var detail = await queryService.GetDetailByIdAsync(_drinkId);

        detail.ShouldNotBeNull();
        detail.Name.ShouldBe("Gimlet");
        detail.RecipeEntries.ShouldNotBeEmpty();
        detail.RecipeEntries[0].Ingredient.Name.ShouldBe("Lime Juice");
        detail.RecipeEntries[0].Ingredient.NotableBrands.ShouldContain("Nellie & Joe's");
    }

    [Fact]
    public async Task GetDetailById_NotFound_ReturnsNull()
    {
        var queryService = new DrinkQueryService(_db);
        var detail = await queryService.GetDetailByIdAsync(Guid.NewGuid());
        detail.ShouldBeNull();
    }
}

[CollectionDefinition("Postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>;
