using AlCopilot.DrinkCatalog.Data;
using AlCopilot.DrinkCatalog.Data.Repositories;
using AlCopilot.DrinkCatalog.Domain.Aggregates;
using AlCopilot.DrinkCatalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AlCopilot.DrinkCatalog.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Postgres")]
public sealed class DrinkCrudIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private DrinkCatalogDbContext _db = null!;

    public async Task InitializeAsync() => _db = fixture.CreateDbContext();

    public async Task DisposeAsync()
    {
        await _db.Database.ExecuteSqlRawAsync(
            "DELETE FROM drink_catalog.\"DrinkTag\"; DELETE FROM drink_catalog.\"RecipeEntries\"; DELETE FROM drink_catalog.\"Drinks\"; DELETE FROM drink_catalog.\"Tags\"; DELETE FROM drink_catalog.\"Ingredients\"; DELETE FROM drink_catalog.\"IngredientCategories\"; DELETE FROM drink_catalog.domain_events;");
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task CreateDrink_WithRecipeAndTags_Persists()
    {
        var tag = Tag.Create(TagName.Create("Tiki"));
        _db.Tags.Add(tag);

        var category = IngredientCategory.Create(CategoryName.Create("Spirits"));
        _db.IngredientCategories.Add(category);
        var ingredient = Ingredient.Create(IngredientName.Create("Rum"), category.Id);
        _db.Ingredients.Add(ingredient);
        await _db.SaveChangesAsync();

        var drink = Drink.Create(DrinkName.Create("Mai Tai"), "A tropical cocktail", ImageUrl.Create("https://example.com/mai-tai.jpg"));
        drink.SetTags([tag]);
        drink.SetRecipeEntries([RecipeEntry.Create(drink.Id, ingredient.Id, Quantity.Create("2 oz"), "Dark rum preferred")]);
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        var repo = new DrinkRepository(_db);
        var detail = await repo.GetDetailByIdAsync(drink.Id);
        detail.ShouldNotBeNull();
        detail!.Name.ShouldBe("Mai Tai");
        detail.Tags.ShouldContain(t => t.Name == "Tiki");
        detail.RecipeEntries.ShouldContain(r => r.Ingredient.Name == "Rum");
    }

    [Fact]
    public async Task DuplicateDrinkName_ThrowsOnSave()
    {
        _db.Drinks.Add(Drink.Create(DrinkName.Create("Unique Cocktail"), null, ImageUrl.Create(null)));
        await _db.SaveChangesAsync();

        _db.Drinks.Add(Drink.Create(DrinkName.Create("Unique Cocktail"), null, ImageUrl.Create(null)));
        await Should.ThrowAsync<DbUpdateException>(() => _db.SaveChangesAsync());
    }

    [Fact]
    public async Task UpdateDrink_ChangesDetails()
    {
        var drink = Drink.Create(DrinkName.Create("Original"), "Old desc", ImageUrl.Create(null));
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        drink.Update(DrinkName.Create("Updated"), "New desc", ImageUrl.Create("https://example.com/new.jpg"));
        await _db.SaveChangesAsync();

        var loaded = await _db.Drinks.FindAsync(drink.Id);
        loaded!.Name.ShouldBe(DrinkName.Create("Updated"));
        loaded.Description.ShouldBe("New desc");
    }

    [Fact]
    public async Task UpdateDrink_ReplacesTags()
    {
        var tag1 = Tag.Create(TagName.Create("Old"));
        var tag2 = Tag.Create(TagName.Create("New"));
        _db.Tags.AddRange(tag1, tag2);

        var drink = Drink.Create(DrinkName.Create("TagSwap"), null, ImageUrl.Create(null));
        drink.SetTags([tag1]);
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        drink.SetTags([tag2]);
        await _db.SaveChangesAsync();

        var detail = await new DrinkRepository(_db).GetDetailByIdAsync(drink.Id);
        detail!.Tags.ShouldHaveSingleItem().Name.ShouldBe("New");
    }

    [Fact]
    public async Task UpdateDrink_ReplacesRecipe()
    {
        var category = IngredientCategory.Create(CategoryName.Create("Juices"));
        _db.IngredientCategories.Add(category);
        var ingredient1 = Ingredient.Create(IngredientName.Create("OJ"), category.Id);
        var ingredient2 = Ingredient.Create(IngredientName.Create("Lime"), category.Id);
        _db.Ingredients.AddRange(ingredient1, ingredient2);

        var drink = Drink.Create(DrinkName.Create("RecipeSwap"), null, ImageUrl.Create(null));
        drink.SetRecipeEntries([RecipeEntry.Create(drink.Id, ingredient1.Id, Quantity.Create("1 oz"), null)]);
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        drink.SetRecipeEntries([RecipeEntry.Create(drink.Id, ingredient2.Id, Quantity.Create("2 oz"), "Fresh")]);
        await _db.SaveChangesAsync();

        var detail = await new DrinkRepository(_db).GetDetailByIdAsync(drink.Id);
        detail!.RecipeEntries.ShouldHaveSingleItem().Ingredient.Name.ShouldBe("Lime");
    }

    [Fact]
    public async Task SoftDelete_ExcludesFromQueries()
    {
        var drink = Drink.Create(DrinkName.Create("SoonDeleted"), null, ImageUrl.Create(null));
        _db.Drinks.Add(drink);
        await _db.SaveChangesAsync();

        drink.SoftDelete();
        await _db.SaveChangesAsync();

        var repo = new DrinkRepository(_db);
        var paged = await repo.GetPagedAsync(null, 1, 20);
        paged.Items.ShouldNotContain(d => d.Name == "SoonDeleted");

        var detail = await repo.GetDetailByIdAsync(drink.Id);
        detail.ShouldBeNull();
    }
}
